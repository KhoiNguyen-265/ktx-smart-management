using System.Drawing;
using System.IO;
using ClosedXML.Excel;
using FluentAssertions;
using KTX.Data;
using KTX.Domain.DTOs;
using KTX.Domain.Entities;
using KTX.Domain.Enums;
using KTX.Infrastructure.VietQR;
using KTX.Services.Configuration;
using KTX.Services.Finance;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace KTX.Tests;

public class Phase6Tests
{
    private readonly DbContextOptions<KtxDbContext> _dbOptions;

    public Phase6Tests()
    {
        _dbOptions = new DbContextOptionsBuilder<KtxDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private IDbContextFactory<KtxDbContext> CreateMockFactory()
    {
        var mockFactory = new Mock<IDbContextFactory<KtxDbContext>>();
        mockFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(() => new KtxDbContext(_dbOptions));
        return mockFactory.Object;
    }

    private async Task SeedBaseDataAsync()
    {
        using var db = new KtxDbContext(_dbOptions);

        var date = DateTime.Today.AddYears(-1);

        // Bậc giá điện EVN 6 bậc chuẩn
        db.BacGias.AddRange(
            new BacGia { Id = 1, LoaiDichVu = LoaiDichVu.Dien, Bac = 1, TuMuc = 0, DenMuc = 50, DonGia = 1806m, ApDungTu = date },
            new BacGia { Id = 2, LoaiDichVu = LoaiDichVu.Dien, Bac = 2, TuMuc = 50, DenMuc = 100, DonGia = 1866m, ApDungTu = date },
            new BacGia { Id = 3, LoaiDichVu = LoaiDichVu.Dien, Bac = 3, TuMuc = 100, DenMuc = 200, DonGia = 2167m, ApDungTu = date },
            new BacGia { Id = 4, LoaiDichVu = LoaiDichVu.Dien, Bac = 4, TuMuc = 200, DenMuc = 300, DonGia = 2729m, ApDungTu = date },
            new BacGia { Id = 5, LoaiDichVu = LoaiDichVu.Dien, Bac = 5, TuMuc = 300, DenMuc = 400, DonGia = 3050m, ApDungTu = date },
            new BacGia { Id = 6, LoaiDichVu = LoaiDichVu.Dien, Bac = 6, TuMuc = 400, DenMuc = null, DonGia = 3151m, ApDungTu = date },

            // Bậc giá nước 4 bậc
            new BacGia { Id = 7, LoaiDichVu = LoaiDichVu.Nuoc, Bac = 1, TuMuc = 0, DenMuc = 10, DonGia = 7500m, ApDungTu = date },
            new BacGia { Id = 8, LoaiDichVu = LoaiDichVu.Nuoc, Bac = 2, TuMuc = 10, DenMuc = 20, DonGia = 8800m, ApDungTu = date },
            new BacGia { Id = 9, LoaiDichVu = LoaiDichVu.Nuoc, Bac = 3, TuMuc = 20, DenMuc = 30, DonGia = 12000m, ApDungTu = date },
            new BacGia { Id = 10, LoaiDichVu = LoaiDichVu.Nuoc, Bac = 4, TuMuc = 30, DenMuc = null, DonGia = 24000m, ApDungTu = date }
        );

        var toa = new ToaNha { Id = 1, Ten = "Tòa A", GioiTinh = GioiTinh.Nam, SoTang = 3 };
        db.ToaNhas.Add(toa);

        var lp4 = new LoaiPhong { Id = 1, Ten = "Phòng 4", SucChua = 4, GiaThang = 800000m };
        db.LoaiPhongs.Add(lp4);

        var p1 = new Phong { Id = 1, MaPhong = "A101", ToaId = 1, ToaNha = toa, Tang = 1, LoaiPhongId = 1, LoaiPhong = lp4, TrangThai = TrangThaiPhong.HoatDong };
        db.Phongs.Add(p1);

        var tinh = new TinhThanh { Id = 1, Ten = "Hà Nội", KhoangCachKm = 10, NhomKhoangCach = 1 };
        db.TinhThanhs.Add(tinh);

        var sv = new SinhVien { MSSV = "20261111", HoTen = "Lê Tài Chính", GioiTinh = GioiTinh.Nam, CCCD = "001205111111", Khoa = "CNTT", KhoaHoc = 68, SoDienThoai = "0988888888", Email = "sv@sis.edu.vn", TinhThanhId = 1 };
        db.SinhViens.Add(sv);

        var hd = new HopDong
        {
            Id = 1,
            MSSV = "20261111",
            PhongId = 1,
            NgayBatDau = DateTime.Today.AddMonths(-1),
            NgayKetThuc = DateTime.Today.AddMonths(5),
            GiaThang = 800000m,
            TienCoc = 1000000m,
            MienGiamPhanTram = 0,
            TrangThai = TrangThaiHopDong.HieuLuc
        };
        db.HopDongs.Add(hd);

        await db.SaveChangesAsync();
    }

    [Fact]
    public void VietQR_BuildEmvPayload_ShouldGenerateValidOfflineStandardPayload()
    {
        // Act
        string payload = VietQRHelper.BuildEmvCoString(
            bankBin: "970422",
            accountNumber: "999988886666",
            amount: 540000m,
            paymentReference: "KTXDN000001");

        // Assert
        payload.Should().NotBeNullOrEmpty();
        payload.Should().StartWith("000201010212"); // Payload format indicator + Dynamic QR
        payload.Should().Contain("A000000727"); // NAPAS GUID
        payload.Should().Contain("970422"); // Bank BIN (MBBank)
        payload.Should().Contain("999988886666"); // Account number
        payload.Should().Contain("5303704"); // Currency VND (704)
        payload.Should().Contain("5406540000"); // Amount tag + length + value
        payload.Should().Contain("KTXDN000001"); // Payment reference in tag 62
        payload.Should().Contain("6304"); // CRC tag

        // Generate QR Image
        using var bmp = VietQRHelper.GenerateQrBitmap(payload, 8);
        bmp.Should().NotBeNull();
        bmp.Width.Should().BeGreaterThan(100);
        bmp.Height.Should().BeGreaterThan(100);
    }

    [Fact]
    public async Task TieredBilling_CalculateTieredCosts_MultiPersonQuotaAndMeterReplacement()
    {
        // Arrange
        await SeedBaseDataAsync();
        var mockFactory = CreateMockFactory();

        var mockConfigService = new Mock<IConfigurationService>();
        mockConfigService.Setup(c => c.GetSystemConfigAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SystemConfigDto
            {
                BankName = "MBBank",
                BankAccountNumber = "999988886666",
                BankAccountHolder = "BAN QUAN LY KTX",
                BankBin = "970422",
                VatRate = 0.10m
            });

        var billingService = new TieredBillingService(mockFactory, mockConfigService.Object);

        // Act 1: 4 people room, consumption 120 kWh electricity, 15 m3 water
        var resultNormal = await billingService.CalculateBillPreviewAsync(
            sanLuongDien: 120,
            sanLuongNuoc: 15,
            soNguoiDinhMuc: 4);

        // Assert 1:
        // SanLuongDien = 120. Với m=1 (4 người):
        // Bậc 1 (50 kWh): 50 * 1806 = 90,300
        // Bậc 2 (50 kWh): 50 * 1866 = 93,300
        // Bậc 3 (20 kWh): 20 * 2167 = 43,340
        // Tổng tiền điện = 226,940
        resultNormal.SanLuongDien.Should().Be(120m);
        resultNormal.SanLuongNuoc.Should().Be(15m);
        resultNormal.HeSoDinhMuc.Should().Be(1);
        resultNormal.TienDien.Should().Be(226940m);

        // Nước 15m3: Bậc 1 (10m3 * 7500 = 75000), Bậc 2 (5m3 * 8800 = 44000) => 119,000
        resultNormal.TienNuoc.Should().Be(119000m);
        resultNormal.PhiDichVuChung.Should().Be(50000m);
        // VAT = 10% * (226,940 + 119,000 + 50,000) = 39,594
        resultNormal.TienVat.Should().Be(39594m);
        resultNormal.TongCong.Should().Be(226940m + 119000m + 50000m + 39594m);

        // Act 2: Meter Replacement (Thay đồng hồ)
        // Cũ: từ 1500 chạy đến 1530 chốt tháo ra (= 30 kWh). Đồng hồ mới từ 0 chạy đến 20 (= 20 kWh). Tổng = 50 kWh.
        await billingService.SaveReadingAsync(new UtilityReadingSaveDto
        {
            PhongId = 1,
            Thang = 10,
            Nam = 2026,
            DienCu = 1500,
            DienMoi = 20,
            CoThayDongHoDien = true,
            DienChotDongHoCu = 1530,
            DienDauDongHoMoi = 0,
            NuocCu = 10,
            NuocMoi = 15,
            GhiChu = "Thay đồng hồ điện bị hỏng đĩa quay"
        });

        var readings = await billingService.GetReadingsAsync(10, 2026);
        readings.Should().ContainSingle();
        var reading = readings.First();
        reading.SanLuongDien.Should().Be(50m);
    }

    [Fact]
    public async Task TieredBilling_BatchGenerateMonthlyInvoices_ShouldCreateInvoicesWithCorrectCodes()
    {
        // Arrange
        await SeedBaseDataAsync();
        var mockFactory = CreateMockFactory();

        var mockConfigService = new Mock<IConfigurationService>();
        mockConfigService.Setup(c => c.GetSystemConfigAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SystemConfigDto
            {
                BankName = "MBBank",
                BankAccountNumber = "999988886666",
                BankAccountHolder = "BAN QUAN LY KTX",
                BankBin = "970422",
                VatRate = 0.10m
            });

        var billingService = new TieredBillingService(mockFactory, mockConfigService.Object);

        // Lưu chỉ số điện nước tháng 10/2026
        await billingService.SaveReadingAsync(new UtilityReadingSaveDto
        {
            PhongId = 1,
            Thang = 10,
            Nam = 2026,
            DienCu = 100,
            DienMoi = 250, // 150 kWh
            NuocCu = 50,
            NuocMoi = 70   // 20 m3
        });

        // Act: Lập hóa đơn hàng loạt
        var generatedInvoices = await billingService.GenerateMonthlyInvoicesAsync(10, 2026);

        // Assert
        generatedInvoices.Should().HaveCount(1);

        var invoices = await billingService.GetInvoicesAsync(10, 2026);
        invoices.Should().HaveCount(1);

        var inv = invoices.First();
        inv.MaThanhToan.Should().StartWith("KTXDN");
        inv.PhongId.Should().Be(1);
        inv.MaPhong.Should().Be("A101");
        inv.TongTien.Should().BeGreaterThan(0);
        inv.VietQrPayload.Should().Contain("KTXDN");
        inv.TrangThai.Should().Be(TrangThaiHoaDon.ChuaThanhToan);
    }

    [Fact]
    public async Task Reconciliation_DoubleHashDeduplication_ShouldRejectDuplicateBankTransactions()
    {
        // Arrange
        await SeedBaseDataAsync();
        var mockFactory = CreateMockFactory();

        var mockConfigService = new Mock<IConfigurationService>();
        mockConfigService.Setup(c => c.GetSystemConfigAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SystemConfigDto
            {
                BankName = "MBBank",
                BankAccountNumber = "999988886666",
                BankAccountHolder = "BAN QUAN LY KTX",
                BankBin = "970422",
                VatRate = 0.10m
            });

        var billingService = new TieredBillingService(mockFactory, mockConfigService.Object);
        var reconciliationService = new ReconciliationService(mockFactory);

        // 1. Tạo chỉ số & lập hóa đơn tháng 10/2026
        await billingService.SaveReadingAsync(new UtilityReadingSaveDto
        {
            PhongId = 1,
            Thang = 10,
            Nam = 2026,
            DienCu = 100,
            DienMoi = 200,
            NuocCu = 10,
            NuocMoi = 20
        });
        await billingService.GenerateMonthlyInvoicesAsync(10, 2026);
        var invoices = await billingService.GetInvoicesAsync(10, 2026);
        var inv = invoices.First();

        // 2. Tự động sinh file Excel mẫu sao kê khớp với hóa đơn trên
        byte[] excelBytes = await reconciliationService.GenerateBankStatementSampleExcelAsync();
        excelBytes.Should().NotBeNull();
        excelBytes.Length.Should().BeGreaterThan(1000);

        // 3. Import lần 1: Phải khớp thành công (file mẫu chứa 1 dòng trùng lặp có chủ đích để kiểm thử)
        using var stream1 = new MemoryStream(excelBytes);
        var result1 = await reconciliationService.ImportAndReconcileExcelAsync(stream1, "SaoKe_Test.xlsx");

        result1.TotalRows.Should().BeGreaterThan(0);
        result1.MatchedCount.Should().BeGreaterThan(0);
        result1.DuplicateCount.Should().Be(1); // File mẫu cố ý có 1 dòng lặp để demo chống trùng trong file!

        // Kiểm tra hóa đơn sau khi đối soát lần 1
        var invoicesAfter = await billingService.GetInvoicesAsync(10, 2026);
        var invAfter = invoicesAfter.First();
        invAfter.TrangThai.Should().Be(TrangThaiHoaDon.DaThanhToan);
        invAfter.ConNo.Should().Be(0);

        // 4. Import lần 2 (cùng file sao kê đó một lần nữa) - THỬ THÁCH CHỐNG TRÙNG LẶP 2 LỚP
        using var stream2 = new MemoryStream(excelBytes);
        var result2 = await reconciliationService.ImportAndReconcileExcelAsync(stream2, "SaoKe_Test_LapLai.xlsx");

        // Assert: Lần 2 tất cả các dòng phải bị chặn bởi cơ chế Hash/MaGiaoDich!
        result2.DuplicateCount.Should().Be(result1.TotalRows);
        result2.MatchedCount.Should().Be(0);
        result2.TotalAmount.Should().Be(0); // Không cộng dồn doanh thu thêm lần nào nữa!
    }
}

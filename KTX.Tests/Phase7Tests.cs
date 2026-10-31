using System.Text;
using FluentAssertions;
using KTX.Data;
using KTX.Domain.DTOs;
using KTX.Domain.Entities;
using KTX.Domain.Enums;
using KTX.Infrastructure.Cccd;
using KTX.Services.Dashboard;
using KTX.Services.Facility;
using KTX.Services.Reporting;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace KTX.Tests;

public class Phase7Tests
{
    private readonly DbContextOptions<KtxDbContext> _dbOptions;

    public Phase7Tests()
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

        var toa = new ToaNha { Id = 1, Ten = "Tòa A", GioiTinh = GioiTinh.Nam, SoTang = 3 };
        db.ToaNhas.Add(toa);

        var lp4 = new LoaiPhong { Id = 1, Ten = "Phòng 4", SucChua = 4, GiaThang = 800000m };
        db.LoaiPhongs.Add(lp4);

        var p1 = new Phong { Id = 1, MaPhong = "A101", ToaId = 1, ToaNha = toa, Tang = 1, LoaiPhongId = 1, LoaiPhong = lp4, TrangThai = TrangThaiPhong.HoatDong };
        var p2 = new Phong { Id = 2, MaPhong = "A102", ToaId = 1, ToaNha = toa, Tang = 1, LoaiPhongId = 1, LoaiPhong = lp4, TrangThai = TrangThaiPhong.HoatDong };
        db.Phongs.AddRange(p1, p2);

        var tinh = new TinhThanh { Id = 1, Ten = "Hà Nội", KhoangCachKm = 10, NhomKhoangCach = 1 };
        db.TinhThanhs.Add(tinh);

        var sv = new SinhVien { MSSV = "20267777", HoTen = "Trần Báo Cáo", GioiTinh = GioiTinh.Nam, CCCD = "001205777777", Khoa = "CNTT", KhoaHoc = 68, SoDienThoai = "0977777777", Email = "sv@sis.edu.vn", TinhThanhId = 1 };
        db.SinhViens.Add(sv);

        var hd = new HopDong
        {
            Id = 1,
            MSSV = "20267777",
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
    public void CccdScanner_ParseCccdQrString_ShouldExtractAllFieldsAccurately()
    {
        // Chuỗi QR chuẩn Bộ Công An: Nam giới, không có CMND cũ
        string raw1 = "001205001234||Nguyễn Văn An|15082005|Nam|Thôn 1, Xã Hoằng Lộc, Huyện Hoằng Hóa, Tỉnh Thanh Hóa|01012023";
        var info1 = CccdScannerHelper.ParseCccdQrString(raw1);

        info1.Should().NotBeNull();
        info1!.CCCD.Should().Be("001205001234");
        info1.CMNDCu.Should().BeNull();
        info1.HoTen.Should().Be("Nguyễn Văn An");
        info1.GioiTinh.Should().Be(GioiTinh.Nam);
        info1.NgaySinh.Should().Be(new DateTime(2005, 8, 15));
        info1.TenTinhThanhNhanDien.Should().Be("Thanh Hóa");

        // Chuỗi QR chuẩn: Nữ giới, có số CMND cũ 9 số
        string raw2 = "079305009999|025689123|TRẦN THỊ HỒNG|20102005|Nữ|Số 12 Lê Lợi, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh|10102022";
        var info2 = CccdScannerHelper.ParseCccdQrString(raw2);

        info2.Should().NotBeNull();
        info2!.CCCD.Should().Be("079305009999");
        info2.CMNDCu.Should().Be("025689123");
        info2.HoTen.Should().Be("Trần Thị Hồng");
        info2.GioiTinh.Should().Be(GioiTinh.Nu);
        info2.NgaySinh.Should().Be(new DateTime(2005, 10, 20));
        info2.TenTinhThanhNhanDien.Should().Be("Hồ Chí Minh");
    }

    [Fact]
    public async Task MaintenanceService_LifecycleAndStatistics()
    {
        // Arrange
        await SeedBaseDataAsync();
        var mockFactory = CreateMockFactory();
        var service = new MaintenanceService(mockFactory);

        // 1. Tạo yêu cầu sửa chữa
        var created = await service.CreateRequestAsync(new MaintenanceCreateDto
        {
            PhongId = 1,
            MoTa = "Hỏng vòi sen nhà tắm phòng A101",
            GhiChu = "Cần thay ron cao su"
        });

        created.Should().NotBeNull();
        created.TrangThai.Should().Be("ChuaXuLy");
        created.MaPhong.Should().Be("A101");

        // 2. Kiểm tra danh sách & thống kê
        var initialStats = await service.GetStatsAsync();
        initialStats.TotalRequests.Should().Be(1);
        initialStats.PendingCount.Should().Be(1);
        initialStats.CompletedCount.Should().Be(0);

        // 3. Cập nhật tiến độ hoàn thành
        bool updated = await service.UpdateRequestStatusAsync(new MaintenanceStatusUpdateDto
        {
            Id = created.Id,
            TrangThai = "HoanThanh",
            ChiPhi = 150000m,
            GhiChu = "Đã thay vòi xịt mới"
        });

        updated.Should().BeTrue();

        // 4. Kiểm tra lại thống kê sau hoàn thành
        var finalStats = await service.GetStatsAsync();
        finalStats.PendingCount.Should().Be(0);
        finalStats.CompletedCount.Should().Be(1);
        finalStats.TotalCost.Should().Be(150000m);
    }

    [Fact]
    public async Task DashboardService_ShouldReturnConsistentKpiAndOccupancy()
    {
        // Arrange
        await SeedBaseDataAsync();
        var mockFactory = CreateMockFactory();
        var service = new DashboardService(mockFactory);

        // Act
        var stats = await service.GetDashboardStatsAsync();

        // Assert
        stats.TotalRooms.Should().Be(2);
        stats.TotalBeds.Should().Be(8); // 2 phòng * 4 giường
        stats.OccupiedBeds.Should().Be(1); // 1 hợp đồng
        stats.ActiveContracts.Should().Be(1);
        stats.OccupancyRate.Should().Be(12.5); // 1/8 * 100%
        stats.Buildings.Should().HaveCount(1);
        stats.Buildings[0].BuildingName.Should().Be("Tòa A");
        stats.Buildings[0].OccupiedBeds.Should().Be(1);
        stats.Buildings[0].AvailableBeds.Should().Be(7);
        stats.RevenueHistory.Should().HaveCount(6);
    }

    [Fact]
    public async Task ReportingService_ExportOccupancyExcelAndFinancialPdf()
    {
        // Arrange
        await SeedBaseDataAsync();
        var mockFactory = CreateMockFactory();
        var service = new ReportingService(mockFactory);

        // 1. Export Excel
        byte[] excelBytes = await service.ExportOccupancyReportExcelAsync();
        excelBytes.Should().NotBeNull();
        excelBytes.Length.Should().BeGreaterThan(1000);

        // 2. Export PDF
        byte[] pdfBytes = await service.ExportFinancialReportPdfAsync(DateTime.Now.Month, DateTime.Now.Year);
        pdfBytes.Should().NotBeNull();
        pdfBytes.Length.Should().BeGreaterThan(1000);

        // PDF file must start with header %PDF
        string pdfHeader = Encoding.ASCII.GetString(pdfBytes, 0, 4);
        pdfHeader.Should().Be("%PDF");
    }
}

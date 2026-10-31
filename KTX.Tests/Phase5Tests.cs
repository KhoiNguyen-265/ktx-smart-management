using FluentAssertions;
using KTX.Data;
using KTX.Domain.DTOs;
using KTX.Domain.Entities;
using KTX.Domain.Enums;
using KTX.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace KTX.Tests;

public class Phase5Tests
{
    private readonly DbContextOptions<KtxDbContext> _dbOptions;

    public Phase5Tests()
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

        var bNam = new ToaNha { Id = 1, Ten = "Tòa A (Nam)", GioiTinh = GioiTinh.Nam, SoTang = 3 };
        var bNu = new ToaNha { Id = 2, Ten = "Tòa B (Nữ)", GioiTinh = GioiTinh.Nu, SoTang = 3 };
        db.ToaNhas.AddRange(bNam, bNu);

        var lp4 = new LoaiPhong { Id = 1, Ten = "Phòng 4", SucChua = 4, GiaThang = 800000m };
        db.LoaiPhongs.Add(lp4);

        var p1 = new Phong { Id = 1, MaPhong = "A101", ToaId = 1, ToaNha = bNam, Tang = 1, LoaiPhongId = 1, LoaiPhong = lp4, TrangThai = TrangThaiPhong.HoatDong };
        var p2 = new Phong { Id = 2, MaPhong = "A102", ToaId = 1, ToaNha = bNam, Tang = 1, LoaiPhongId = 1, LoaiPhong = lp4, TrangThai = TrangThaiPhong.HoatDong };
        var pNu = new Phong { Id = 3, MaPhong = "B101", ToaId = 2, ToaNha = bNu, Tang = 1, LoaiPhongId = 1, LoaiPhong = lp4, TrangThai = TrangThaiPhong.HoatDong };
        db.Phongs.AddRange(p1, p2, pNu);

        var tinh = new TinhThanh { Id = 1, Ten = "Hà Nội", KhoangCachKm = 10, NhomKhoangCach = 1 };
        db.TinhThanhs.Add(tinh);

        var sv = new SinhVien
        {
            MSSV = "20265001",
            HoTen = "Nguyễn Hợp Đồng",
            GioiTinh = GioiTinh.Nam,
            CCCD = "001205009999",
            Khoa = "CNTT",
            KhoaHoc = 68,
            SoDienThoai = "0912345678",
            Email = "sv@sis.edu.vn",
            TinhThanhId = 1
        };
        db.SinhViens.Add(sv);

        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateContractAsync_ShouldEnforceUniqueActiveContractAndGender()
    {
        await SeedBaseDataAsync();
        var factory = CreateMockFactory();
        var service = new ContractService(factory);

        // 1. Thử tạo hợp đồng vào phòng Nữ (B101) cho sinh viên Nam -> Phải từ chối do lệch giới tính
        var failGender = await service.CreateContractAsync(new ContractCreateDto
        {
            MSSV = "20265001",
            PhongId = 3, // B101 Nữ
            NgayBatDau = DateTime.Now,
            NgayKetThuc = DateTime.Now.AddMonths(5),
            GiaThang = 800000m,
            TienCoc = 800000m
        });
        failGender.Success.Should().BeFalse();
        failGender.Message.Should().Contain("Giới tính");

        // 2. Tạo hợp đồng hợp lệ vào phòng Nam (A101) -> Thành công
        var success = await service.CreateContractAsync(new ContractCreateDto
        {
            MSSV = "20265001",
            PhongId = 1, // A101 Nam
            NgayBatDau = DateTime.Now,
            NgayKetThuc = DateTime.Now.AddMonths(5),
            GiaThang = 800000m,
            TienCoc = 800000m
        });
        success.Success.Should().BeTrue();

        // 3. Thử tạo tiếp hợp đồng thứ 2 khi hợp đồng 1 đang hiệu lực -> Phải từ chối
        var failDuplicate = await service.CreateContractAsync(new ContractCreateDto
        {
            MSSV = "20265001",
            PhongId = 2,
            NgayBatDau = DateTime.Now,
            NgayKetThuc = DateTime.Now.AddMonths(5),
            GiaThang = 800000m,
            TienCoc = 800000m
        });
        failDuplicate.Success.Should().BeFalse();
        failDuplicate.Message.Should().Contain("hiệu lực");
    }

    [Fact]
    public async Task TransferRoomAsync_ShouldUpdateRoomAndLogHistory()
    {
        await SeedBaseDataAsync();
        var factory = CreateMockFactory();
        var service = new ContractService(factory);

        // Tạo hợp đồng ở phòng A101
        await service.CreateContractAsync(new ContractCreateDto
        {
            MSSV = "20265001",
            PhongId = 1,
            NgayBatDau = DateTime.Now,
            NgayKetThuc = DateTime.Now.AddMonths(5),
            GiaThang = 800000m,
            TienCoc = 800000m
        });

        using var db = new KtxDbContext(_dbOptions);
        var contract = await db.HopDongs.FirstAsync(h => h.MSSV == "20265001");

        // Act: Chuyển sang phòng A102
        var transferResult = await service.TransferRoomAsync(new RoomTransferRequestDto
        {
            HopDongId = contract.Id,
            DenPhongId = 2,
            LyDo = "Ở cùng bạn cùng khoa"
        });

        // Assert
        transferResult.Success.Should().BeTrue();

        using var verifyDb = new KtxDbContext(_dbOptions);
        var updated = await verifyDb.HopDongs.Include(h => h.LichSuChuyenPhongs).FirstAsync(h => h.Id == contract.Id);
        updated.PhongId.Should().Be(2); // Đã sang A102
        updated.LichSuChuyenPhongs.Should().HaveCount(1);
        updated.LichSuChuyenPhongs.First().TuPhongId.Should().Be(1);
        updated.LichSuChuyenPhongs.First().DenPhongId.Should().Be(2);
        updated.LichSuChuyenPhongs.First().LyDo.Should().Be("Ở cùng bạn cùng khoa");
    }

    [Fact]
    public async Task CheckOutContractAsync_ShouldDeductDamageAndRefundDeposit()
    {
        await SeedBaseDataAsync();
        var factory = CreateMockFactory();
        var service = new ContractService(factory);

        // Tạo hợp đồng với cọc 800.000 đ
        await service.CreateContractAsync(new ContractCreateDto
        {
            MSSV = "20265001",
            PhongId = 1,
            NgayBatDau = DateTime.Now,
            NgayKetThuc = DateTime.Now.AddMonths(5),
            GiaThang = 800000m,
            TienCoc = 800000m
        });

        using var db = new KtxDbContext(_dbOptions);
        var contract = await db.HopDongs.FirstAsync(h => h.MSSV == "20265001");

        // Act: Trả phòng với tiền khấu trừ hỏng hóc 150.000 đ
        var checkOut = await service.CheckOutContractAsync(new ContractCheckOutDto
        {
            HopDongId = contract.Id,
            TienHongHocTaiSan = 150000m,
            TienNoDienNuocKhauTru = 50000m
        });

        // Assert
        checkOut.Success.Should().BeTrue();
        checkOut.TienCocBanDau.Should().Be(800000m);
        checkOut.TongKhauTru.Should().Be(200000m);
        checkOut.TienHoanTra.Should().Be(600000m); // 800k - 200k = 600k

        using var verifyDb = new KtxDbContext(_dbOptions);
        var updated = await verifyDb.HopDongs.FirstAsync(h => h.Id == contract.Id);
        updated.TrangThai.Should().Be(TrangThaiHopDong.DaTra);
    }

    [Fact]
    public async Task ExportContractPdfAsync_ShouldGenerateValidPdfDocument()
    {
        await SeedBaseDataAsync();
        var factory = CreateMockFactory();
        var service = new ContractService(factory);

        await service.CreateContractAsync(new ContractCreateDto
        {
            MSSV = "20265001",
            PhongId = 1,
            NgayBatDau = DateTime.Now,
            NgayKetThuc = DateTime.Now.AddMonths(5),
            GiaThang = 800000m,
            TienCoc = 800000m
        });

        using var db = new KtxDbContext(_dbOptions);
        var contract = await db.HopDongs.FirstAsync(h => h.MSSV == "20265001");

        // Act
        var pdfBytes = await service.ExportContractPdfAsync(contract.Id);

        // Assert
        pdfBytes.Should().NotBeNull();
        pdfBytes.Length.Should().BeGreaterThan(1000); // Standard PDF binary
        // Check PDF magic header %PDF
        pdfBytes[0].Should().Be((byte)'%');
        pdfBytes[1].Should().Be((byte)'P');
        pdfBytes[2].Should().Be((byte)'D');
        pdfBytes[3].Should().Be((byte)'F');
    }
}

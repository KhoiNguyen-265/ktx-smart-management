using ClosedXML.Excel;
using FluentAssertions;
using KTX.Data;
using KTX.Domain.DTOs;
using KTX.Domain.Entities;
using KTX.Domain.Enums;
using KTX.Services.Configuration;
using KTX.Services.Students;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace KTX.Tests;

public class Phase3Tests
{
    private readonly DbContextOptions<KtxDbContext> _dbOptions;

    public Phase3Tests()
    {
        _dbOptions = new DbContextOptionsBuilder<KtxDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private IDbContextFactory<KtxDbContext> CreateMockFactory(KtxDbContext db)
    {
        var mockFactory = new Mock<IDbContextFactory<KtxDbContext>>();
        mockFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(() => new KtxDbContext(_dbOptions));
        return mockFactory.Object;
    }

    [Fact]
    public async Task GenerateStudentTemplateAsync_ShouldCreateValidExcelWorkbook()
    {
        // Arrange
        using (var db = new KtxDbContext(_dbOptions))
        {
            db.TinhThanhs.Add(new TinhThanh { Id = 1, Ten = "Hà Nội", KhoangCachKm = 0, NhomKhoangCach = 1 });
            db.TinhThanhs.Add(new TinhThanh { Id = 2, Ten = "Hải Phòng", KhoangCachKm = 105, NhomKhoangCach = 2 });
            db.DienUuTiens.Add(new DienUuTien { Id = 1, Ten = "Hộ nghèo", DiemCong = 20 });
            await db.SaveChangesAsync();
        }

        using var dbContext = new KtxDbContext(_dbOptions);
        var factory = CreateMockFactory(dbContext);
        var service = new StudentService(factory);

        // Act
        var bytes = await service.GenerateStudentTemplateAsync();

        // Assert
        bytes.Should().NotBeNull();
        bytes.Length.Should().BeGreaterThan(0);

        using var ms = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(ms);

        workbook.Worksheets.Count.Should().Be(3);
        workbook.Worksheet("NhapSinhVien").Should().NotBeNull();
        workbook.Worksheet("DanhMuc_TinhThanh").Should().NotBeNull();
        workbook.Worksheet("DanhMuc_DienUuTien").Should().NotBeNull();
    }

    [Fact]
    public async Task ImportStudentsFromExcelAsync_ShouldHandleOldProvincesAndValidation()
    {
        // Arrange: Seed Hà Nội and TinhThanhCu "Hà Tây"
        using (var db = new KtxDbContext(_dbOptions))
        {
            var hanoi = new TinhThanh { Id = 1, Ten = "Hà Nội", KhoangCachKm = 0, NhomKhoangCach = 1 };
            db.TinhThanhs.Add(hanoi);
            db.TinhThanhCus.Add(new TinhThanhCu { Id = 1, TenCu = "Hà Tây", TinhThanhId = 1, TinhThanh = hanoi });
            db.DienUuTiens.Add(new DienUuTien { Id = 1, Ten = "Con thương binh", DiemCong = 30 });
            await db.SaveChangesAsync();
        }

        // Tạo Excel file trong memory để test import
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("NhapSinhVien");

        // Headers
        ws.Cell(1, 1).Value = "MSSV";
        ws.Cell(1, 2).Value = "Họ tên";
        ws.Cell(1, 3).Value = "Ngày sinh";
        ws.Cell(1, 4).Value = "Giới tính";
        ws.Cell(1, 5).Value = "CCCD";
        ws.Cell(1, 6).Value = "Khoa";
        ws.Cell(1, 7).Value = "Khóa";
        ws.Cell(1, 8).Value = "SĐT";
        ws.Cell(1, 9).Value = "Email";
        ws.Cell(1, 10).Value = "Tỉnh thành";
        ws.Cell(1, 11).Value = "Diện ưu tiên";

        // Dòng 2: Hợp lệ, dùng tên tỉnh cũ "Hà Tây" -> Mong đợi ánh xạ sang Hà Nội (Id = 1)
        ws.Cell(2, 1).Value = "20261011";
        ws.Cell(2, 2).Value = "Nguyễn Văn Test";
        ws.Cell(2, 3).Value = "10/05/2005";
        ws.Cell(2, 4).Value = "Nam";
        ws.Cell(2, 5).Value = "001205001122";
        ws.Cell(2, 6).Value = "CNTT";
        ws.Cell(2, 7).Value = 67;
        ws.Cell(2, 8).Value = "0987654321";
        ws.Cell(2, 9).Value = "test@edu.vn";
        ws.Cell(2, 10).Value = "Hà Tây"; // Old province
        ws.Cell(2, 11).Value = "Con thương binh";

        // Dòng 3: Lỗi do CCCD không đủ 12 số
        ws.Cell(3, 1).Value = "20261012";
        ws.Cell(3, 2).Value = "Lê Thị Lỗi";
        ws.Cell(3, 3).Value = "12/08/2005";
        ws.Cell(3, 4).Value = "Nữ";
        ws.Cell(3, 5).Value = "12345"; // Invalid CCCD
        ws.Cell(3, 6).Value = "Kinh Tế";
        ws.Cell(3, 7).Value = 67;
        ws.Cell(3, 8).Value = "0987654322";
        ws.Cell(3, 9).Value = "loi@edu.vn";
        ws.Cell(3, 10).Value = "Hà Nội";
        ws.Cell(3, 11).Value = "";

        using var excelMs = new MemoryStream();
        workbook.SaveAs(excelMs);
        excelMs.Position = 0;

        using var dbContext = new KtxDbContext(_dbOptions);
        var factory = CreateMockFactory(dbContext);
        var service = new StudentService(factory);

        // Act
        var result = await service.ImportStudentsFromExcelAsync(excelMs);

        // Assert
        result.TotalRows.Should().Be(2);
        result.SuccessCount.Should().Be(1);
        result.FailedCount.Should().Be(1);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].RowIndex.Should().Be(3);
        result.Errors[0].ErrorMessage.Should().Contain("12 chữ số");

        // Verify student saved in DB with mapped province Id = 1 (Hà Nội)
        using var verifyDb = new KtxDbContext(_dbOptions);
        var sv = await verifyDb.SinhViens.FirstOrDefaultAsync(s => s.MSSV == "20261011");
        sv.Should().NotBeNull();
        sv!.TinhThanhId.Should().Be(1);
        sv.DienUuTienId.Should().Be(1);
    }

    [Fact]
    public async Task ConfigurationService_ShouldSaveAndRetrieveConfigs()
    {
        // Arrange
        using var dbContext = new KtxDbContext(_dbOptions);
        var factory = CreateMockFactory(dbContext);
        var service = new ConfigurationService(factory);

        var configToSave = new SystemConfigDto
        {
            WPriority = 0.40,
            WDistance = 0.20,
            WCourse = 0.20,
            WPreference = 0.10,
            WRoommate = 0.10,
            VatRate = 0.10m,
            BankName = "MBBank",
            BankBin = "970422",
            BankAccountNumber = "0987654321098",
            BankAccountHolder = "KTX TEST DHQG"
        };

        // Act
        var saveResult = await service.SaveSystemConfigAsync(configToSave);
        var retrieved = await service.GetSystemConfigAsync();

        // Assert
        saveResult.Should().BeTrue();
        retrieved.WPriority.Should().Be(0.40);
        retrieved.WDistance.Should().Be(0.20);
        retrieved.BankName.Should().Be("MBBank");
        retrieved.BankBin.Should().Be("970422");
        retrieved.BankAccountNumber.Should().Be("0987654321098");
        retrieved.BankAccountHolder.Should().Be("KTX TEST DHQG");
        retrieved.VatRate.Should().Be(0.10m);
    }
}

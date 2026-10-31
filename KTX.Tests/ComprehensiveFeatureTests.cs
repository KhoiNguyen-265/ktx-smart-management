using ClosedXML.Excel;
using FluentAssertions;
using KTX.Data;
using KTX.Data.Seed;
using KTX.Domain.DTOs;
using KTX.Domain.Entities;
using KTX.Domain.Enums;
using KTX.Services.Allocation;
using KTX.Services.Auth;
using KTX.Services.Configuration;
using KTX.Services.Contracts;
using KTX.Services.Dashboard;
using KTX.Services.Facility;
using KTX.Services.Finance;
using KTX.Services.Registration;
using KTX.Services.Reporting;
using KTX.Services.Students;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace KTX.Tests;

public class ComprehensiveFeatureTests
{
    private readonly DbContextOptions<KtxDbContext> _dbOptions;

    public ComprehensiveFeatureTests()
    {
        _dbOptions = new DbContextOptionsBuilder<KtxDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private IDbContextFactory<KtxDbContext> CreateMockFactory()
    {
        var mock = new Mock<IDbContextFactory<KtxDbContext>>();
        mock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new KtxDbContext(_dbOptions));
        return mock.Object;
    }

    [Fact]
    public async Task Seeder_ShouldPopulateCompleteDataset_WithoutErrors()
    {
        // Arrange
        using var db = new KtxDbContext(_dbOptions);

        // Act: Run full database seeder
        await DatabaseSeeder.SeedAsync(db);

        // Assert: Verify all tables have substantial, realistic data
        (await db.VaiTros.CountAsync()).Should().Be(3);
        (await db.NguoiDungs.CountAsync()).Should().Be(3);
        (await db.ToaNhas.CountAsync()).Should().Be(4);
        (await db.LoaiPhongs.CountAsync()).Should().Be(3);
        (await db.Phongs.CountAsync()).Should().Be(120);
        (await db.SinhViens.CountAsync()).Should().Be(600);
        (await db.DonDangKys.CountAsync()).Should().Be(500);
        (await db.HopDongs.CountAsync()).Should().BeGreaterThan(400);
        (await db.ChiSoDienNuocs.CountAsync()).Should().BeGreaterThanOrEqualTo(60);
        (await db.YeuCauSuaChuas.CountAsync()).Should().Be(8);
        (await db.HoaDonDienNuocs.CountAsync()).Should().BeGreaterThan(100);
        (await db.KhoanThuTienPhongs.CountAsync()).Should().BeGreaterThan(500);
        (await db.GiaoDichNganHangs.CountAsync()).Should().BeGreaterThanOrEqualTo(20);
    }

    [Fact]
    public async Task RoomService_GetBuildingsAndFloorRooms_ShouldReturnAccurateOccupancy()
    {
        // Arrange
        using (var db = new KtxDbContext(_dbOptions))
        {
            await DatabaseSeeder.SeedAsync(db);
        }

        var factory = CreateMockFactory();
        var roomService = new RoomService(factory);

        // Act: Get buildings
        var buildings = await roomService.GetBuildingsAsync();
        buildings.Should().HaveCount(4);
        buildings.All(b => b.TongSoPhong == 30).Should().BeTrue();

        // Act: Get rooms for Building 1, Floor 1
        var floor1Rooms = await roomService.GetRoomsByBuildingAndFloorAsync(buildings[0].Id, 1);
        floor1Rooms.Should().NotBeEmpty();
        floor1Rooms.All(r => r.Tang == 1).Should().BeTrue();

        // Act: Get room detail
        int testRoomId = floor1Rooms[0].Id;
        var detail = await roomService.GetRoomDetailAsync(testRoomId);
        detail.Should().NotBeNull();
        detail!.Summary.MaPhong.Should().Be(floor1Rooms[0].MaPhong);

        // Act: Toggle room status to BaoTri
        bool toggleResult = await roomService.UpdateRoomStatusAsync(testRoomId, isUnderMaintenance: true);
        toggleResult.Should().BeTrue();

        var updatedDetail = await roomService.GetRoomDetailAsync(testRoomId);
        updatedDetail!.Summary.TrangThai.Should().Be(TrangThaiPhong.BaoTri);
    }

    [Fact]
    public async Task StudentService_PagedSearchAndExcelExport_ShouldSucceed()
    {
        // Arrange
        using (var db = new KtxDbContext(_dbOptions))
        {
            await DatabaseSeeder.SeedAsync(db);
        }

        var factory = CreateMockFactory();
        var studentService = new StudentService(factory);

        // Act: Paged query for students with MSSV starting with DCCN
        var pagedResult = await studentService.GetStudentsPagedAsync(search: "DCCN", pageIndex: 1, pageSize: 20);
        pagedResult.Items.Should().NotBeEmpty();
        pagedResult.TotalCount.Should().BeGreaterThan(0);

        // Act: Export to Excel
        var excelBytes = await studentService.ExportStudentsToExcelAsync();
        excelBytes.Should().NotBeNull();
        excelBytes.Length.Should().BeGreaterThan(1000);

        using var ms = new MemoryStream(excelBytes);
        using var workbook = new XLWorkbook(ms);
        var worksheet = workbook.Worksheets.First();
        worksheet.RowCount().Should().BeGreaterThan(10);
    }

    [Fact]
    public async Task AllocationEngine_GreedyAndLocalSearch_ShouldSatisfyGenderAndCapacityConstraints()
    {
        // Arrange
        using (var db = new KtxDbContext(_dbOptions))
        {
            await DatabaseSeeder.SeedAsync(db);
        }

        var factory = CreateMockFactory();
        var regService = new RegistrationService(factory);
        var configService = new ConfigurationService(factory);
        var engine = new AllocationEngine(factory);

        var batches = await regService.GetBatchesAsync();
        batches.Should().NotBeEmpty();

        var config = await configService.GetSystemConfigAsync();

        // Act: Run allocation engine on pending applications
        var result = await engine.ExecuteAllocationAsync(batches[0].Id, config, enableLocalSearch: true, maxLocalSearchSeconds: 2);

        // Assert: Allocation results
        result.Should().NotBeNull();
        result.TotalApplications.Should().BeGreaterThan(0);
        result.AssignedCount.Should().BeGreaterThan(0);
        result.Assignments.Should().NotBeEmpty();

        // Hard Constraint 1: Males only in male buildings, Females only in female buildings
        using (var db = new KtxDbContext(_dbOptions))
        {
            var roomDict = await db.Phongs.Include(p => p.ToaNha).ToDictionaryAsync(p => p.Id);

            foreach (var assign in result.Assignments)
            {
                var room = roomDict[assign.PhongId];
                room.ToaNha.GioiTinh.Should().Be(assign.GioiTinh,
                    because: $"Student {assign.MSSV} of gender {assign.GioiTinh} cannot be allocated to {room.ToaNha.Ten} with gender policy {room.ToaNha.GioiTinh}");
            }

            // Hard Constraint 2: Room capacity must never be exceeded
            var roomCapacities = await db.Phongs.Include(p => p.LoaiPhong).ToDictionaryAsync(p => p.Id, p => p.LoaiPhong.SucChua);
            var roomCurrentResidents = await db.Phongs.ToDictionaryAsync(p => p.Id, p => db.HopDongs.Count(h => h.PhongId == p.Id && h.TrangThai == TrangThaiHopDong.HieuLuc));

            var newlyAssignedPerRoom = result.Assignments.GroupBy(a => a.PhongId).ToDictionary(g => g.Key, g => g.Count());
            foreach (var kvp in newlyAssignedPerRoom)
            {
                int totalInRoom = roomCurrentResidents[kvp.Key] + kvp.Value;
                totalInRoom.Should().BeLessThanOrEqualTo(roomCapacities[kvp.Key],
                    because: $"Room ID {kvp.Key} capacity is {roomCapacities[kvp.Key]} but has {totalInRoom} residents");
            }
        }
    }

    [Fact]
    public async Task ContractService_TransferExtendAndCheckOut_ShouldUpdateStateCorrectly()
    {
        // Arrange
        using (var db = new KtxDbContext(_dbOptions))
        {
            await DatabaseSeeder.SeedAsync(db);
        }

        var factory = CreateMockFactory();
        var contractService = new ContractService(factory);

        var pagedContracts = await contractService.GetContractsPagedAsync(search: null, status: TrangThaiHopDong.HieuLuc, expiringSoon: null, pageIndex: 1, pageSize: 10);
        pagedContracts.Items.Should().NotBeEmpty();

        var testContract = pagedContracts.Items[0];
        int contractId = testContract.Id;
        int originalRoomId = testContract.PhongId;

        // Find another room of the same building to transfer
        int targetRoomId;
        using (var db = new KtxDbContext(_dbOptions))
        {
            var curRoom = await db.Phongs.FindAsync(originalRoomId);
            var targetRoom = await db.Phongs
                .Where(p => p.Id != originalRoomId && p.ToaId == curRoom!.ToaId && p.TrangThai == TrangThaiPhong.HoatDong)
                .FirstAsync();
            targetRoomId = targetRoom.Id;
        }

        // Act 1: Transfer Room
        var transferResult = await contractService.TransferRoomAsync(new RoomTransferRequestDto
        {
            HopDongId = contractId,
            DenPhongId = targetRoomId,
            LyDo = "Chuyển theo nguyện vọng học tập"
        });
        transferResult.Success.Should().BeTrue();

        using (var db = new KtxDbContext(_dbOptions))
        {
            var transferred = await db.HopDongs.AsNoTracking().FirstOrDefaultAsync(h => h.Id == contractId);
            transferred!.PhongId.Should().Be(targetRoomId);
        }

        // Act 2: Extend Contract beyond 2027-06-30
        var newEndDate = new DateTime(2028, 6, 30);
        bool extendResult = await contractService.ExtendContractAsync(contractId, newEndDate);
        extendResult.Should().BeTrue();

        using (var db = new KtxDbContext(_dbOptions))
        {
            var extended = await db.HopDongs.AsNoTracking().FirstOrDefaultAsync(h => h.Id == contractId);
            extended!.NgayKetThuc.Date.Should().Be(newEndDate.Date);
        }

        // Act 3: Check Out
        var checkoutResult = await contractService.CheckOutContractAsync(new ContractCheckOutDto
        {
            HopDongId = contractId,
            TienHongHocTaiSan = 100000m,
            GhiChu = "Hỏng đèn bàn học"
        });
        checkoutResult.Success.Should().BeTrue();

        using (var db = new KtxDbContext(_dbOptions))
        {
            var checkedOut = await db.HopDongs.AsNoTracking().FirstOrDefaultAsync(h => h.Id == contractId);
            checkedOut!.TrangThai.Should().Be(TrangThaiHopDong.DaTra);
        }
    }

    [Fact]
    public async Task TieredBillingService_CalculationAndInvoicing_ShouldSucceed()
    {
        // Arrange
        using (var db = new KtxDbContext(_dbOptions))
        {
            await DatabaseSeeder.SeedAsync(db);
        }

        var factory = CreateMockFactory();
        var configService = new ConfigurationService(factory);
        var billingService = new TieredBillingService(factory, configService);

        // Act: Generate invoices for month 10/2026 (which has seeded meter readings)
        var generatedInvoices = await billingService.GenerateMonthlyInvoicesAsync(10, 2026);
        generatedInvoices.Should().NotBeNull();
        generatedInvoices.Count.Should().BeGreaterThan(0);

        // Act: Query generated readings and invoices
        var readings = await billingService.GetReadingsAsync(10, 2026, null);
        readings.Should().NotBeEmpty();

        var invoices = await billingService.GetInvoicesAsync(10, 2026, null, null);
        invoices.Should().NotBeEmpty();

        // Act: Process cash payment for the first unpaid invoice
        var unpaidInvoice = invoices.FirstOrDefault(i => i.TrangThai == TrangThaiHoaDon.ChuaThanhToan);
        if (unpaidInvoice != null)
        {
            bool payResult = await billingService.RecordCashPaymentAsync(unpaidInvoice.Id, null, unpaidInvoice.TongTien);
            payResult.Should().BeTrue();

            using var db = new KtxDbContext(_dbOptions);
            var updated = await db.HoaDonDienNuocs.FindAsync(unpaidInvoice.Id);
            updated!.TrangThai.Should().Be(TrangThaiHoaDon.DaThanhToan);
        }
    }

    [Fact]
    public async Task ReconciliationService_AntiDuplicateAndMatching_ShouldWorkCorrectly()
    {
        // Arrange
        using (var db = new KtxDbContext(_dbOptions))
        {
            await DatabaseSeeder.SeedAsync(db);
        }

        var factory = CreateMockFactory();
        var reconciliationService = new ReconciliationService(factory);

        // Act: Generate sample bank statement Excel
        byte[] sampleExcel = await reconciliationService.GenerateBankStatementSampleExcelAsync();
        sampleExcel.Should().NotBeNull();
        sampleExcel.Length.Should().BeGreaterThan(2000);

        // Act: Import statement into system
        using var stream1 = new MemoryStream(sampleExcel);
        var result1 = await reconciliationService.ImportAndReconcileExcelAsync(stream1, "SaoKe_MBBank_T10.xlsx");

        result1.TotalRows.Should().BeGreaterThan(0);
        result1.MatchedCount.Should().BeGreaterThan(0);
        result1.DuplicateCount.Should().Be(1); // Mẫu sao kê chứa 1 dòng trùng cố tình để kiểm thử chống trùng

        // Act: Re-import the EXACT SAME statement to test Double-Hash anti-duplicate protection
        using var stream2 = new MemoryStream(sampleExcel);
        var result2 = await reconciliationService.ImportAndReconcileExcelAsync(stream2, "SaoKe_MBBank_T10_Duplicate.xlsx");

        // Assert: 100% of rows must be safely skipped as duplicates
        result2.DuplicateCount.Should().Be(result1.TotalRows);
        result2.MatchedCount.Should().Be(0);
    }

    [Fact]
    public async Task MaintenanceService_WorkflowAndCostTracking_ShouldOperateSmoothly()
    {
        // Arrange
        using (var db = new KtxDbContext(_dbOptions))
        {
            await DatabaseSeeder.SeedAsync(db);
        }

        var factory = CreateMockFactory();
        var maintenanceService = new MaintenanceService(factory);

        // Act: Get existing stats
        var statsBefore = await maintenanceService.GetStatsAsync();
        statsBefore.TotalRequests.Should().Be(8);

        // Act: Create new maintenance request
        var newReqDto = new MaintenanceCreateDto
        {
            PhongId = 1,
            MoTa = "Bóng đèn hành lang bị nhấp nháy cần kiểm tra chấn lưu",
            GhiChu = "Sinh viên báo qua ban quản lý"
        };
        var created = await maintenanceService.CreateRequestAsync(newReqDto);
        created.Should().NotBeNull();
        created.TrangThai.Should().Be("ChuaXuLy");

        // Act: Update to DangXuLy
        bool update1 = await maintenanceService.UpdateRequestStatusAsync(new MaintenanceStatusUpdateDto
        {
            Id = created.Id,
            TrangThai = "DangXuLy",
            ChiPhi = 50000m,
            GhiChu = "Đã cử thợ điện kiểm tra"
        });
        update1.Should().BeTrue();

        // Act: Complete request
        bool update2 = await maintenanceService.UpdateRequestStatusAsync(new MaintenanceStatusUpdateDto
        {
            Id = created.Id,
            TrangThai = "HoanThanh",
            ChiPhi = 120000m,
            GhiChu = "Đã thay bóng LED Philips mới"
        });
        update2.Should().BeTrue();

        var statsAfter = await maintenanceService.GetStatsAsync();
        statsAfter.TotalRequests.Should().Be(9);
        statsAfter.CompletedCount.Should().Be(statsBefore.CompletedCount + 1);
        statsAfter.TotalCost.Should().Be(statsBefore.TotalCost + 120000m);
    }

    [Fact]
    public async Task ReportingService_ExcelAndPdf_ShouldGenerateValidStreams()
    {
        // Arrange
        using (var db = new KtxDbContext(_dbOptions))
        {
            await DatabaseSeeder.SeedAsync(db);
        }

        var factory = CreateMockFactory();
        var reportingService = new ReportingService(factory);

        // Act 1: Generate ClosedXML Occupancy Report
        byte[] excelReport = await reportingService.ExportOccupancyReportExcelAsync();
        excelReport.Should().NotBeNull();
        excelReport.Length.Should().BeGreaterThan(5000);

        using var ms = new MemoryStream(excelReport);
        using var workbook = new XLWorkbook(ms);
        workbook.Worksheets.Count.Should().BeGreaterThan(0);
        var ws = workbook.Worksheets.First();
        ws.Cell("A2").Value.ToString().Should().Contain("BÁO CÁO");

        // Act 2: Generate QuestPDF Financial Report
        byte[] pdfReport = await reportingService.ExportFinancialReportPdfAsync(10, 2026);
        pdfReport.Should().NotBeNull();
        pdfReport.Length.Should().BeGreaterThan(5000);

        // Verify PDF Magic Bytes (%PDF-)
        pdfReport.Take(4).ToArray().Should().Equal(new byte[] { 0x25, 0x50, 0x44, 0x46 });
    }

    [Fact]
    public async Task DashboardService_AggregationAndRevenueHistory_ShouldReturnCorrectData()
    {
        // Arrange
        using (var db = new KtxDbContext(_dbOptions))
        {
            await DatabaseSeeder.SeedAsync(db);
        }

        var factory = CreateMockFactory();
        var dashboardService = new DashboardService(factory);

        // Act: Get Dashboard Stats
        var stats = await dashboardService.GetDashboardStatsAsync();

        // Assert
        stats.Should().NotBeNull();
        stats.TotalRooms.Should().Be(120);
        stats.TotalBeds.Should().Be(720);
        stats.OccupiedBeds.Should().BeGreaterThan(400);
        stats.ActiveContracts.Should().BeGreaterThan(400);
        stats.ExpiringContracts30Days.Should().Be(14);
        stats.PendingMaintenance.Should().Be(3);
        stats.MonthlyRevenue.Should().BeGreaterThan(50_000_000m);

        stats.Buildings.Should().HaveCount(4);
        stats.Buildings.All(b => b.OccupiedBeds > 0).Should().BeTrue();

        stats.RevenueHistory.Should().HaveCount(6);
        stats.RevenueHistory.All(h => h.TotalRevenue > 0).Should().BeTrue();
    }
}

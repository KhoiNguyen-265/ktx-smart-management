using System.Diagnostics;
using FluentAssertions;
using KTX.Data;
using KTX.Domain.DTOs;
using KTX.Domain.Entities;
using KTX.Domain.Enums;
using KTX.Services.Allocation;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit.Abstractions;

namespace KTX.Tests;

public class AlgorithmBenchmarkTests
{
    private readonly ITestOutputHelper _output;
    private readonly DbContextOptions<KtxDbContext> _dbOptions;

    public AlgorithmBenchmarkTests(ITestOutputHelper output)
    {
        _output = output;
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

    private async Task SeedLargeDormitoryAsync(int totalRooms = 150)
    {
        using var db = new KtxDbContext(_dbOptions);

        var bNam = new ToaNha { Id = 1, Ten = "Tòa A (Nam)", GioiTinh = GioiTinh.Nam, SoTang = 5 };
        var bNu = new ToaNha { Id = 2, Ten = "Tòa B (Nữ)", GioiTinh = GioiTinh.Nu, SoTang = 5 };
        db.ToaNhas.AddRange(bNam, bNu);

        var lp4 = new LoaiPhong { Id = 1, Ten = "Phòng 4 Người", SucChua = 4, GiaThang = 800000m };
        var lp6 = new LoaiPhong { Id = 2, Ten = "Phòng 6 Người", SucChua = 6, GiaThang = 600000m };
        var lp8 = new LoaiPhong { Id = 3, Ten = "Phòng 8 Người", SucChua = 8, GiaThang = 450000m };
        db.LoaiPhongs.AddRange(lp4, lp6, lp8);

        // Tạo 150 phòng (tổng sức chứa ~750 chỗ)
        for (int i = 1; i <= totalRooms; i++)
        {
            var toa = (i % 2 == 1) ? bNam : bNu;
            int lpId = (i % 3) + 1;
            int tang = ((i - 1) % 5) + 1;

            db.Phongs.Add(new Phong
            {
                Id = i,
                MaPhong = $"P{(i % 2 == 1 ? "A" : "B")}{tang:D2}{i:D3}",
                ToaId = toa.Id,
                ToaNha = toa,
                Tang = tang,
                LoaiPhongId = lpId,
                TrangThai = TrangThaiPhong.HoatDong
            });
        }

        var tinh = new TinhThanh { Id = 1, Ten = "Hà Nội", KhoangCachKm = 10, NhomKhoangCach = 1 };
        db.TinhThanhs.Add(tinh);

        var dot = new DotDangKy
        {
            Id = 1,
            Ten = "Đợt Benchmark Tối Ưu Xếp Phòng",
            NgayMo = DateTime.Today.AddDays(-10),
            NgayDong = DateTime.Today.AddDays(10),
            DangMo = true
        };
        db.DotDangKys.Add(dot);

        await db.SaveChangesAsync();
    }

    private async Task GenerateApplicationsAsync(int count)
    {
        using var db = new KtxDbContext(_dbOptions);

        var random = new Random(42); // Seed cố định để kết quả benchmark tái lập chính xác 100%
        var students = new List<SinhVien>();
        var apps = new List<DonDangKy>();
        int groupCounter = 1;

        for (int i = 1; i <= count; i++)
        {
            var gender = (i % 2 == 1) ? GioiTinh.Nam : GioiTinh.Nu;
            string mssv = $"BM{i:D5}";

            students.Add(new SinhVien
            {
                MSSV = mssv,
                HoTen = $"Sinh Viên Benchmark {i}",
                GioiTinh = gender,
                CCCD = $"001205{i:D6}",
                Khoa = "CNTT",
                KhoaHoc = random.Next(66, 70),
                TinhThanhId = 1,
                SoDienThoai = $"09{i:D8}",
                Email = $"{mssv}@sis.edu.vn"
            });

            int? nhomId = null;
            // 30% sinh viên đăng ký theo nhóm bạn (2-3 bạn / nhóm)
            if (i % 3 == 0)
            {
                nhomId = groupCounter;
                if (i % 2 == 0) groupCounter++;
            }

            apps.Add(new DonDangKy
            {
                Id = i,
                DotId = 1,
                MSSV = mssv,
                DiemUuTien = random.Next(0, 50),
                LoaiPhongMongMuonId = (random.Next(0, 3) == 0) ? null : random.Next(1, 4),
                ToaMongMuonId = null,
                TangMongMuon = null,
                NhomId = nhomId,
                TrangThai = TrangThaiDonDangKy.ChoXep,
                ThoiGianNop = DateTime.Now.AddHours(-random.Next(1, 240))
            });
        }

        db.SinhViens.AddRange(students);
        db.DonDangKys.AddRange(apps);
        await db.SaveChangesAsync();
    }

    [Theory]
    [InlineData(100, 400)]
    [InlineData(500, 1000)]
    [InlineData(1000, 2500)]
    [InlineData(2000, 6000)]
    public async Task Benchmark_AllocationEngine_PerformanceScalability(int applicationCount, int maxAllowedMs)
    {
        // Arrange
        await SeedLargeDormitoryAsync(150);
        await GenerateApplicationsAsync(applicationCount);

        var mockFactory = CreateMockFactory();
        var engine = new AllocationEngine(mockFactory);
        var config = new SystemConfigDto();

        // Act: Đo lường thời gian chạy thuật toán tối ưu xếp phòng
        var sw = Stopwatch.StartNew();
        var result = await engine.ExecuteAllocationAsync(
            batchId: 1,
            config: config,
            enableLocalSearch: true);
        sw.Stop();

        long elapsedMs = sw.ElapsedMilliseconds;

        _output.WriteLine($"[BENCHMARK] Số đơn: {applicationCount,5} | Thời gian: {elapsedMs,5} ms | Xếp được: {result.AssignedCount,4} SV | Chờ: {result.WaitingCount,4} SV | Hàm mục tiêu S: {result.TotalFitnessScore:F2}");

        // Assert
        result.Should().NotBeNull();
        result.AssignedCount.Should().BeGreaterThan(0);
        elapsedMs.Should().BeLessThanOrEqualTo(maxAllowedMs,
            $"Thuật toán cần phản hồi nhanh dưới {maxAllowedMs}ms cho {applicationCount} đơn đăng ký.");
    }
}

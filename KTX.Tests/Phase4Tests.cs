using System.Diagnostics;
using FluentAssertions;
using KTX.Data;
using KTX.Domain.DTOs;
using KTX.Domain.Entities;
using KTX.Domain.Enums;
using KTX.Services.Allocation;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace KTX.Tests;

public class Phase4Tests
{
    private readonly DbContextOptions<KtxDbContext> _dbOptions;

    public Phase4Tests()
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

    [Fact]
    public void PriorityCalculator_ShouldCalculateAccurateScore()
    {
        // Arrange
        var batchStart = new DateTime(2026, 9, 1, 0, 0, 0);
        var batchEnd = new DateTime(2026, 9, 15, 0, 0, 0);
        var submitTime = new DateTime(2026, 9, 1, 12, 0, 0); // Submitted very early

        // Act
        var score = PriorityCalculator.CalculatePriority(
            priorityPolicyPoints: 30, // Thương binh
            studentCourse: 68,        // Tân sinh viên
            latestCourse: 68,
            distanceGroup: 3,         // > 300km (20 pts)
            submittedAt: submitTime,
            batchStart: batchStart,
            batchEnd: batchEnd);

        // Assert
        // Score = 30 + 10 + 20 + tieBreak (~4.something) >= 64
        score.Should().BeGreaterThanOrEqualTo(64m);
    }

    [Fact]
    public async Task AllocationEngine_ShouldSatisfyAllHardConstraints()
    {
        // Arrange
        using (var db = new KtxDbContext(_dbOptions))
        {
            // Seed 2 buildings: Toa Nam (Id 1) and Toa Nu (Id 2)
            var bNam = new ToaNha { Id = 1, Ten = "Tòa A (Nam)", GioiTinh = GioiTinh.Nam, SoTang = 3 };
            var bNu = new ToaNha { Id = 2, Ten = "Tòa B (Nữ)", GioiTinh = GioiTinh.Nu, SoTang = 3 };
            db.ToaNhas.AddRange(bNam, bNu);

            var lp4 = new LoaiPhong { Id = 1, Ten = "Phòng 4", SucChua = 4, GiaThang = 800000m };
            var lp6 = new LoaiPhong { Id = 2, Ten = "Phòng 6", SucChua = 6, GiaThang = 600000m };
            db.LoaiPhongs.AddRange(lp4, lp6);

            // 1 Phòng Nam 4 chỗ hoạt động
            db.Phongs.Add(new Phong { Id = 1, MaPhong = "A101", ToaId = 1, ToaNha = bNam, Tang = 1, LoaiPhongId = 1, LoaiPhong = lp4, TrangThai = TrangThaiPhong.HoatDong });
            // 1 Phòng Nữ 4 chỗ hoạt động
            db.Phongs.Add(new Phong { Id = 2, MaPhong = "B101", ToaId = 2, ToaNha = bNu, Tang = 1, LoaiPhongId = 1, LoaiPhong = lp4, TrangThai = TrangThaiPhong.HoatDong });
            // 1 Phòng Nam đang bảo trì
            db.Phongs.Add(new Phong { Id = 3, MaPhong = "A102-BT", ToaId = 1, ToaNha = bNam, Tang = 1, LoaiPhongId = 1, LoaiPhong = lp4, TrangThai = TrangThaiPhong.BaoTri });

            // 3 sinh viên Nam và 3 sinh viên Nữ
            var tinh = new TinhThanh { Id = 1, Ten = "Hà Nội", KhoangCachKm = 10, NhomKhoangCach = 1 };
            db.TinhThanhs.Add(tinh);

            var svNam1 = new SinhVien { MSSV = "SV_M1", HoTen = "Nam 1", GioiTinh = GioiTinh.Nam, TinhThanhId = 1, CCCD = "001205000001", Khoa = "CNTT", KhoaHoc = 68 };
            var svNam2 = new SinhVien { MSSV = "SV_M2", HoTen = "Nam 2", GioiTinh = GioiTinh.Nam, TinhThanhId = 1, CCCD = "001205000002", Khoa = "CNTT", KhoaHoc = 68 };
            var svNu1 = new SinhVien { MSSV = "SV_F1", HoTen = "Nu 1", GioiTinh = GioiTinh.Nu, TinhThanhId = 1, CCCD = "001205000003", Khoa = "KinhTe", KhoaHoc = 68 };
            var svNu2 = new SinhVien { MSSV = "SV_F2", HoTen = "Nu 2", GioiTinh = GioiTinh.Nu, TinhThanhId = 1, CCCD = "001205000004", Khoa = "KinhTe", KhoaHoc = 68 };

            db.SinhViens.AddRange(svNam1, svNam2, svNu1, svNu2);

            var batch = new DotDangKy { Id = 1, Ten = "Đợt 1", NgayMo = DateTime.Now.AddDays(-5), NgayDong = DateTime.Now.AddDays(5), DangMo = true };
            db.DotDangKys.Add(batch);

            // Đơn đăng ký
            db.DonDangKys.Add(new DonDangKy { Id = 1, DotId = 1, MSSV = "SV_M1", DiemUuTien = 50, TrangThai = TrangThaiDonDangKy.ChoXep });
            db.DonDangKys.Add(new DonDangKy { Id = 2, DotId = 1, MSSV = "SV_M2", DiemUuTien = 45, TrangThai = TrangThaiDonDangKy.ChoXep });
            db.DonDangKys.Add(new DonDangKy { Id = 3, DotId = 1, MSSV = "SV_F1", DiemUuTien = 60, TrangThai = TrangThaiDonDangKy.ChoXep });
            db.DonDangKys.Add(new DonDangKy { Id = 4, DotId = 1, MSSV = "SV_F2", DiemUuTien = 55, TrangThai = TrangThaiDonDangKy.ChoXep });

            await db.SaveChangesAsync();
        }

        var factory = CreateMockFactory();
        var engine = new AllocationEngine(factory);
        var config = new SystemConfigDto();

        // Act
        var result = await engine.ExecuteAllocationAsync(1, config, enableLocalSearch: false);

        // Assert
        result.AssignedCount.Should().Be(4);
        result.WaitingCount.Should().Be(0);

        // Ràng buộc giới tính
        var maleAssignments = result.Assignments.Where(a => a.GioiTinh == GioiTinh.Nam).ToList();
        maleAssignments.Should().OnlyContain(a => a.PhongId == 1 && a.ToaTen.Contains("Nam"));

        var femaleAssignments = result.Assignments.Where(a => a.GioiTinh == GioiTinh.Nu).ToList();
        femaleAssignments.Should().OnlyContain(a => a.PhongId == 2 && a.ToaTen.Contains("Nữ"));

        // Không xếp vào phòng bảo trì
        result.Assignments.Should().NotContain(a => a.PhongId == 3);
    }

    [Fact]
    public async Task AllocationEngine_ShouldKeepRoommateGroupsTogether()
    {
        // Arrange
        using (var db = new KtxDbContext(_dbOptions))
        {
            var bNam = new ToaNha { Id = 10, Ten = "Tòa A (Nam)", GioiTinh = GioiTinh.Nam, SoTang = 2 };
            db.ToaNhas.Add(bNam);

            var lp4 = new LoaiPhong { Id = 10, Ten = "Phòng 4 chỗ", SucChua = 4, GiaThang = 800000m };
            db.LoaiPhongs.Add(lp4);

            var p1 = new Phong { Id = 101, MaPhong = "A201", ToaId = 10, ToaNha = bNam, Tang = 2, LoaiPhongId = 10, LoaiPhong = lp4, TrangThai = TrangThaiPhong.HoatDong };
            var p2 = new Phong { Id = 102, MaPhong = "A202", ToaId = 10, ToaNha = bNam, Tang = 2, LoaiPhongId = 10, LoaiPhong = lp4, TrangThai = TrangThaiPhong.HoatDong };
            db.Phongs.AddRange(p1, p2);

            var tinh = new TinhThanh { Id = 10, Ten = "Hải Phòng", KhoangCachKm = 100, NhomKhoangCach = 2 };
            db.TinhThanhs.Add(tinh);

            // Nhóm bạn 4 người
            var nhom = new NhomDangKy { Id = 99, MaNhom = "BAN_THAN_4", MSSVTruongNhom = "SV_GRP1" };
            db.NhomDangKys.Add(nhom);

            var batch = new DotDangKy { Id = 10, Ten = "Đợt Nhóm", NgayMo = DateTime.Now, NgayDong = DateTime.Now.AddDays(7), DangMo = true };
            db.DotDangKys.Add(batch);

            for (int i = 1; i <= 4; i++)
            {
                var mssv = $"SV_GRP{i}";
                db.SinhViens.Add(new SinhVien
                {
                    MSSV = mssv,
                    HoTen = $"Bạn thân {i}",
                    GioiTinh = GioiTinh.Nam,
                    CCCD = $"03120500000{i}",
                    Khoa = "CNTT",
                    KhoaHoc = 68,
                    TinhThanhId = 10
                });

                db.DonDangKys.Add(new DonDangKy
                {
                    DotId = 10,
                    MSSV = mssv,
                    NhomId = 99,
                    NhomDangKy = nhom,
                    DiemUuTien = 40,
                    TrangThai = TrangThaiDonDangKy.ChoXep
                });
            }

            await db.SaveChangesAsync();
        }

        var factory = CreateMockFactory();
        var engine = new AllocationEngine(factory);
        var config = new SystemConfigDto();

        // Act
        var result = await engine.ExecuteAllocationAsync(10, config, enableLocalSearch: true);

        // Assert
        result.AssignedCount.Should().Be(4);
        result.RoommateKeepRate.Should().Be(100.0);

        var assignedRoomIds = result.Assignments.Select(a => a.PhongId).Distinct().ToList();
        assignedRoomIds.Count.Should().Be(1); // Cả 4 bạn cùng phòng
    }

    [Fact]
    public async Task AllocationEngine_Benchmark500Applications_ShouldCompleteUnder3Seconds()
    {
        // Arrange: Generate 120 rooms (capacity ~600) and 500 applications
        using (var db = new KtxDbContext(_dbOptions))
        {
            var bNam = new ToaNha { Id = 20, Ten = "Tòa Nam Benchmark", GioiTinh = GioiTinh.Nam, SoTang = 5 };
            var bNu = new ToaNha { Id = 21, Ten = "Tòa Nữ Benchmark", GioiTinh = GioiTinh.Nu, SoTang = 5 };
            db.ToaNhas.AddRange(bNam, bNu);

            var lp6 = new LoaiPhong { Id = 20, Ten = "Phòng 6 chỗ", SucChua = 6, GiaThang = 600000m };
            db.LoaiPhongs.Add(lp6);

            // 100 phòng
            for (int r = 1; r <= 50; r++)
            {
                db.Phongs.Add(new Phong { Id = 200 + r, MaPhong = $"BNam_{r}", ToaId = 20, Tang = 1 + (r % 5), LoaiPhongId = 20, LoaiPhong = lp6, TrangThai = TrangThaiPhong.HoatDong });
            }
            for (int r = 1; r <= 50; r++)
            {
                db.Phongs.Add(new Phong { Id = 300 + r, MaPhong = $"BNu_{r}", ToaId = 21, Tang = 1 + (r % 5), LoaiPhongId = 20, LoaiPhong = lp6, TrangThai = TrangThaiPhong.HoatDong });
            }

            var tinh = new TinhThanh { Id = 20, Ten = "Đà Nẵng", KhoangCachKm = 700, NhomKhoangCach = 3 };
            db.TinhThanhs.Add(tinh);

            var batch = new DotDangKy { Id = 20, Ten = "Đợt Benchmark 500 Đơn", NgayMo = DateTime.Now, NgayDong = DateTime.Now.AddDays(10), DangMo = true };
            db.DotDangKys.Add(batch);

            for (int i = 1; i <= 500; i++)
            {
                var gender = (i % 2 == 0) ? GioiTinh.Nam : GioiTinh.Nu;
                var mssv = $"SV_BENCH_{i:D4}";
                db.SinhViens.Add(new SinhVien
                {
                    MSSV = mssv,
                    HoTen = $"Sinh Viên Benchmark {i}",
                    GioiTinh = gender,
                    CCCD = $"001205{i:D6}",
                    Khoa = (i % 3 == 0) ? "CNTT" : (i % 3 == 1) ? "KinhTe" : "Dien",
                    KhoaHoc = 65 + (i % 4),
                    TinhThanhId = 20
                });

                db.DonDangKys.Add(new DonDangKy
                {
                    DotId = 20,
                    MSSV = mssv,
                    DiemUuTien = 30 + (i % 50),
                    TrangThai = TrangThaiDonDangKy.ChoXep
                });
            }

            await db.SaveChangesAsync();
        }

        var factory = CreateMockFactory();
        var engine = new AllocationEngine(factory);
        var config = new SystemConfigDto();

        // Act
        var sw = Stopwatch.StartNew();
        var result = await engine.ExecuteAllocationAsync(20, config, enableLocalSearch: true, maxLocalSearchSeconds: 1);
        sw.Stop();

        // Assert
        sw.ElapsedMilliseconds.Should().BeLessThan(3000); // Tiêu chí nghiệm thu: dưới 3 giây!
        result.AssignedCount.Should().Be(500);
        result.WaitingCount.Should().Be(0);

        // 0 vi phạm ràng buộc giới tính
        foreach (var a in result.Assignments)
        {
            if (a.GioiTinh == GioiTinh.Nam)
            {
                a.ToaTen.Should().Contain("Nam");
            }
            else
            {
                a.ToaTen.Should().Contain("Nữ");
            }
        }
    }
}

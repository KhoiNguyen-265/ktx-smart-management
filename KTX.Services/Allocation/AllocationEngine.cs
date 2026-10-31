using System.Diagnostics;
using System.Text.Json;
using KTX.Data;
using KTX.Domain.DTOs;
using KTX.Domain.Entities;
using KTX.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace KTX.Services.Allocation;

public class AllocationEngine : IAllocationEngine
{
    private readonly IDbContextFactory<KtxDbContext> _contextFactory;

    public AllocationEngine(IDbContextFactory<KtxDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    private class RoomMemory
    {
        public int Id { get; set; }
        public string MaPhong { get; set; } = string.Empty;
        public int ToaId { get; set; }
        public string ToaTen { get; set; } = string.Empty;
        public GioiTinh GioiTinh { get; set; }
        public int Tang { get; set; }
        public int LoaiPhongId { get; set; }
        public int SucChua { get; set; }
        public decimal GiaThang { get; set; }
        public TrangThaiPhong TrangThai { get; set; }

        public List<StudentCandidate> Occupants { get; set; } = new();
        public int AvailableBeds => SucChua - Occupants.Count;
    }

    private class StudentCandidate
    {
        public int DonId { get; set; }
        public string MSSV { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public GioiTinh GioiTinh { get; set; }
        public string Khoa { get; set; } = string.Empty;
        public int KhoaHoc { get; set; }
        public string? MaNhom { get; set; }
        public decimal PriorityScore { get; set; }

        public int? LoaiPhongMongMuonId { get; set; }
        public bool BatBuocLoaiPhong { get; set; }
        public int? ToaMongMuonId { get; set; }
        public int? TangMongMuon { get; set; }

        public decimal FitnessScore { get; set; }
        public bool ChinhTay { get; set; }
    }

    private class AllocationUnit
    {
        public string? MaNhom { get; set; }
        public List<StudentCandidate> Members { get; set; } = new();
        public decimal UnitPriority => Members.Count > 0 ? Members.Average(m => m.PriorityScore) : 0;
        public GioiTinh GioiTinh => Members[0].GioiTinh;
    }

    public async Task<AllocationResultDto> ExecuteAllocationAsync(
        int batchId,
        SystemConfigDto config,
        bool enableLocalSearch = true,
        int maxLocalSearchSeconds = 3,
        IProgress<AllocationProgressDto>? progress = null,
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();

        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        // 1. Nạp danh sách phòng hoạt động vào bộ nhớ
        var roomsDb = await db.Phongs.AsNoTracking()
            .Include(p => p.ToaNha)
            .Include(p => p.LoaiPhong)
            .Include(p => p.HopDongs.Where(h => h.TrangThai == TrangThaiHopDong.HieuLuc))
                .ThenInclude(h => h.SinhVien)
            .Where(p => p.TrangThai == TrangThaiPhong.HoatDong)
            .ToListAsync(ct);

        var rooms = roomsDb.Select(r => new RoomMemory
        {
            Id = r.Id,
            MaPhong = r.MaPhong,
            ToaId = r.ToaId,
            ToaTen = r.ToaNha.Ten,
            GioiTinh = r.ToaNha.GioiTinh,
            Tang = r.Tang,
            LoaiPhongId = r.LoaiPhongId,
            SucChua = r.LoaiPhong.SucChua,
            GiaThang = r.LoaiPhong.GiaThang,
            TrangThai = r.TrangThai,
            // Nạp các sinh viên đang ở sẵn trong phòng (nếu có từ trước)
            Occupants = r.HopDongs.Where(h => h.TrangThai == TrangThaiHopDong.HieuLuc).Select(h => new StudentCandidate
            {
                DonId = 0,
                MSSV = h.SinhVien.MSSV,
                HoTen = h.SinhVien.HoTen,
                GioiTinh = h.SinhVien.GioiTinh,
                Khoa = h.SinhVien.Khoa,
                KhoaHoc = h.SinhVien.KhoaHoc
            }).ToList()
        }).ToList();

        // 2. Nạp đơn đăng ký của đợt
        var applicationsDb = await db.DonDangKys.AsNoTracking()
            .Where(d => d.DotId == batchId && d.TrangThai == TrangThaiDonDangKy.ChoXep)
            .Include(d => d.SinhVien)
            .Include(d => d.NhomDangKy)
            .ToListAsync(ct);

        var candidates = applicationsDb.Select(d => new StudentCandidate
        {
            DonId = d.Id,
            MSSV = d.MSSV,
            HoTen = d.SinhVien.HoTen,
            GioiTinh = d.SinhVien.GioiTinh,
            Khoa = d.SinhVien.Khoa,
            KhoaHoc = d.SinhVien.KhoaHoc,
            MaNhom = d.NhomDangKy?.MaNhom,
            PriorityScore = d.DiemUuTien,
            LoaiPhongMongMuonId = d.LoaiPhongMongMuonId,
            BatBuocLoaiPhong = d.BatBuocLoaiPhong,
            ToaMongMuonId = d.ToaMongMuonId,
            TangMongMuon = d.TangMongMuon
        }).ToList();

        // 3. Gom đơn vị: nhóm bạn (size >= 1) và sinh viên đăng ký lẻ (size = 1)
        var units = new List<AllocationUnit>();
        var groupBuckets = candidates.Where(c => !string.IsNullOrEmpty(c.MaNhom)).GroupBy(c => c.MaNhom!);
        foreach (var grp in groupBuckets)
        {
            units.Add(new AllocationUnit
            {
                MaNhom = grp.Key,
                Members = grp.ToList()
            });
        }
        foreach (var solo in candidates.Where(c => string.IsNullOrEmpty(c.MaNhom)))
        {
            units.Add(new AllocationUnit
            {
                MaNhom = null,
                Members = new List<StudentCandidate> { solo }
            });
        }

        // Sắp xếp đơn vị theo điểm ưu tiên P giảm dần
        units = units.OrderByDescending(u => u.UnitPriority).ToList();

        var waitingList = new List<StudentCandidate>();
        int processedUnits = 0;

        // 4. Pha GREEDY: Duyệt từng đơn vị và chọn phòng tối ưu
        foreach (var unit in units)
        {
            ct.ThrowIfCancellationRequested();

            var candidateRooms = rooms
                .Where(r => r.GioiTinh == unit.GioiTinh && r.AvailableBeds >= unit.Members.Count)
                .Where(r => unit.Members.All(m => !m.BatBuocLoaiPhong || m.LoaiPhongMongMuonId == null || r.LoaiPhongId == m.LoaiPhongMongMuonId))
                .ToList();

            if (candidateRooms.Count > 0)
            {
                // Chọn phòng có tổng điểm phù hợp lớn nhất
                RoomMemory bestRoom = null!;
                decimal maxUnitScore = decimal.MinValue;

                foreach (var r in candidateRooms)
                {
                    decimal score = 0;
                    foreach (var m in unit.Members)
                    {
                        score += CalculateStudentFitness(m, r, config);
                    }
                    if (score > maxUnitScore)
                    {
                        maxUnitScore = score;
                        bestRoom = r;
                    }
                }

                // Gán cả đơn vị vào phòng tốt nhất
                foreach (var m in unit.Members)
                {
                    m.FitnessScore = CalculateStudentFitness(m, bestRoom, config);
                    bestRoom.Occupants.Add(m);
                }
            }
            else
            {
                // Nếu là nhóm và không tìm được phòng vừa cả nhóm, thử tách nhóm (unit splitting)
                if (unit.Members.Count > 1)
                {
                    foreach (var m in unit.Members)
                    {
                        var singleRooms = rooms
                            .Where(r => r.GioiTinh == m.GioiTinh && r.AvailableBeds >= 1)
                            .Where(r => !m.BatBuocLoaiPhong || m.LoaiPhongMongMuonId == null || r.LoaiPhongId == m.LoaiPhongMongMuonId)
                            .ToList();

                        if (singleRooms.Count > 0)
                        {
                            var bestSoloRoom = singleRooms
                                .OrderByDescending(r => CalculateStudentFitness(m, r, config))
                                .First();

                            m.FitnessScore = CalculateStudentFitness(m, bestSoloRoom, config);
                            bestSoloRoom.Occupants.Add(m);
                        }
                        else
                        {
                            waitingList.Add(m);
                        }
                    }
                }
                else
                {
                    waitingList.Add(unit.Members[0]);
                }
            }

            processedUnits++;

            if (progress != null && (processedUnits % 10 == 0 || processedUnits == units.Count))
            {
                progress.Report(new AllocationProgressDto
                {
                    StepName = "Giai đoạn Greedy Matching...",
                    TotalUnits = units.Count,
                    UnitsProcessed = processedUnits,
                    AssignedCount = candidates.Count - waitingList.Count,
                    WaitingCount = waitingList.Count,
                    RoomOccupancyMap = rooms.ToDictionary(r => r.Id, r => r.Occupants.Count)
                });
            }
        }

        // 5. Pha LOCAL SEARCH (Tối ưu cục bộ bằng 2-Opt Swaps)
        if (enableLocalSearch && candidates.Count > 2)
        {
            var assignedStudents = rooms.SelectMany(r => r.Occupants.Where(o => o.DonId > 0)).ToList();
            var localSearchWatch = Stopwatch.StartNew();
            int maxSwaps = 1000;
            int swapsDone = 0;
            var rand = new Random(42);

            for (int iter = 0; iter < maxSwaps && localSearchWatch.Elapsed.TotalSeconds < maxLocalSearchSeconds; iter++)
            {
                if (ct.IsCancellationRequested) break;

                // Chọn ngẫu nhiên 2 sinh viên cùng giới ở 2 phòng khác nhau
                if (assignedStudents.Count < 2) break;
                var s1 = assignedStudents[rand.Next(assignedStudents.Count)];
                var s2 = assignedStudents[rand.Next(assignedStudents.Count)];

                if (s1 == s2 || s1.GioiTinh != s2.GioiTinh) continue;

                var r1 = rooms.First(r => r.Occupants.Contains(s1));
                var r2 = rooms.First(r => r.Occupants.Contains(s2));
                if (r1 == r2) continue;

                // Kiểm tra ràng buộc cứng khi tráo đổi
                if (s1.BatBuocLoaiPhong && s1.LoaiPhongMongMuonId.HasValue && r2.LoaiPhongId != s1.LoaiPhongMongMuonId.Value)
                    continue;
                if (s2.BatBuocLoaiPhong && s2.LoaiPhongMongMuonId.HasValue && r1.LoaiPhongId != s2.LoaiPhongMongMuonId.Value)
                    continue;

                // Điểm hiện tại
                decimal currentScore = s1.FitnessScore + s2.FitnessScore;

                // Điểm sau khi tráo
                decimal newScoreS1 = CalculateStudentFitness(s1, r2, config);
                decimal newScoreS2 = CalculateStudentFitness(s2, r1, config);
                decimal newScore = newScoreS1 + newScoreS2;

                if (newScore > currentScore)
                {
                    // Chấp nhận bước nhảy cải thiện (Local Search Improvement)
                    r1.Occupants.Remove(s1);
                    r2.Occupants.Remove(s2);

                    s1.FitnessScore = newScoreS1;
                    s2.FitnessScore = newScoreS2;

                    r2.Occupants.Add(s1);
                    r1.Occupants.Add(s2);

                    swapsDone++;
                }
            }

            if (progress != null)
            {
                progress.Report(new AllocationProgressDto
                {
                    StepName = $"Hoàn tất Local Search ({swapsDone} phép tráo tối ưu)",
                    TotalUnits = units.Count,
                    UnitsProcessed = units.Count,
                    AssignedCount = candidates.Count - waitingList.Count,
                    WaitingCount = waitingList.Count,
                    RoomOccupancyMap = rooms.ToDictionary(r => r.Id, r => r.Occupants.Count)
                });
            }
        }

        sw.Stop();

        // 6. Tổng hợp kết quả thống kê
        var result = new AllocationResultDto
        {
            TotalApplications = candidates.Count,
            AssignedCount = candidates.Count - waitingList.Count,
            WaitingCount = waitingList.Count,
            ExecutionTimeMs = sw.ElapsedMilliseconds
        };

        foreach (var r in rooms)
        {
            foreach (var o in r.Occupants.Where(x => x.DonId > 0))
            {
                result.Assignments.Add(new AllocationAssignmentDto
                {
                    DonId = o.DonId,
                    MSSV = o.MSSV,
                    HoTen = o.HoTen,
                    GioiTinh = o.GioiTinh,
                    Khoa = o.Khoa,
                    KhoaHoc = o.KhoaHoc,
                    MaNhom = o.MaNhom,
                    PhongId = r.Id,
                    MaPhong = r.MaPhong,
                    ToaTen = r.ToaTen,
                    Tang = r.Tang,
                    DiemPhuHop = o.FitnessScore,
                    ChinhTay = o.ChinhTay
                });
            }
        }

        foreach (var w in waitingList)
        {
            result.WaitingList.Add(new DonDangKyDto
            {
                Id = w.DonId,
                MSSV = w.MSSV,
                HoTen = w.HoTen,
                GioiTinh = w.GioiTinh,
                Khoa = w.Khoa,
                KhoaHoc = w.KhoaHoc,
                MaNhom = w.MaNhom,
                DiemUuTien = w.PriorityScore,
                TrangThai = TrangThaiDonDangKy.DanhSachCho
            });
        }

        if (result.Assignments.Count > 0)
        {
            result.TotalFitnessScore = (double)result.Assignments.Sum(a => a.DiemPhuHop);
            result.AverageFitnessScore = Math.Round(result.TotalFitnessScore / result.Assignments.Count, 2);
        }

        // Tỉ lệ giữ nhóm: % các nhóm bạn được ở chung phòng hoàn toàn
        var allGroups = candidates.Where(c => !string.IsNullOrEmpty(c.MaNhom)).GroupBy(c => c.MaNhom!).ToList();
        if (allGroups.Count > 0)
        {
            int keptGroups = 0;
            foreach (var grp in allGroups)
            {
                var assignedRooms = result.Assignments
                    .Where(a => a.MaNhom == grp.Key)
                    .Select(a => a.PhongId)
                    .Distinct()
                    .ToList();

                if (assignedRooms.Count == 1 && result.Assignments.Count(a => a.MaNhom == grp.Key) == grp.Count())
                {
                    keptGroups++;
                }
            }
            result.RoommateKeepRate = Math.Round((double)keptGroups / allGroups.Count * 100, 1);
        }
        else
        {
            result.RoommateKeepRate = 100.0;
        }

        // Tỉ lệ lấp đầy phòng
        int totalBeds = rooms.Sum(r => r.SucChua);
        int occupiedBeds = rooms.Sum(r => r.Occupants.Count);
        result.RoomOccupancyRate = totalBeds > 0 ? Math.Round((double)occupiedBeds / totalBeds * 100, 1) : 0;

        return result;
    }

    private static decimal CalculateStudentFitness(StudentCandidate s, RoomMemory r, SystemConfigDto config)
    {
        // Công thức chuẩn:
        // S(s,r) = w1*[đúng loại] + w2*[đúng tòa] + w3*[đúng tầng] + w4*#cùng khoa + w5*#cùng khóa + w6*[phòng có người]
        decimal score = 0;

        // Trọng số cơ sở
        decimal wPref = (decimal)config.WPreference; // Nguyện vọng
        decimal wRoommate = (decimal)config.WRoommate; // Bạn cùng phòng

        // 1. Đúng loại phòng
        if (!s.LoaiPhongMongMuonId.HasValue || s.LoaiPhongMongMuonId == r.LoaiPhongId)
        {
            score += 25 * wPref;
        }

        // 2. Đúng tòa
        if (!s.ToaMongMuonId.HasValue || s.ToaMongMuonId == r.ToaId)
        {
            score += 20 * wPref;
        }

        // 3. Đúng tầng
        if (!s.TangMongMuon.HasValue || s.TangMongMuon == r.Tang)
        {
            score += 15 * wPref;
        }

        // 4. Số bạn cùng khoa trong phòng
        int sameFacultyCount = r.Occupants.Count(o => o.MSSV != s.MSSV && o.Khoa == s.Khoa);
        score += sameFacultyCount * 8 * wRoommate;

        // 5. Số bạn cùng khóa trong phòng
        int sameCourseCount = r.Occupants.Count(o => o.MSSV != s.MSSV && o.KhoaHoc == s.KhoaHoc);
        score += sameCourseCount * 6 * wRoommate;

        // 6. Nhóm bạn đăng ký chung phòng
        if (!string.IsNullOrEmpty(s.MaNhom))
        {
            int sameGroupCount = r.Occupants.Count(o => o.MSSV != s.MSSV && o.MaNhom == s.MaNhom);
            score += sameGroupCount * 30 * wRoommate;
        }

        // 7. Phòng đã có người (khuyến khích gom kín phòng)
        if (r.Occupants.Count > 0)
        {
            score += 5;
        }

        return Math.Round(score, 2);
    }

    public List<ManualAdjustmentOptionDto> EvaluateManualMoves(
        int donId,
        AllocationResultDto currentResult,
        SystemConfigDto config)
    {
        var targetAssignment = currentResult.Assignments.FirstOrDefault(a => a.DonId == donId);
        if (targetAssignment == null) return new List<ManualAdjustmentOptionDto>();

        // Giả lập danh sách phòng và số chỗ từ currentResult
        var roomAssignments = currentResult.Assignments.GroupBy(a => a.PhongId).ToDictionary(g => g.Key, g => g.ToList());

        var options = new List<ManualAdjustmentOptionDto>();

        var distinctRooms = currentResult.Assignments
            .Select(a => new { a.PhongId, a.MaPhong, a.ToaTen, a.Tang, a.GioiTinh })
            .DistinctBy(a => a.PhongId)
            .ToList();

        foreach (var r in distinctRooms)
        {
            if (r.GioiTinh != targetAssignment.GioiTinh) continue; // Ràng buộc giới tính
            if (r.PhongId == targetAssignment.PhongId) continue;   // Đang ở phòng này rồi

            var occupantsInTarget = roomAssignments.GetValueOrDefault(r.PhongId, new List<AllocationAssignmentDto>());
            int capacity = 6; // Tiêu chuẩn 6 chỗ
            int freeBeds = capacity - occupantsInTarget.Count;
            if (freeBeds <= 0) continue; // Hết chỗ trống

            var fakeRoom = new RoomMemory
            {
                Id = r.PhongId,
                MaPhong = r.MaPhong,
                ToaTen = r.ToaTen,
                Tang = r.Tang,
                GioiTinh = r.GioiTinh,
                SucChua = capacity,
                Occupants = occupantsInTarget.Select(o => new StudentCandidate
                {
                    MSSV = o.MSSV,
                    HoTen = o.HoTen,
                    Khoa = o.Khoa,
                    KhoaHoc = o.KhoaHoc,
                    MaNhom = o.MaNhom
                }).ToList()
            };

            var student = new StudentCandidate
            {
                DonId = targetAssignment.DonId,
                MSSV = targetAssignment.MSSV,
                HoTen = targetAssignment.HoTen,
                Khoa = targetAssignment.Khoa,
                KhoaHoc = targetAssignment.KhoaHoc,
                MaNhom = targetAssignment.MaNhom,
                GioiTinh = targetAssignment.GioiTinh
            };

            decimal newScore = CalculateStudentFitness(student, fakeRoom, config);

            options.Add(new ManualAdjustmentOptionDto
            {
                PhongId = r.PhongId,
                MaPhong = r.MaPhong,
                ToaTen = r.ToaTen,
                Tang = r.Tang,
                SucChua = capacity,
                ChoTrong = freeBeds,
                CurrentScore = targetAssignment.DiemPhuHop,
                NewScore = newScore
            });
        }

        // Sắp xếp theo DeltaScore giảm dần (các phòng cải thiện điểm nhiều nhất đứng đầu)
        return options.OrderByDescending(o => o.DeltaScore).ToList();
    }

    public bool ApplyManualMove(
        int donId,
        int targetRoomId,
        AllocationResultDto currentResult,
        SystemConfigDto config)
    {
        var targetAssignment = currentResult.Assignments.FirstOrDefault(a => a.DonId == donId);
        if (targetAssignment == null) return false;

        var targetRoomOption = EvaluateManualMoves(donId, currentResult, config).FirstOrDefault(o => o.PhongId == targetRoomId);
        if (targetRoomOption == null) return false;

        targetAssignment.PhongId = targetRoomOption.PhongId;
        targetAssignment.MaPhong = targetRoomOption.MaPhong;
        targetAssignment.ToaTen = targetRoomOption.ToaTen;
        targetAssignment.Tang = targetRoomOption.Tang;
        targetAssignment.DiemPhuHop = targetRoomOption.NewScore;
        targetAssignment.ChinhTay = true;

        // Cập nhật lại tổng điểm phiên
        currentResult.TotalFitnessScore = (double)currentResult.Assignments.Sum(a => a.DiemPhuHop);
        currentResult.AverageFitnessScore = Math.Round(currentResult.TotalFitnessScore / currentResult.Assignments.Count, 2);

        return true;
    }

    public async Task<bool> CommitAllocationToContractsAsync(
        int phienId,
        AllocationResultDto result,
        DateTime contractStart,
        DateTime contractEnd,
        CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        try
        {
            // 1. Tạo bản ghi PhienXepPhong
            var phien = new PhienXepPhong
            {
                DotId = 1, // Default or from batch
                ThuatToan = "Greedy + LocalSearch (2-Opt)",
                ThoiGian = DateTime.Now,
                DaChot = true,
                ThongKeJson = JsonSerializer.Serialize(new
                {
                    result.TotalApplications,
                    result.AssignedCount,
                    result.WaitingCount,
                    result.TotalFitnessScore,
                    result.AverageFitnessScore,
                    result.ExecutionTimeMs,
                    result.RoommateKeepRate,
                    result.PreferenceMatchRate,
                    result.RoomOccupancyRate
                })
            };

            db.PhienXepPhongs.Add(phien);
            await db.SaveChangesAsync(ct);

            // 2. Tạo chi tiết xếp phòng và sinh Hợp đồng lưu trú
            var roomPrices = await db.Phongs.Include(p => p.LoaiPhong).ToDictionaryAsync(p => p.Id, p => p.LoaiPhong.GiaThang, ct);
            var studentPolicyDiscount = await db.SinhViens.Include(s => s.DienUuTien).ToDictionaryAsync(
                s => s.MSSV,
                s => s.DienUuTien != null ? (decimal)s.DienUuTien.DiemCong : 0m,
                ct);

            var applicationIds = result.Assignments.Select(a => a.DonId).ToList();
            var applications = await db.DonDangKys.Where(d => applicationIds.Contains(d.Id)).ToListAsync(ct);
            var appDict = applications.ToDictionary(d => d.Id);

            foreach (var item in result.Assignments)
            {
                // ChiTietXepPhong
                var detail = new ChiTietXepPhong
                {
                    PhienId = phien.Id,
                    DonId = item.DonId,
                    PhongId = item.PhongId,
                    DiemPhuHop = item.DiemPhuHop,
                    ChinhTay = item.ChinhTay
                };
                db.ChiTietXepPhongs.Add(detail);

                // Cập nhật trạng thái đơn
                if (appDict.TryGetValue(item.DonId, out var app))
                {
                    app.TrangThai = TrangThaiDonDangKy.DaXep;
                }

                // Sinh Hợp đồng (HopDong)
                decimal monthlyPrice = roomPrices.GetValueOrDefault(item.PhongId, 600000m);
                decimal discountPercent = Math.Min(50m, studentPolicyDiscount.GetValueOrDefault(item.MSSV, 0m));

                var contract = new HopDong
                {
                    MSSV = item.MSSV,
                    PhongId = item.PhongId,
                    NgayBatDau = contractStart,
                    NgayKetThuc = contractEnd,
                    GiaThang = monthlyPrice,
                    MienGiamPhanTram = discountPercent,
                    TienCoc = monthlyPrice, // Cọc 1 tháng tiền phòng
                    TrangThai = TrangThaiHopDong.HieuLuc
                };
                db.HopDongs.Add(contract);
            }

            // Cập nhật trạng thái các đơn trong danh sách chờ
            var waitingIds = result.WaitingList.Select(w => w.Id).ToList();
            var waitingApps = await db.DonDangKys.Where(d => waitingIds.Contains(d.Id)).ToListAsync(ct);
            foreach (var w in waitingApps)
            {
                w.TrangThai = TrangThaiDonDangKy.DanhSachCho;
            }

            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            result.PhienId = phien.Id;
            return true;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}

using KTX.Domain.Enums;

namespace KTX.Domain.DTOs;

public class DotDangKyDto
{
    public int Id { get; set; }
    public string Ten { get; set; } = string.Empty;
    public DateTime NgayMo { get; set; }
    public DateTime NgayDong { get; set; }
    public bool DangMo { get; set; }
    public int SoDon { get; set; }
}

public class DonDangKyDto
{
    public int Id { get; set; }
    public int DotId { get; set; }
    public string MSSV { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public GioiTinh GioiTinh { get; set; }
    public string CCCD { get; set; } = string.Empty;
    public string Khoa { get; set; } = string.Empty;
    public int KhoaHoc { get; set; }
    public string TinhThanh { get; set; } = string.Empty;
    public int KhoangCachKm { get; set; }
    public int NhomKhoangCach { get; set; }
    public string? DienUuTien { get; set; }
    public int DiemCongDien { get; set; }
    public decimal DiemUuTien { get; set; }

    public int? NhomId { get; set; }
    public string? MaNhom { get; set; }

    public int? LoaiPhongMongMuonId { get; set; }
    public string? LoaiPhongMongMuonTen { get; set; }
    public bool BatBuocLoaiPhong { get; set; }

    public int? ToaMongMuonId { get; set; }
    public string? ToaMongMuonTen { get; set; }
    public int? TangMongMuon { get; set; }

    public DateTime ThoiGianNop { get; set; }
    public TrangThaiDonDangKy TrangThai { get; set; }
}

public class AllocationProgressDto
{
    public string StepName { get; set; } = string.Empty;
    public int TotalUnits { get; set; }
    public int UnitsProcessed { get; set; }
    public int AssignedCount { get; set; }
    public int WaitingCount { get; set; }
    public double CurrentScore { get; set; }
    public int Percentage => TotalUnits > 0 ? (int)((double)UnitsProcessed / TotalUnits * 100) : 0;

    // Room Id -> Number of occupied beds for real-time map coloring
    public Dictionary<int, int> RoomOccupancyMap { get; set; } = new();
}

public class AllocationAssignmentDto
{
    public int DonId { get; set; }
    public string MSSV { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public GioiTinh GioiTinh { get; set; }
    public string Khoa { get; set; } = string.Empty;
    public int KhoaHoc { get; set; }
    public string? MaNhom { get; set; }

    public int PhongId { get; set; }
    public string MaPhong { get; set; } = string.Empty;
    public string ToaTen { get; set; } = string.Empty;
    public int Tang { get; set; }
    public decimal DiemPhuHop { get; set; }
    public bool ChinhTay { get; set; }
}

public class AllocationResultDto
{
    public int PhienId { get; set; }
    public int TotalApplications { get; set; }
    public int AssignedCount { get; set; }
    public int WaitingCount { get; set; }
    public double TotalFitnessScore { get; set; }
    public double AverageFitnessScore { get; set; }
    public long ExecutionTimeMs { get; set; }

    public double RoommateKeepRate { get; set; }      // Tỉ lệ giữ nhóm (%)
    public double PreferenceMatchRate { get; set; }   // Tỉ lệ đúng nguyện vọng loại phòng/tầng (%)
    public double RoomOccupancyRate { get; set; }     // Tỉ lệ lấp đầy giường (%)

    public List<AllocationAssignmentDto> Assignments { get; set; } = new();
    public List<DonDangKyDto> WaitingList { get; set; } = new();
}

public class ManualAdjustmentOptionDto
{
    public int PhongId { get; set; }
    public string MaPhong { get; set; } = string.Empty;
    public string ToaTen { get; set; } = string.Empty;
    public int Tang { get; set; }
    public int SucChua { get; set; }
    public int ChoTrong { get; set; }
    public decimal CurrentScore { get; set; }
    public decimal NewScore { get; set; }
    public decimal DeltaScore => NewScore - CurrentScore;
}

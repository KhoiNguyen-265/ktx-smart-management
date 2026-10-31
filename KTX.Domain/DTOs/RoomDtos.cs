using KTX.Domain.Enums;

namespace KTX.Domain.DTOs;

public class BuildingDto
{
    public int Id { get; set; }
    public string Ten { get; set; } = string.Empty;
    public string GioiTinhText { get; set; } = string.Empty;
    public int SoTang { get; set; }
    public int TongSoPhong { get; set; }
}

public class RoomTypeDto
{
    public int Id { get; set; }
    public string Ten { get; set; } = string.Empty;
    public int SucChua { get; set; }
    public decimal GiaThang { get; set; }
    public bool CoDieuHoa { get; set; }
}

public class RoomSummaryDto
{
    public int Id { get; set; }
    public string MaPhong { get; set; } = string.Empty;
    public int ToaId { get; set; }
    public string TenToa { get; set; } = string.Empty;
    public int Tang { get; set; }
    public int LoaiPhongId { get; set; }
    public string TenLoaiPhong { get; set; } = string.Empty;
    public int SucChua { get; set; }
    public decimal GiaThang { get; set; }
    public bool CoDieuHoa { get; set; }
    public TrangThaiPhong TrangThai { get; set; }

    public int SoNguoiDangO { get; set; }
    public int SoChoTrong => Math.Max(0, SucChua - SoNguoiDangO);

    public List<string> TenSinhVienDangO { get; set; } = new();
}

public class RoomDetailDto
{
    public RoomSummaryDto Summary { get; set; } = new();
    public List<RoomResidentDto> Residents { get; set; } = new();
    public List<RoomAssetDto> Assets { get; set; } = new();
    public MeterReadingDto? LatestMeterReading { get; set; }
}

public class RoomResidentDto
{
    public string MSSV { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string Khoa { get; set; } = string.Empty;
    public int KhoaHoc { get; set; }
    public string SoDienThoai { get; set; } = string.Empty;
    public DateTime NgayBatDau { get; set; }
    public DateTime NgayKetThuc { get; set; }
    public decimal GiaThang { get; set; }
}

public class RoomAssetDto
{
    public int Id { get; set; }
    public string TenTaiSan { get; set; } = string.Empty;
    public int SoLuong { get; set; }
    public string TinhTrang { get; set; } = string.Empty;
}

public class MeterReadingDto
{
    public int Thang { get; set; }
    public int Nam { get; set; }
    public decimal DienMoi { get; set; }
    public decimal SanLuongDien { get; set; }
    public decimal NuocMoi { get; set; }
    public decimal SanLuongNuoc { get; set; }
}

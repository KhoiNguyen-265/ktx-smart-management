using KTX.Domain.Enums;

namespace KTX.Domain.DTOs;

public class StudentPortalOverviewDto
{
    public string MSSV { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public GioiTinh GioiTinh { get; set; }
    public string GioiTinhText => GioiTinh == GioiTinh.Nam ? "Nam" : "Nữ";
    public DateTime NgaySinh { get; set; }
    public string CCCD { get; set; } = string.Empty;
    public string Khoa { get; set; } = string.Empty;
    public int KhoaHoc { get; set; }
    public string SoDienThoai { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string QueQuan { get; set; } = string.Empty;
    public string DienUuTien { get; set; } = string.Empty;

    // Hợp đồng & Phòng hiện tại
    public int? ActiveContractId { get; set; }
    public int? PhongId { get; set; }
    public string MaPhong { get; set; } = "Chưa xếp phòng";
    public string TenToa { get; set; } = "---";
    public int Tang { get; set; }
    public string TenLoaiPhong { get; set; } = "---";
    public int SucChua { get; set; }
    public decimal GiaThang { get; set; }
    public decimal MienGiamPhanTram { get; set; }
    public decimal TienCoc { get; set; }
    public DateTime? NgayBatDau { get; set; }
    public DateTime? NgayKetThuc { get; set; }
    public int SoNgayConLai => NgayKetThuc.HasValue ? Math.Max(0, (NgayKetThuc.Value.Date - DateTime.Today).Days) : 0;

    // Tổng công nợ cá nhân & phòng
    public decimal TongNoTienPhong { get; set; }
    public decimal TongNoDienNuocPhong { get; set; }
    public int SoSuCoDangXuLy { get; set; }

    public List<RoomResidentDto> Roommates { get; set; } = new();
    public List<RoomAssetDto> RoomAssets { get; set; } = new();
}

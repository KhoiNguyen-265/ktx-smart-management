namespace KTX.Domain.Entities;

public class VaiTro
{
    public int Id { get; set; }
    public string TenVaiTro { get; set; } = string.Empty; // "Admin", "QuanLy", "KeToan"

    public ICollection<NguoiDung> NguoiDungs { get; set; } = new List<NguoiDung>();
}

public class NguoiDung
{
    public int Id { get; set; }
    public string TenDangNhap { get; set; } = string.Empty;
    public string MatKhauHash { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public int MaVaiTro { get; set; }
    public VaiTro VaiTro { get; set; } = null!;
    public bool HoatDong { get; set; } = true;
    public int SoLanSai { get; set; }
    public DateTime? KhoaDen { get; set; }

    public ICollection<NhatKy> NhatKys { get; set; } = new List<NhatKy>();
    public ICollection<ThanhToan> ThanhToansThu { get; set; } = new List<ThanhToan>();
}

public class NhatKy
{
    public long Id { get; set; }
    public int? NguoiDungId { get; set; }
    public NguoiDung? NguoiDung { get; set; }
    public string HanhDong { get; set; } = string.Empty;
    public string ChiTiet { get; set; } = string.Empty;
    public string? BangLienQuan { get; set; }
    public string? KhoaChinhLienQuan { get; set; }
    public DateTime ThoiGian { get; set; } = DateTime.Now;
}

public class CauHinh
{
    public int Id { get; set; }
    public string Khoa { get; set; } = string.Empty;
    public string GiaTri { get; set; } = string.Empty;
    public string MoTa { get; set; } = string.Empty;
}

public class YeuCauSuaChua
{
    public int Id { get; set; }
    public int PhongId { get; set; }
    public Phong Phong { get; set; } = null!;
    public string MoTa { get; set; } = string.Empty;
    public string TrangThai { get; set; } = "ChuaXuLy"; // "ChuaXuLy", "DangXuLy", "HoanThanh"
    public DateTime NgayTao { get; set; } = DateTime.Now;
    public DateTime? NgayHoanThanh { get; set; }
    public decimal ChiPhi { get; set; }
    public string? GhiChu { get; set; }
}

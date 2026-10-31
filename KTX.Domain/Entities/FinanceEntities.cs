using KTX.Domain.Enums;

namespace KTX.Domain.Entities;

public class BacGia
{
    public int Id { get; set; }
    public LoaiDichVu LoaiDichVu { get; set; }
    public int Bac { get; set; }
    public decimal TuMuc { get; set; }
    public decimal? DenMuc { get; set; } // null = không giới hạn
    public decimal DonGia { get; set; }
    public DateTime ApDungTu { get; set; }
}

public class ChiSoDienNuoc
{
    public int Id { get; set; }
    public int PhongId { get; set; }
    public Phong Phong { get; set; } = null!;
    public int Thang { get; set; }
    public int Nam { get; set; }

    // Điện
    public decimal DienCu { get; set; }
    public decimal DienMoi { get; set; }
    public bool CoThayDongHoDien { get; set; }
    public decimal? DienChotDongHoCu { get; set; }
    public decimal? DienDauDongHoMoi { get; set; }
    public decimal SanLuongDien { get; set; }

    // Nước
    public decimal NuocCu { get; set; }
    public decimal NuocMoi { get; set; }
    public bool CoThayDongHoNuoc { get; set; }
    public decimal? NuocChotDongHoCu { get; set; }
    public decimal? NuocDauDongHoMoi { get; set; }
    public decimal SanLuongNuoc { get; set; }

    public string? GhiChu { get; set; }
}

public class HoaDonDienNuoc
{
    public int Id { get; set; }
    public string MaThanhToan { get; set; } = string.Empty; // KTXDN...
    public int PhongId { get; set; }
    public Phong Phong { get; set; } = null!;
    public int Thang { get; set; }
    public int Nam { get; set; }
    public int SoNguoiTinhDinhMuc { get; set; }
    public decimal TongTien { get; set; }
    public decimal DaThu { get; set; }
    public TrangThaiHoaDon TrangThai { get; set; } = TrangThaiHoaDon.ChuaThanhToan;
    public byte[]? RowVersion { get; set; }

    public ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();
    public ICollection<ThanhToan> ThanhToans { get; set; } = new List<ThanhToan>();
}

public class ChiTietHoaDon
{
    public int Id { get; set; }
    public int HoaDonId { get; set; }
    public HoaDonDienNuoc HoaDonDienNuoc { get; set; } = null!;
    public string NoiDung { get; set; } = string.Empty;
    public decimal SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien { get; set; }
}

public class KhoanThuTienPhong
{
    public int Id { get; set; }
    public string MaThanhToan { get; set; } = string.Empty; // KTXTP...
    public int HopDongId { get; set; }
    public HopDong HopDong { get; set; } = null!;
    public DateTime TuNgay { get; set; }
    public DateTime DenNgay { get; set; }
    public decimal SoTienGoc { get; set; }
    public decimal MienGiam { get; set; }
    public decimal PhaiThu { get; set; }
    public decimal DaThu { get; set; }
    public TrangThaiHoaDon TrangThai { get; set; } = TrangThaiHoaDon.ChuaThanhToan;
    public byte[]? RowVersion { get; set; }

    public ICollection<ThanhToan> ThanhToans { get; set; } = new List<ThanhToan>();
}

public class GiaoDichNganHang
{
    public int Id { get; set; }
    public DateTime NgayGD { get; set; }
    public decimal SoTien { get; set; }
    public string NoiDungGoc { get; set; } = string.Empty;
    public string? MaGiaoDich { get; set; }
    public decimal? SoDuSau { get; set; }
    public string HashSaoKe { get; set; } = string.Empty;
    public TrangThaiGiaoDich TrangThaiKhop { get; set; } = TrangThaiGiaoDich.CanXuLy;
    public string FileNguon { get; set; } = string.Empty;
    public DateTime NgayImport { get; set; } = DateTime.Now;

    public ICollection<ThanhToan> ThanhToans { get; set; } = new List<ThanhToan>();
}

public class ThanhToan
{
    public int Id { get; set; }

    public int? HoaDonDienNuocId { get; set; }
    public HoaDonDienNuoc? HoaDonDienNuoc { get; set; }

    public int? KhoanThuTienPhongId { get; set; }
    public KhoanThuTienPhong? KhoanThuTienPhong { get; set; }

    public int? GiaoDichNganHangId { get; set; }
    public GiaoDichNganHang? GiaoDichNganHang { get; set; }

    public decimal SoTien { get; set; }
    public HinhThucThanhToan HinhThuc { get; set; } = HinhThucThanhToan.ChuyenKhoan;
    public DateTime ThoiGian { get; set; } = DateTime.Now;
    public int NguoiThuId { get; set; }
    public NguoiDung NguoiThu { get; set; } = null!;
}

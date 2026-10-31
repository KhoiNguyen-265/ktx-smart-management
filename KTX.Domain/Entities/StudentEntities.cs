using KTX.Domain.Enums;

namespace KTX.Domain.Entities;

public class TinhThanh
{
    public int Id { get; set; }
    public string Ten { get; set; } = string.Empty;
    public int KhoangCachKm { get; set; }
    public int NhomKhoangCach { get; set; } // 1: <100km, 2: 100-300km, 3: >300km

    public ICollection<TinhThanhCu> TinhThanhCus { get; set; } = new List<TinhThanhCu>();
    public ICollection<SinhVien> SinhViens { get; set; } = new List<SinhVien>();
}

public class TinhThanhCu
{
    public int Id { get; set; }
    public string TenCu { get; set; } = string.Empty;
    public int TinhThanhId { get; set; }
    public TinhThanh TinhThanh { get; set; } = null!;
}

public class DienUuTien
{
    public int Id { get; set; }
    public string Ten { get; set; } = string.Empty;
    public int DiemCong { get; set; }

    public ICollection<SinhVien> SinhViens { get; set; } = new List<SinhVien>();
}

public class SinhVien
{
    public string MSSV { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public DateTime NgaySinh { get; set; }
    public GioiTinh GioiTinh { get; set; }
    public string CCCD { get; set; } = string.Empty;
    public string Khoa { get; set; } = string.Empty;
    public int KhoaHoc { get; set; }
    public string SoDienThoai { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public int TinhThanhId { get; set; }
    public TinhThanh TinhThanh { get; set; } = null!;

    public int? DienUuTienId { get; set; }
    public DienUuTien? DienUuTien { get; set; }

    public ICollection<DonDangKy> DonDangKys { get; set; } = new List<DonDangKy>();
    public ICollection<HopDong> HopDongs { get; set; } = new List<HopDong>();
}

using KTX.Domain.Enums;

namespace KTX.Domain.Entities;

public class ToaNha
{
    public int Id { get; set; }
    public string Ten { get; set; } = string.Empty;
    public GioiTinh GioiTinh { get; set; }
    public int SoTang { get; set; }

    public ICollection<Phong> Phongs { get; set; } = new List<Phong>();
}

public class LoaiPhong
{
    public int Id { get; set; }
    public string Ten { get; set; } = string.Empty;
    public int SucChua { get; set; }
    public decimal GiaThang { get; set; }
    public bool CoDieuHoa { get; set; }

    public ICollection<Phong> Phongs { get; set; } = new List<Phong>();
}

public class Phong
{
    public int Id { get; set; }
    public string MaPhong { get; set; } = string.Empty;
    public int ToaId { get; set; }
    public ToaNha ToaNha { get; set; } = null!;
    public int Tang { get; set; }
    public int LoaiPhongId { get; set; }
    public LoaiPhong LoaiPhong { get; set; } = null!;
    public TrangThaiPhong TrangThai { get; set; } = TrangThaiPhong.HoatDong;
    public byte[]? RowVersion { get; set; }

    public ICollection<TaiSanPhong> TaiSanPhongs { get; set; } = new List<TaiSanPhong>();
    public ICollection<HopDong> HopDongs { get; set; } = new List<HopDong>();
    public ICollection<ChiSoDienNuoc> ChiSoDienNuocs { get; set; } = new List<ChiSoDienNuoc>();
    public ICollection<HoaDonDienNuoc> HoaDonDienNuocs { get; set; } = new List<HoaDonDienNuoc>();
    public ICollection<ChiTietXepPhong> ChiTietXepPhongs { get; set; } = new List<ChiTietXepPhong>();
}

public class TaiSanPhong
{
    public int Id { get; set; }
    public int PhongId { get; set; }
    public Phong Phong { get; set; } = null!;
    public string TenTaiSan { get; set; } = string.Empty;
    public int SoLuong { get; set; } = 1;
    public string TinhTrang { get; set; } = "Tốt";
}

using KTX.Domain.Enums;

namespace KTX.Domain.Entities;

public class HopDong
{
    public int Id { get; set; }
    public string MSSV { get; set; } = string.Empty;
    public SinhVien SinhVien { get; set; } = null!;

    public int PhongId { get; set; }
    public Phong Phong { get; set; } = null!;

    public DateTime NgayBatDau { get; set; }
    public DateTime NgayKetThuc { get; set; }
    public decimal GiaThang { get; set; }
    public decimal MienGiamPhanTram { get; set; }
    public decimal TienCoc { get; set; }
    public TrangThaiHopDong TrangThai { get; set; } = TrangThaiHopDong.HieuLuc;
    public byte[]? RowVersion { get; set; }

    public ICollection<KhoanThuTienPhong> KhoanThuTienPhongs { get; set; } = new List<KhoanThuTienPhong>();
    public ICollection<LichSuChuyenPhong> LichSuChuyenPhongs { get; set; } = new List<LichSuChuyenPhong>();
}

public class LichSuChuyenPhong
{
    public int Id { get; set; }
    public int HopDongId { get; set; }
    public HopDong HopDong { get; set; } = null!;

    public int TuPhongId { get; set; }
    public Phong TuPhong { get; set; } = null!;

    public int DenPhongId { get; set; }
    public Phong DenPhong { get; set; } = null!;

    public DateTime NgayChuyen { get; set; } = DateTime.Now;
    public string LyDo { get; set; } = string.Empty;
}

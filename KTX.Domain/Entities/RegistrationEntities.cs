using KTX.Domain.Enums;

namespace KTX.Domain.Entities;

public class DotDangKy
{
    public int Id { get; set; }
    public string Ten { get; set; } = string.Empty;
    public DateTime NgayMo { get; set; }
    public DateTime NgayDong { get; set; }
    public bool DangMo { get; set; } = true;

    public ICollection<DonDangKy> DonDangKys { get; set; } = new List<DonDangKy>();
    public ICollection<PhienXepPhong> PhienXepPhongs { get; set; } = new List<PhienXepPhong>();
}

public class NhomDangKy
{
    public int Id { get; set; }
    public string MaNhom { get; set; } = string.Empty;
    public string MSSVTruongNhom { get; set; } = string.Empty;

    public ICollection<DonDangKy> DonDangKys { get; set; } = new List<DonDangKy>();
}

public class DonDangKy
{
    public int Id { get; set; }
    public string MSSV { get; set; } = string.Empty;
    public SinhVien SinhVien { get; set; } = null!;

    public int DotId { get; set; }
    public DotDangKy DotDangKy { get; set; } = null!;

    public int? NhomId { get; set; }
    public NhomDangKy? NhomDangKy { get; set; }

    public int? LoaiPhongMongMuonId { get; set; }
    public LoaiPhong? LoaiPhongMongMuon { get; set; }
    public bool BatBuocLoaiPhong { get; set; }

    public int? ToaMongMuonId { get; set; }
    public ToaNha? ToaMongMuon { get; set; }
    public int? TangMongMuon { get; set; }

    public decimal DiemUuTien { get; set; }
    public DateTime ThoiGianNop { get; set; } = DateTime.Now;
    public TrangThaiDonDangKy TrangThai { get; set; } = TrangThaiDonDangKy.ChoXep;

    public ICollection<ChiTietXepPhong> ChiTietXepPhongs { get; set; } = new List<ChiTietXepPhong>();
}

public class PhienXepPhong
{
    public int Id { get; set; }
    public int DotId { get; set; }
    public DotDangKy DotDangKy { get; set; } = null!;
    public string ThuatToan { get; set; } = "Greedy+LocalSearch"; // hoặc "OrTools"
    public DateTime ThoiGian { get; set; } = DateTime.Now;
    public string ThongKeJson { get; set; } = "{}";
    public bool DaChot { get; set; }

    public ICollection<ChiTietXepPhong> ChiTietXepPhongs { get; set; } = new List<ChiTietXepPhong>();
}

public class ChiTietXepPhong
{
    public int Id { get; set; }
    public int PhienId { get; set; }
    public PhienXepPhong PhienXepPhong { get; set; } = null!;

    public int DonId { get; set; }
    public DonDangKy DonDangKy { get; set; } = null!;

    public int PhongId { get; set; }
    public Phong Phong { get; set; } = null!;

    public decimal DiemPhuHop { get; set; }
    public bool ChinhTay { get; set; }
}

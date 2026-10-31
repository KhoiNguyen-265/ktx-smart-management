using KTX.Domain.Enums;

namespace KTX.Domain.DTOs;

public class ContractDto
{
    public int Id { get; set; }
    public string MSSV { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public GioiTinh GioiTinh { get; set; }
    public string CCCD { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public string Khoa { get; set; } = string.Empty;
    public int KhoaHoc { get; set; }

    public int PhongId { get; set; }
    public string MaPhong { get; set; } = string.Empty;
    public string ToaTen { get; set; } = string.Empty;
    public int Tang { get; set; }
    public int SucChua { get; set; }

    public DateTime NgayBatDau { get; set; }
    public DateTime NgayKetThuc { get; set; }
    public decimal GiaThang { get; set; }
    public decimal MienGiamPhanTram { get; set; }
    public decimal GiaThucTe => Math.Round(GiaThang * (1 - MienGiamPhanTram / 100m), 0);
    public decimal TienCoc { get; set; }
    public TrangThaiHopDong TrangThai { get; set; }

    public int SoNgayConLai => (NgayKetThuc.Date - DateTime.Now.Date).Days;
    public bool IsExpiringSoon => TrangThai == TrangThaiHopDong.HieuLuc && SoNgayConLai >= 0 && SoNgayConLai <= 30;
    public bool IsExpired => TrangThai == TrangThaiHopDong.HieuLuc && SoNgayConLai < 0;
}

public class ContractCreateDto
{
    public string MSSV { get; set; } = string.Empty;
    public int PhongId { get; set; }
    public DateTime NgayBatDau { get; set; }
    public DateTime NgayKetThuc { get; set; }
    public decimal GiaThang { get; set; }
    public decimal MienGiamPhanTram { get; set; }
    public decimal TienCoc { get; set; }
}

public class RoomTransferRequestDto
{
    public int HopDongId { get; set; }
    public int DenPhongId { get; set; }
    public string LyDo { get; set; } = string.Empty;
}

public class ContractCheckOutDto
{
    public int HopDongId { get; set; }
    public decimal TienHongHocTaiSan { get; set; }
    public decimal TienNoDienNuocKhauTru { get; set; }
    public string GhiChu { get; set; } = string.Empty;
}

public class ContractCheckOutResultDto
{
    public bool Success { get; set; }
    public decimal TienCocBanDau { get; set; }
    public decimal TongKhauTru { get; set; }
    public decimal TienHoanTra { get; set; }
    public string Message { get; set; } = string.Empty;
}

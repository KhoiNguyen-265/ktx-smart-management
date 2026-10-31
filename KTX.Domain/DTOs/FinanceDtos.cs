using KTX.Domain.Enums;

namespace KTX.Domain.DTOs;

public class UtilityReadingDto
{
    public int Id { get; set; }
    public int PhongId { get; set; }
    public string MaPhong { get; set; } = string.Empty;
    public string ToaTen { get; set; } = string.Empty;
    public int Thang { get; set; }
    public int Nam { get; set; }

    public decimal DienCu { get; set; }
    public decimal DienMoi { get; set; }
    public bool CoThayDongHoDien { get; set; }
    public decimal? DienChotDongHoCu { get; set; }
    public decimal? DienDauDongHoMoi { get; set; }
    public decimal SanLuongDien { get; set; }

    public decimal NuocCu { get; set; }
    public decimal NuocMoi { get; set; }
    public bool CoThayDongHoNuoc { get; set; }
    public decimal? NuocChotDongHoCu { get; set; }
    public decimal? NuocDauDongHoMoi { get; set; }
    public decimal SanLuongNuoc { get; set; }

    public string? GhiChu { get; set; }
    public bool CanhBaoTieuThuBatThuong { get; set; }
}

public class UtilityReadingSaveDto
{
    public int PhongId { get; set; }
    public int Thang { get; set; }
    public int Nam { get; set; }

    public decimal DienCu { get; set; }
    public decimal DienMoi { get; set; }
    public bool CoThayDongHoDien { get; set; }
    public decimal? DienChotDongHoCu { get; set; }
    public decimal? DienDauDongHoMoi { get; set; }

    public decimal NuocCu { get; set; }
    public decimal NuocMoi { get; set; }
    public bool CoThayDongHoNuoc { get; set; }
    public decimal? NuocChotDongHoCu { get; set; }
    public decimal? NuocDauDongHoMoi { get; set; }

    public string? GhiChu { get; set; }
}

public class TierDetailDto
{
    public int Bac { get; set; }
    public LoaiDichVu LoaiDichVu { get; set; }
    public string NoiDung { get; set; } = string.Empty;
    public decimal TuMuc { get; set; }
    public decimal? DenMuc { get; set; }
    public decimal SanLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien => Math.Round(SanLuong * DonGia, 0);
}

public class TieredCalculationResultDto
{
    public decimal SanLuongDien { get; set; }
    public decimal SanLuongNuoc { get; set; }
    public int SoNguoiDinhMuc { get; set; }
    public int HeSoDinhMuc { get; set; } // m = ceil(SoNguoi / 4)

    public List<TierDetailDto> ChiTietTiers { get; set; } = new();

    public decimal TienDien { get; set; }
    public decimal TienNuoc { get; set; }
    public decimal PhiDichVuChung { get; set; }
    public decimal TienVat { get; set; }
    public decimal TongCong => TienDien + TienNuoc + PhiDichVuChung + TienVat;
}

public class InvoiceDto
{
    public int Id { get; set; }
    public string MaThanhToan { get; set; } = string.Empty; // KTXDN...
    public int PhongId { get; set; }
    public string MaPhong { get; set; } = string.Empty;
    public string ToaTen { get; set; } = string.Empty;
    public int Thang { get; set; }
    public int Nam { get; set; }
    public int SoNguoiTinhDinhMuc { get; set; }
    public decimal TongTien { get; set; }
    public decimal DaThu { get; set; }
    public decimal ConNo => Math.Max(0m, TongTien - DaThu);
    public TrangThaiHoaDon TrangThai { get; set; }
    public string VietQrPayload { get; set; } = string.Empty;
    public List<string> ChiTietRows { get; set; } = new();
}

public class RoomFeeDto
{
    public int Id { get; set; }
    public string MaThanhToan { get; set; } = string.Empty; // KTXTP...
    public int HopDongId { get; set; }
    public string MSSV { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string MaPhong { get; set; } = string.Empty;
    public string ToaTen { get; set; } = string.Empty;
    public DateTime TuNgay { get; set; }
    public DateTime DenNgay { get; set; }
    public decimal SoTienGoc { get; set; }
    public decimal MienGiam { get; set; }
    public decimal PhaiThu { get; set; }
    public decimal DaThu { get; set; }
    public decimal ConNo => Math.Max(0m, PhaiThu - DaThu);
    public TrangThaiHoaDon TrangThai { get; set; }
    public string VietQrPayload { get; set; } = string.Empty;
}

public class BankTransactionDto
{
    public int Id { get; set; }
    public DateTime NgayGD { get; set; }
    public decimal SoTien { get; set; }
    public string NoiDungGoc { get; set; } = string.Empty;
    public string? MaGiaoDich { get; set; }
    public decimal? SoDuSau { get; set; }
    public string HashSaoKe { get; set; } = string.Empty;
    public TrangThaiGiaoDich TrangThaiKhop { get; set; }
    public string? MaHoaDonNhanDien { get; set; }
    public string? ChiTietKhop { get; set; }
}

public class ReconcileSummaryDto
{
    public int TotalRows { get; set; }
    public int DuplicateCount { get; set; }
    public int MatchedCount { get; set; }
    public int PartialCount { get; set; }
    public int UnmatchedCount { get; set; }
    public decimal TotalAmount { get; set; }
    public List<BankTransactionDto> Transactions { get; set; } = new();
}

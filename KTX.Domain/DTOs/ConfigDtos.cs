namespace KTX.Domain.DTOs;

public class SystemConfigDto
{
    // Trọng số thuật toán xếp phòng (Tổng nên là 1.0 hoặc 100)
    public double WPriority { get; set; } = 0.35;      // w_dien: Điểm diện ưu tiên
    public double WDistance { get; set; } = 0.25;      // w_khoangcach: Khoảng cách quê quán
    public double WCourse { get; set; } = 0.15;        // w_khoahoc: Ưu tiên K mới
    public double WPreference { get; set; } = 0.15;    // w_nguyenvong: Nguyện vọng loại phòng / tầng
    public double WRoommate { get; set; } = 0.10;      // w_nhom: Giữ nhóm bạn ở cùng nhau

    // Thuế VAT
    public decimal VatRate { get; set; } = 0.08m;      // 8%

    // Cấu hình VietQR
    public string BankBin { get; set; } = "970415";    // VietinBank default
    public string BankName { get; set; } = "VietinBank";
    public string BankAccountNumber { get; set; } = "10288889999";
    public string BankAccountHolder { get; set; } = "BAN QUAN LY KTX";
}

public class PriorityDto
{
    public int Id { get; set; }
    public string Ten { get; set; } = string.Empty;
    public int DiemCong { get; set; }
}

public class ProvinceDto
{
    public int Id { get; set; }
    public string Ten { get; set; } = string.Empty;
    public int KhoangCachKm { get; set; }
    public int NhomKhoangCach { get; set; }
    public List<string> TenCus { get; set; } = new();
}

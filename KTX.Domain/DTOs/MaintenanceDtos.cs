namespace KTX.Domain.DTOs;

public class MaintenanceRequestDto
{
    public int Id { get; set; }
    public int PhongId { get; set; }
    public string MaPhong { get; set; } = string.Empty;
    public string ToaTen { get; set; } = string.Empty;
    public string MoTa { get; set; } = string.Empty;
    public string TrangThai { get; set; } = "ChuaXuLy"; // "ChuaXuLy", "DangXuLy", "HoanThanh"
    public DateTime NgayTao { get; set; }
    public DateTime? NgayHoanThanh { get; set; }
    public decimal ChiPhi { get; set; }
    public string? GhiChu { get; set; }

    public string TrangThaiText => TrangThai switch
    {
        "HoanThanh" => "Đã hoàn thành",
        "DangXuLy" => "Đang xử lý",
        _ => "Chưa xử lý"
    };
}

public class MaintenanceCreateDto
{
    public int PhongId { get; set; }
    public string MoTa { get; set; } = string.Empty;
    public string? GhiChu { get; set; }
}

public class MaintenanceStatusUpdateDto
{
    public int Id { get; set; }
    public string TrangThai { get; set; } = "HoanThanh";
    public decimal ChiPhi { get; set; }
    public string? GhiChu { get; set; }
}

public class MaintenanceStatsDto
{
    public int TotalRequests { get; set; }
    public int PendingCount { get; set; }
    public int ProcessingCount { get; set; }
    public int CompletedCount { get; set; }
    public decimal TotalCost { get; set; }
}

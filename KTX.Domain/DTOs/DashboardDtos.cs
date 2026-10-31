namespace KTX.Domain.DTOs;

public class DashboardStatsDto
{
    public int TotalRooms { get; set; }
    public int TotalBeds { get; set; }
    public int OccupiedBeds { get; set; }
    public double OccupancyRate => TotalBeds > 0 ? Math.Round((double)OccupiedBeds / TotalBeds * 100.0, 1) : 0;

    public int ActiveContracts { get; set; }
    public int ExpiringContracts30Days { get; set; }

    public decimal MonthlyRevenue { get; set; }
    public int UnpaidInvoices { get; set; }
    public int PendingMaintenance { get; set; }

    public List<BuildingOccupancyDto> Buildings { get; set; } = new();
    public List<MonthlyRevenueDto> RevenueHistory { get; set; } = new();
}

public class BuildingOccupancyDto
{
    public string BuildingName { get; set; } = string.Empty;
    public int TotalBeds { get; set; }
    public int OccupiedBeds { get; set; }
    public int AvailableBeds => Math.Max(0, TotalBeds - OccupiedBeds);
}

public class MonthlyRevenueDto
{
    public string MonthName { get; set; } = string.Empty;
    public decimal RoomFeeRevenue { get; set; }
    public decimal UtilityRevenue { get; set; }
    public decimal TotalRevenue => RoomFeeRevenue + UtilityRevenue;
}

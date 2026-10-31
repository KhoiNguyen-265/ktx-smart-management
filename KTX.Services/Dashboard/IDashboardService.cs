using KTX.Domain.DTOs;

namespace KTX.Services.Dashboard;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken ct = default);
}

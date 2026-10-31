using KTX.Domain.DTOs;

namespace KTX.Services.Facility;

public interface IMaintenanceService
{
    Task<List<MaintenanceRequestDto>> GetRequestsAsync(string? status = null, int? phongId = null, CancellationToken ct = default);
    Task<MaintenanceRequestDto> CreateRequestAsync(MaintenanceCreateDto dto, CancellationToken ct = default);
    Task<bool> UpdateRequestStatusAsync(MaintenanceStatusUpdateDto dto, CancellationToken ct = default);
    Task<MaintenanceStatsDto> GetStatsAsync(CancellationToken ct = default);
}

using KTX.Domain.DTOs;

namespace KTX.Services.Configuration;

public interface IConfigurationService
{
    Task<SystemConfigDto> GetSystemConfigAsync(CancellationToken ct = default);
    Task<bool> SaveSystemConfigAsync(SystemConfigDto dto, CancellationToken ct = default);

    Task<List<PriorityDto>> GetPrioritiesAsync(CancellationToken ct = default);
    Task<bool> SavePriorityAsync(PriorityDto dto, CancellationToken ct = default);
    Task<bool> DeletePriorityAsync(int id, CancellationToken ct = default);

    Task<List<ProvinceDto>> GetProvincesAsync(CancellationToken ct = default);
    Task<bool> UpdateProvinceDistanceAsync(int id, int km, int group, CancellationToken ct = default);
}

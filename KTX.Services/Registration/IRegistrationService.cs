using KTX.Domain.DTOs;

namespace KTX.Services.Registration;

public interface IRegistrationService
{
    Task<List<DotDangKyDto>> GetBatchesAsync(CancellationToken ct = default);
    Task<DotDangKyDto?> GetBatchByIdAsync(int batchId, CancellationToken ct = default);
    Task<int> CreateBatchAsync(string tenDot, DateTime ngayMo, DateTime ngayDong, CancellationToken ct = default);
    Task<List<DonDangKyDto>> GetApplicationsAsync(int batchId, CancellationToken ct = default);

    Task<byte[]> GenerateApplicationTemplateAsync(CancellationToken ct = default);
    Task<ExcelImportResultDto> ImportApplicationsFromExcelAsync(int batchId, Stream excelStream, CancellationToken ct = default);
}

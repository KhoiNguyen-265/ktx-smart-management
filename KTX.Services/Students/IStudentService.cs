using KTX.Domain.DTOs;

namespace KTX.Services.Students;

public interface IStudentService
{
    Task<PagedResult<StudentDto>> GetStudentsPagedAsync(
        string? search = null,
        int? provinceId = null,
        int? priorityId = null,
        int pageIndex = 1,
        int pageSize = 20,
        CancellationToken ct = default);

    Task<StudentDto?> GetStudentByMssvAsync(string mssv, CancellationToken ct = default);
    Task<bool> CreateStudentAsync(StudentCreateUpdateDto dto, CancellationToken ct = default);
    Task<bool> UpdateStudentAsync(StudentCreateUpdateDto dto, CancellationToken ct = default);
    Task<bool> DeleteStudentAsync(string mssv, CancellationToken ct = default);

    Task<byte[]> ExportStudentsToExcelAsync(CancellationToken ct = default);
    Task<byte[]> GenerateStudentTemplateAsync(CancellationToken ct = default);
    Task<ExcelImportResultDto> ImportStudentsFromExcelAsync(Stream fileStream, CancellationToken ct = default);
}

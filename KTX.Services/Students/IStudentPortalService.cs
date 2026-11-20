using KTX.Domain.DTOs;

namespace KTX.Services.Students;

public interface IStudentPortalService
{
    Task<StudentPortalOverviewDto?> GetOverviewAsync(string mssv, CancellationToken ct = default);
    Task<List<InvoiceDto>> GetRoomUtilityInvoicesAsync(string mssv, CancellationToken ct = default);
    Task<List<RoomFeeDto>> GetPersonalRoomFeesAsync(string mssv, CancellationToken ct = default);
    Task<List<MaintenanceRequestDto>> GetRoomMaintenanceRequestsAsync(string mssv, CancellationToken ct = default);
    Task<MaintenanceRequestDto> ReportRoomIssueAsync(string mssv, string moTa, string? ghiChu = null, CancellationToken ct = default);
}

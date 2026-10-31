namespace KTX.Services.Reporting;

public interface IReportingService
{
    Task<byte[]> ExportOccupancyReportExcelAsync(CancellationToken ct = default);
    Task<byte[]> ExportFinancialReportPdfAsync(int thang, int nam, CancellationToken ct = default);
}

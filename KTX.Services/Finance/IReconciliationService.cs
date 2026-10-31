using KTX.Domain.DTOs;
using KTX.Domain.Enums;

namespace KTX.Services.Finance;

public interface IReconciliationService
{
    Task<ReconcileSummaryDto> ImportAndReconcileExcelAsync(Stream fileStream, string fileName, CancellationToken ct = default);
    Task<List<BankTransactionDto>> GetTransactionsAsync(TrangThaiGiaoDich? filterStatus = null, CancellationToken ct = default);
    Task<byte[]> GenerateBankStatementSampleExcelAsync(CancellationToken ct = default);
}

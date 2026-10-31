using KTX.Domain.DTOs;

namespace KTX.Services.Allocation;

public interface IAllocationEngine
{
    Task<AllocationResultDto> ExecuteAllocationAsync(
        int batchId,
        SystemConfigDto config,
        bool enableLocalSearch = true,
        int maxLocalSearchSeconds = 3,
        IProgress<AllocationProgressDto>? progress = null,
        CancellationToken ct = default);

    List<ManualAdjustmentOptionDto> EvaluateManualMoves(
        int donId,
        AllocationResultDto currentResult,
        SystemConfigDto config);

    bool ApplyManualMove(
        int donId,
        int targetRoomId,
        AllocationResultDto currentResult,
        SystemConfigDto config);

    Task<bool> CommitAllocationToContractsAsync(
        int phienId,
        AllocationResultDto result,
        DateTime contractStart,
        DateTime contractEnd,
        CancellationToken ct = default);
}

using KTX.Domain.DTOs;
using KTX.Domain.Enums;

namespace KTX.Services.Contracts;

public interface IContractService
{
    Task<PagedResult<ContractDto>> GetContractsPagedAsync(
        string? search = null,
        TrangThaiHopDong? status = null,
        bool? expiringSoon = null,
        int pageIndex = 1,
        int pageSize = 20,
        CancellationToken ct = default);

    Task<ContractDto?> GetContractByIdAsync(int id, CancellationToken ct = default);
    Task<(bool Success, string Message)> CreateContractAsync(ContractCreateDto dto, CancellationToken ct = default);
    Task<(bool Success, string Message)> TransferRoomAsync(RoomTransferRequestDto dto, CancellationToken ct = default);
    Task<bool> ExtendContractAsync(int contractId, DateTime newEndDate, CancellationToken ct = default);
    Task<ContractCheckOutResultDto> CheckOutContractAsync(ContractCheckOutDto dto, CancellationToken ct = default);
    Task<byte[]> ExportContractPdfAsync(int contractId, CancellationToken ct = default);
}

using KTX.Domain.DTOs;
using KTX.Domain.Enums;

namespace KTX.Services.Finance;

public interface ITieredBillingService
{
    Task<List<UtilityReadingDto>> GetReadingsAsync(int thang, int nam, int? toaId = null, CancellationToken ct = default);
    Task<bool> SaveReadingAsync(UtilityReadingSaveDto dto, CancellationToken ct = default);
    Task<TieredCalculationResultDto> CalculateBillPreviewAsync(decimal sanLuongDien, decimal sanLuongNuoc, int soNguoiDinhMuc, CancellationToken ct = default);

    Task<List<InvoiceDto>> GenerateMonthlyInvoicesAsync(int thang, int nam, CancellationToken ct = default);
    Task<List<InvoiceDto>> GetInvoicesAsync(int thang, int nam, TrangThaiHoaDon? status = null, string? search = null, CancellationToken ct = default);

    Task<List<RoomFeeDto>> GenerateRoomFeesAsync(DateTime tuNgay, DateTime denNgay, CancellationToken ct = default);
    Task<List<RoomFeeDto>> GetRoomFeesAsync(TrangThaiHoaDon? status = null, string? search = null, CancellationToken ct = default);

    Task<bool> RecordCashPaymentAsync(int? invoiceId, int? roomFeeId, decimal amount, CancellationToken ct = default);
}

using KTX.Data;
using KTX.Domain.DTOs;
using KTX.Domain.Entities;
using KTX.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace KTX.Services.Contracts;

public class ContractService : IContractService
{
    private readonly IDbContextFactory<KtxDbContext> _contextFactory;

    public ContractService(IDbContextFactory<KtxDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
        QuestPDF.Settings.License = LicenseType.Community;
        QuestPDF.Settings.UseSystemFonts = true;
        QuestPDF.Settings.ThrowOnMissingFontFamilies = false;
    }

    public async Task<PagedResult<ContractDto>> GetContractsPagedAsync(
        string? search = null,
        TrangThaiHopDong? status = null,
        bool? expiringSoon = null,
        int pageIndex = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var query = db.HopDongs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(h => h.MSSV.Contains(s) ||
                                     h.SinhVien.HoTen.Contains(s) ||
                                     h.Phong.MaPhong.Contains(s) ||
                                     h.SinhVien.CCCD.Contains(s));
        }

        if (status.HasValue)
        {
            query = query.Where(h => h.TrangThai == status.Value);
        }

        if (expiringSoon == true)
        {
            var now = DateTime.Now.Date;
            var threshold = now.AddDays(30);
            query = query.Where(h => h.TrangThai == TrangThaiHopDong.HieuLuc &&
                                     h.NgayKetThuc.Date >= now &&
                                     h.NgayKetThuc.Date <= threshold);
        }

        int totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(h => h.Id)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(h => new ContractDto
            {
                Id = h.Id,
                MSSV = h.MSSV,
                HoTen = h.SinhVien.HoTen,
                GioiTinh = h.SinhVien.GioiTinh,
                CCCD = h.SinhVien.CCCD,
                SoDienThoai = h.SinhVien.SoDienThoai,
                Khoa = h.SinhVien.Khoa,
                KhoaHoc = h.SinhVien.KhoaHoc,
                PhongId = h.PhongId,
                MaPhong = h.Phong.MaPhong,
                ToaTen = h.Phong.ToaNha.Ten,
                Tang = h.Phong.Tang,
                SucChua = h.Phong.LoaiPhong.SucChua,
                NgayBatDau = h.NgayBatDau,
                NgayKetThuc = h.NgayKetThuc,
                GiaThang = h.GiaThang,
                MienGiamPhanTram = h.MienGiamPhanTram,
                TienCoc = h.TienCoc,
                TrangThai = h.TrangThai
            })
            .ToListAsync(ct);

        return new PagedResult<ContractDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    public async Task<ContractDto?> GetContractByIdAsync(int id, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        return await db.HopDongs.AsNoTracking()
            .Where(h => h.Id == id)
            .Select(h => new ContractDto
            {
                Id = h.Id,
                MSSV = h.MSSV,
                HoTen = h.SinhVien.HoTen,
                GioiTinh = h.SinhVien.GioiTinh,
                CCCD = h.SinhVien.CCCD,
                SoDienThoai = h.SinhVien.SoDienThoai,
                Khoa = h.SinhVien.Khoa,
                KhoaHoc = h.SinhVien.KhoaHoc,
                PhongId = h.PhongId,
                MaPhong = h.Phong.MaPhong,
                ToaTen = h.Phong.ToaNha.Ten,
                Tang = h.Phong.Tang,
                SucChua = h.Phong.LoaiPhong.SucChua,
                NgayBatDau = h.NgayBatDau,
                NgayKetThuc = h.NgayKetThuc,
                GiaThang = h.GiaThang,
                MienGiamPhanTram = h.MienGiamPhanTram,
                TienCoc = h.TienCoc,
                TrangThai = h.TrangThai
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<(bool Success, string Message)> CreateContractAsync(ContractCreateDto dto, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        // 1. Kiểm tra sinh viên
        var sv = await db.SinhViens.Include(s => s.HopDongs).FirstOrDefaultAsync(s => s.MSSV == dto.MSSV, ct);
        if (sv == null)
        {
            return (false, $"Sinh viên '{dto.MSSV}' không tồn tại trong hệ thống.");
        }

        if (sv.HopDongs.Any(h => h.TrangThai == TrangThaiHopDong.HieuLuc))
        {
            return (false, $"Sinh viên '{sv.HoTen}' hiện đang có hợp đồng hiệu lực.");
        }

        // 2. Kiểm tra phòng
        var phong = await db.Phongs
            .Include(p => p.ToaNha)
            .Include(p => p.LoaiPhong)
            .Include(p => p.HopDongs)
            .FirstOrDefaultAsync(p => p.Id == dto.PhongId, ct);

        if (phong == null)
        {
            return (false, "Phòng không tồn tại.");
        }

        if (phong.TrangThai == TrangThaiPhong.BaoTri)
        {
            return (false, $"Phòng {phong.MaPhong} hiện đang trong trạng thái bảo trì.");
        }

        // Kiểm tra giới tính
        if (phong.ToaNha.GioiTinh != sv.GioiTinh)
        {
            return (false, $"Giới tính của sinh viên ({sv.GioiTinh}) không phù hợp với tòa ({phong.ToaNha.GioiTinh}).");
        }

        // Kiểm tra số chỗ trống
        int currentOccupants = phong.HopDongs.Count(h => h.TrangThai == TrangThaiHopDong.HieuLuc);
        if (currentOccupants >= phong.LoaiPhong.SucChua)
        {
            return (false, $"Phòng {phong.MaPhong} đã đủ {phong.LoaiPhong.SucChua} người, không còn chỗ trống.");
        }

        var contract = new HopDong
        {
            MSSV = dto.MSSV,
            PhongId = dto.PhongId,
            NgayBatDau = dto.NgayBatDau,
            NgayKetThuc = dto.NgayKetThuc,
            GiaThang = dto.GiaThang > 0 ? dto.GiaThang : phong.LoaiPhong.GiaThang,
            MienGiamPhanTram = dto.MienGiamPhanTram,
            TienCoc = dto.TienCoc > 0 ? dto.TienCoc : phong.LoaiPhong.GiaThang,
            TrangThai = TrangThaiHopDong.HieuLuc
        };

        db.HopDongs.Add(contract);
        await db.SaveChangesAsync(ct);

        return (true, $"Tạo hợp đồng thành công cho sinh viên {sv.HoTen} tại phòng {phong.MaPhong}.");
    }

    public async Task<(bool Success, string Message)> TransferRoomAsync(RoomTransferRequestDto dto, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var contract = await db.HopDongs
            .Include(h => h.SinhVien)
            .Include(h => h.Phong).ThenInclude(p => p.ToaNha)
            .FirstOrDefaultAsync(h => h.Id == dto.HopDongId, ct);

        if (contract == null || contract.TrangThai != TrangThaiHopDong.HieuLuc)
        {
            return (false, "Hợp đồng không tồn tại hoặc không còn hiệu lực.");
        }

        if (contract.PhongId == dto.DenPhongId)
        {
            return (false, "Phòng đích trùng với phòng hiện tại của sinh viên.");
        }

        var targetRoom = await db.Phongs
            .Include(p => p.ToaNha)
            .Include(p => p.LoaiPhong)
            .Include(p => p.HopDongs)
            .FirstOrDefaultAsync(p => p.Id == dto.DenPhongId, ct);

        if (targetRoom == null)
        {
            return (false, "Phòng chuyển đến không tồn tại.");
        }

        if (targetRoom.TrangThai == TrangThaiPhong.BaoTri)
        {
            return (false, $"Phòng {targetRoom.MaPhong} đang bảo trì.");
        }

        if (targetRoom.ToaNha.GioiTinh != contract.SinhVien.GioiTinh)
        {
            return (false, $"Tòa nhà {targetRoom.ToaNha.Ten} dành cho {targetRoom.ToaNha.GioiTinh}, không phù hợp với sinh viên.");
        }

        int activeInTarget = targetRoom.HopDongs.Count(h => h.TrangThai == TrangThaiHopDong.HieuLuc);
        if (activeInTarget >= targetRoom.LoaiPhong.SucChua)
        {
            return (false, $"Phòng {targetRoom.MaPhong} đã đầy chỗ ({activeInTarget}/{targetRoom.LoaiPhong.SucChua}).");
        }

        // Ghi lại lịch sử chuyển phòng
        var history = new LichSuChuyenPhong
        {
            HopDongId = contract.Id,
            TuPhongId = contract.PhongId,
            DenPhongId = targetRoom.Id,
            NgayChuyen = DateTime.Now,
            LyDo = dto.LyDo.Trim()
        };
        db.LichSuChuyenPhongs.Add(history);

        // Cập nhật hợp đồng
        int oldRoomId = contract.PhongId;
        string oldRoomCode = contract.Phong.MaPhong;
        contract.PhongId = targetRoom.Id;
        contract.GiaThang = targetRoom.LoaiPhong.GiaThang;

        await db.SaveChangesAsync(ct);

        return (true, $"Đã chuyển phòng cho sinh viên {contract.SinhVien.HoTen} từ {oldRoomCode} sang {targetRoom.MaPhong}.");
    }

    public async Task<bool> ExtendContractAsync(int contractId, DateTime newEndDate, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var contract = await db.HopDongs.FindAsync(new object[] { contractId }, ct);
        if (contract == null || contract.TrangThai != TrangThaiHopDong.HieuLuc)
        {
            return false;
        }

        if (newEndDate <= contract.NgayKetThuc)
        {
            return false;
        }

        contract.NgayKetThuc = newEndDate;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<ContractCheckOutResultDto> CheckOutContractAsync(ContractCheckOutDto dto, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var contract = await db.HopDongs
            .Include(h => h.SinhVien)
            .Include(h => h.Phong)
            .FirstOrDefaultAsync(h => h.Id == dto.HopDongId, ct);

        if (contract == null || contract.TrangThai != TrangThaiHopDong.HieuLuc)
        {
            return new ContractCheckOutResultDto
            {
                Success = false,
                Message = "Hợp đồng không tồn tại hoặc đã được thanh lý trước đó."
            };
        }

        decimal initialDeposit = contract.TienCoc;
        decimal totalDeductions = dto.TienHongHocTaiSan + dto.TienNoDienNuocKhauTru;
        decimal refundAmount = Math.Max(0m, initialDeposit - totalDeductions);

        contract.TrangThai = TrangThaiHopDong.DaTra;
        await db.SaveChangesAsync(ct);

        return new ContractCheckOutResultDto
        {
            Success = true,
            TienCocBanDau = initialDeposit,
            TongKhauTru = totalDeductions,
            TienHoanTra = refundAmount,
            Message = $"Hoàn tất thủ tục trả phòng cho sinh viên {contract.SinhVien.HoTen}. Hoàn tiền cọc: {refundAmount:N0} VNĐ."
        };
    }

    public async Task<byte[]> ExportContractPdfAsync(int contractId, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var contract = await db.HopDongs.AsNoTracking()
            .Include(h => h.SinhVien).ThenInclude(s => s.TinhThanh)
            .Include(h => h.Phong).ThenInclude(p => p.ToaNha)
            .Include(h => h.Phong).ThenInclude(p => p.LoaiPhong)
            .FirstOrDefaultAsync(h => h.Id == contractId, ct);

        if (contract == null)
        {
            throw new InvalidOperationException($"Không tìm thấy hợp đồng số #{contractId}");
        }

        var sv = contract.SinhVien;
        var phong = contract.Phong;
        var giaThucTe = Math.Round(contract.GiaThang * (1 - contract.MienGiamPhanTram / 100m), 0);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(x => x.FontFamily("Lato").FontSize(11));

                // Header
                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text("CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM").Bold().FontSize(12);
                    col.Item().AlignCenter().Text("Độc lập – Tự do – Hạnh phúc").FontSize(11);
                    col.Item().AlignCenter().Text("---------------------------------").FontSize(9);
                    col.Item().Height(12);

                    col.Item().AlignCenter().Text("HỢP ĐỒNG LƯU TRÚ KÝ TÚC XÁ").Bold().FontSize(16).FontColor(Colors.Blue.Darken2);
                    col.Item().AlignCenter().Text($"Số: HD-KTX/{contract.Id:D5}/{contract.NgayBatDau:yyyy}").Italic().FontSize(10);
                    col.Item().Height(16);
                });

                // Content
                page.Content().Column(col =>
                {
                    col.Item().Text("Hôm nay, ngày " + DateTime.Now.ToString("dd/MM/yyyy") + ", tại Văn phòng Ban Quản lý Ký túc xá, hai bên chúng tôi gồm có:");
                    col.Item().Height(8);

                    // Bên A
                    col.Item().Text("BÊN CHO THUÊ (BÊN A):").Bold();
                    col.Item().PaddingLeft(14).Text("• Đơn vị: BAN QUẢN LÝ KÝ TÚC XÁ ĐẠI HỌC");
                    col.Item().PaddingLeft(14).Text("• Địa chỉ: Khuôn viên Ký túc xá Trung tâm");
                    col.Item().PaddingLeft(14).Text("• Đại diện: Ông/Bà Giám đốc Ký túc xá");
                    col.Item().Height(8);

                    // Bên B
                    col.Item().Text("BÊN LƯU TRÚ (BÊN B):").Bold();
                    col.Item().PaddingLeft(14).Text($"• Họ và tên: {sv.HoTen} (Giới tính: {(sv.GioiTinh == GioiTinh.Nam ? "Nam" : "Nữ")})");
                    col.Item().PaddingLeft(14).Text($"• Mã số sinh viên: {sv.MSSV} | Khóa: {sv.KhoaHoc} | Khoa: {sv.Khoa}");
                    col.Item().PaddingLeft(14).Text($"• Số CCCD: {sv.CCCD} | Quê quán: {sv.TinhThanh?.Ten}");
                    col.Item().PaddingLeft(14).Text($"• Số điện thoại: {sv.SoDienThoai} | Email: {sv.Email}");
                    col.Item().Height(12);

                    col.Item().Text("Hai bên thống nhất ký kết Hợp đồng lưu trú với các điều khoản cụ thể sau đây:");
                    col.Item().Height(6);

                    // Điều 1
                    col.Item().Text("ĐIỀU 1: ĐỐI TƯỢNG VÀ THỜI HẠN LƯU TRÚ").Bold();
                    col.Item().PaddingLeft(14).Text($"1.1 Bên A bố trí cho Bên B một chỗ ở tại Phòng {phong.MaPhong}, Tầng {phong.Tang}, {phong.ToaNha.Ten}.");
                    col.Item().PaddingLeft(14).Text($"1.2 Loại phòng: {phong.LoaiPhong.Ten} ({phong.LoaiPhong.SucChua} người/phòng, {(phong.LoaiPhong.CoDieuHoa ? "Có điều hòa" : "Không điều hòa")}).");
                    col.Item().PaddingLeft(14).Text($"1.3 Thời hạn lưu trú: Từ ngày {contract.NgayBatDau:dd/MM/yyyy} đến ngày {contract.NgayKetThuc:dd/MM/yyyy}.");
                    col.Item().Height(8);

                    // Điều 2
                    col.Item().Text("ĐIỀU 2: PHÍ LƯU TRÚ VÀ TIỀN ĐẶT CỌC").Bold();
                    col.Item().PaddingLeft(14).Text($"2.1 Đơn giá thuê phòng: {contract.GiaThang:N0} VNĐ/tháng.");
                    col.Item().PaddingLeft(14).Text($"2.2 Tỉ lệ miễn giảm theo diện chính sách: {contract.MienGiamPhanTram:F0}%.");
                    col.Item().PaddingLeft(14).Text($"2.3 Số tiền phải thanh toán thực tế: {giaThucTe:N0} VNĐ/tháng.");
                    col.Item().PaddingLeft(14).Text($"2.4 Tiền đặt cọc tài sản: {contract.TienCoc:N0} VNĐ (Hoàn trả khi thanh lý hợp đồng sau khi trừ các khoản công nợ hỏng hóc nếu có).");
                    col.Item().PaddingLeft(14).Text("2.5 Tiền điện, nước và dịch vụ sinh hoạt chung thanh toán hàng tháng theo chỉ số đồng hồ thực tế của phòng.");
                    col.Item().Height(8);

                    // Điều 3
                    col.Item().Text("ĐIỀU 3: NGHĨA VỤ VÀ TRÁCH NHIỆM").Bold();
                    col.Item().PaddingLeft(14).Text("3.1 Bên B cam kết chấp hành nghiêm túc Nội quy Ký túc xá, giữ gìn vệ sinh chung, bảo quản tài sản công.");
                    col.Item().PaddingLeft(14).Text("3.2 Bên A có trách nhiệm đảm bảo an ninh trật tự, cấp điện nước đầy đủ và bảo trì trang thiết bị chung.");
                    col.Item().Height(24);

                    // Chữ ký
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Column(c =>
                        {
                            c.Item().AlignCenter().Text("ĐẠI DIỆN BÊN A").Bold();
                            c.Item().AlignCenter().Text("(Ký, đóng dấu và ghi rõ họ tên)").Italic().FontSize(9);
                            c.Item().Height(60);
                            c.Item().AlignCenter().Text("BAN QUẢN LÝ KÝ TÚC XÁ").Bold();
                        });

                        r.RelativeItem().Column(c =>
                        {
                            c.Item().AlignCenter().Text("NGƯỜI LƯU TRÚ (BÊN B)").Bold();
                            c.Item().AlignCenter().Text("(Ký và ghi rõ họ tên)").Italic().FontSize(9);
                            c.Item().Height(60);
                            c.Item().AlignCenter().Text(sv.HoTen).Bold();
                        });
                    });
                });

                // Footer
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Trang ");
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        });

        using var ms = new MemoryStream();
        document.GeneratePdf(ms);
        return ms.ToArray();
    }
}

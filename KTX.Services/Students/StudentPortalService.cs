using KTX.Data;
using KTX.Domain.DTOs;
using KTX.Domain.Entities;
using KTX.Domain.Enums;
using KTX.Infrastructure.VietQR;
using KTX.Services.Configuration;
using Microsoft.EntityFrameworkCore;

namespace KTX.Services.Students;

public class StudentPortalService : IStudentPortalService
{
    private readonly IDbContextFactory<KtxDbContext> _contextFactory;
    private readonly IConfigurationService _configService;

    public StudentPortalService(
        IDbContextFactory<KtxDbContext> contextFactory,
        IConfigurationService configService)
    {
        _contextFactory = contextFactory;
        _configService = configService;
    }

    public async Task<StudentPortalOverviewDto?> GetOverviewAsync(string mssv, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var sv = await db.SinhViens.AsNoTracking()
            .Include(s => s.TinhThanh)
            .Include(s => s.DienUuTien)
            .FirstOrDefaultAsync(s => s.MSSV == mssv, ct);

        if (sv == null) return null;

        var activeContract = await db.HopDongs.AsNoTracking()
            .Include(h => h.Phong).ThenInclude(p => p.ToaNha)
            .Include(h => h.Phong).ThenInclude(p => p.LoaiPhong)
            .Include(h => h.Phong).ThenInclude(p => p.TaiSanPhongs)
            .Where(h => h.MSSV == mssv && h.TrangThai == TrangThaiHopDong.HieuLuc)
            .OrderByDescending(h => h.Id)
            .FirstOrDefaultAsync(ct);

        var dto = new StudentPortalOverviewDto
        {
            MSSV = sv.MSSV,
            HoTen = sv.HoTen,
            GioiTinh = sv.GioiTinh,
            NgaySinh = sv.NgaySinh,
            CCCD = sv.CCCD,
            Khoa = sv.Khoa,
            KhoaHoc = sv.KhoaHoc,
            SoDienThoai = sv.SoDienThoai,
            Email = sv.Email,
            QueQuan = sv.TinhThanh?.Ten ?? "---",
            DienUuTien = sv.DienUuTien?.Ten ?? "Không thuộc diện ưu tiên"
        };

        if (activeContract != null)
        {
            dto.ActiveContractId = activeContract.Id;
            dto.PhongId = activeContract.PhongId;
            dto.MaPhong = activeContract.Phong.MaPhong;
            dto.TenToa = activeContract.Phong.ToaNha.Ten;
            dto.Tang = activeContract.Phong.Tang;
            dto.TenLoaiPhong = activeContract.Phong.LoaiPhong.Ten;
            dto.SucChua = activeContract.Phong.LoaiPhong.SucChua;
            dto.GiaThang = activeContract.GiaThang;
            dto.MienGiamPhanTram = activeContract.MienGiamPhanTram;
            dto.TienCoc = activeContract.TienCoc;
            dto.NgayBatDau = activeContract.NgayBatDau;
            dto.NgayKetThuc = activeContract.NgayKetThuc;

            dto.RoomAssets = activeContract.Phong.TaiSanPhongs
                .Select(a => new RoomAssetDto
                {
                    Id = a.Id,
                    TenTaiSan = a.TenTaiSan,
                    SoLuong = a.SoLuong,
                    TinhTrang = a.TinhTrang
                }).ToList();

            var roommates = await db.HopDongs.AsNoTracking()
                .Include(h => h.SinhVien)
                .Where(h => h.PhongId == activeContract.PhongId && h.TrangThai == TrangThaiHopDong.HieuLuc)
                .OrderBy(h => h.SinhVien.HoTen)
                .ToListAsync(ct);

            dto.Roommates = roommates.Select(h => new RoomResidentDto
            {
                MSSV = h.MSSV,
                HoTen = h.SinhVien.HoTen + (h.MSSV == mssv ? " (Bạn)" : ""),
                Khoa = h.SinhVien.Khoa,
                KhoaHoc = h.SinhVien.KhoaHoc,
                SoDienThoai = h.SinhVien.SoDienThoai,
                NgayBatDau = h.NgayBatDau,
                NgayKetThuc = h.NgayKetThuc
            }).ToList();

            var unpaidRoomFees = await db.KhoanThuTienPhongs.AsNoTracking()
                .Where(k => k.HopDongId == activeContract.Id && k.TrangThai != TrangThaiHoaDon.DaThanhToan)
                .ToListAsync(ct);
            dto.TongNoTienPhong = unpaidRoomFees.Sum(k => Math.Max(0, k.PhaiThu - k.DaThu));

            var unpaidUtil = await db.HoaDonDienNuocs.AsNoTracking()
                .Where(h => h.PhongId == activeContract.PhongId && h.TrangThai != TrangThaiHoaDon.DaThanhToan)
                .ToListAsync(ct);
            dto.TongNoDienNuocPhong = unpaidUtil.Sum(h => Math.Max(0, h.TongTien - h.DaThu));

            dto.SoSuCoDangXuLy = await db.YeuCauSuaChuas.AsNoTracking()
                .CountAsync(y => y.PhongId == activeContract.PhongId && y.TrangThai != "HoanThanh", ct);
        }

        return dto;
    }

    public async Task<List<InvoiceDto>> GetRoomUtilityInvoicesAsync(string mssv, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        var config = await _configService.GetSystemConfigAsync(ct);

        var activeContract = await db.HopDongs.AsNoTracking()
            .Where(h => h.MSSV == mssv && h.TrangThai == TrangThaiHopDong.HieuLuc)
            .OrderByDescending(h => h.Id)
            .FirstOrDefaultAsync(ct);

        if (activeContract == null) return new List<InvoiceDto>();

        var invoices = await db.HoaDonDienNuocs.AsNoTracking()
            .Include(h => h.Phong).ThenInclude(p => p.ToaNha)
            .Include(h => h.ChiTietHoaDons)
            .Where(h => h.PhongId == activeContract.PhongId)
            .OrderByDescending(h => h.Nam).ThenByDescending(h => h.Thang)
            .ToListAsync(ct);

        return invoices.Select(inv =>
        {
            decimal conNo = Math.Max(0, inv.TongTien - inv.DaThu);
            return new InvoiceDto
            {
                Id = inv.Id,
                MaThanhToan = inv.MaThanhToan,
                PhongId = inv.PhongId,
                MaPhong = inv.Phong.MaPhong,
                ToaTen = inv.Phong.ToaNha.Ten,
                Thang = inv.Thang,
                Nam = inv.Nam,
                SoNguoiTinhDinhMuc = inv.SoNguoiTinhDinhMuc,
                TongTien = inv.TongTien,
                DaThu = inv.DaThu,
                TrangThai = inv.TrangThai,
                VietQrPayload = conNo > 0
                    ? VietQRHelper.BuildEmvCoString(config.BankBin, config.BankAccountNumber, conNo, inv.MaThanhToan)
                    : string.Empty
            };
        }).ToList();
    }

    public async Task<List<RoomFeeDto>> GetPersonalRoomFeesAsync(string mssv, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        var config = await _configService.GetSystemConfigAsync(ct);

        var fees = await db.KhoanThuTienPhongs.AsNoTracking()
            .Include(k => k.HopDong).ThenInclude(h => h.SinhVien)
            .Include(k => k.HopDong).ThenInclude(h => h.Phong)
            .Where(k => k.HopDong.MSSV == mssv)
            .OrderByDescending(k => k.TuNgay)
            .ToListAsync(ct);

        return fees.Select(f =>
        {
            decimal conNo = Math.Max(0, f.PhaiThu - f.DaThu);
            return new RoomFeeDto
            {
                Id = f.Id,
                HopDongId = f.HopDongId,
                MaThanhToan = f.MaThanhToan,
                MSSV = f.HopDong.MSSV,
                HoTen = f.HopDong.SinhVien.HoTen,
                MaPhong = f.HopDong.Phong.MaPhong,
                TuNgay = f.TuNgay,
                DenNgay = f.DenNgay,
                SoTienGoc = f.SoTienGoc,
                MienGiam = f.MienGiam,
                PhaiThu = f.PhaiThu,
                DaThu = f.DaThu,
                TrangThai = f.TrangThai,
                VietQrPayload = conNo > 0
                    ? VietQRHelper.BuildEmvCoString(config.BankBin, config.BankAccountNumber, conNo, f.MaThanhToan)
                    : string.Empty
            };
        }).ToList();
    }

    public async Task<List<MaintenanceRequestDto>> GetRoomMaintenanceRequestsAsync(string mssv, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var activeContract = await db.HopDongs.AsNoTracking()
            .Where(h => h.MSSV == mssv && h.TrangThai == TrangThaiHopDong.HieuLuc)
            .OrderByDescending(h => h.Id)
            .FirstOrDefaultAsync(ct);

        if (activeContract == null) return new List<MaintenanceRequestDto>();

        return await db.YeuCauSuaChuas.AsNoTracking()
            .Include(y => y.Phong).ThenInclude(p => p.ToaNha)
            .Where(y => y.PhongId == activeContract.PhongId)
            .OrderByDescending(y => y.NgayTao)
            .Select(y => new MaintenanceRequestDto
            {
                Id = y.Id,
                PhongId = y.PhongId,
                MaPhong = y.Phong.MaPhong,
                ToaTen = y.Phong.ToaNha.Ten,
                MoTa = y.MoTa,
                TrangThai = y.TrangThai,
                NgayTao = y.NgayTao,
                NgayHoanThanh = y.NgayHoanThanh,
                ChiPhi = y.ChiPhi,
                GhiChu = y.GhiChu
            })
            .ToListAsync(ct);
    }

    public async Task<MaintenanceRequestDto> ReportRoomIssueAsync(string mssv, string moTa, string? ghiChu = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(moTa))
            throw new ArgumentException("Vui lòng nhập mô tả sự cố.");

        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var activeContract = await db.HopDongs.AsNoTracking()
            .Include(h => h.SinhVien)
            .Include(h => h.Phong).ThenInclude(p => p.ToaNha)
            .Where(h => h.MSSV == mssv && h.TrangThai == TrangThaiHopDong.HieuLuc)
            .OrderByDescending(h => h.Id)
            .FirstOrDefaultAsync(ct);

        if (activeContract == null)
            throw new InvalidOperationException("Bạn hiện chưa có hợp đồng lưu trú KTX đang hiệu lực.");

        string note = string.IsNullOrWhiteSpace(ghiChu)
            ? $"SV {activeContract.SinhVien.HoTen} ({mssv}) báo qua Cổng Sinh viên"
            : $"SV {activeContract.SinhVien.HoTen} ({mssv}): {ghiChu.Trim()}";

        var entity = new YeuCauSuaChua
        {
            PhongId = activeContract.PhongId,
            MoTa = moTa.Trim(),
            TrangThai = "ChuaXuLy",
            NgayTao = DateTime.Now,
            ChiPhi = 0,
            GhiChu = note
        };

        db.YeuCauSuaChuas.Add(entity);
        await db.SaveChangesAsync(ct);

        return new MaintenanceRequestDto
        {
            Id = entity.Id,
            PhongId = entity.PhongId,
            MaPhong = activeContract.Phong.MaPhong,
            ToaTen = activeContract.Phong.ToaNha.Ten,
            MoTa = entity.MoTa,
            TrangThai = entity.TrangThai,
            NgayTao = entity.NgayTao,
            ChiPhi = entity.ChiPhi,
            GhiChu = entity.GhiChu
        };
    }
}

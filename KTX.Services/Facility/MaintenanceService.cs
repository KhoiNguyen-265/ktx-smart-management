using KTX.Data;
using KTX.Domain.DTOs;
using KTX.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KTX.Services.Facility;

public class MaintenanceService : IMaintenanceService
{
    private readonly IDbContextFactory<KtxDbContext> _contextFactory;

    public MaintenanceService(IDbContextFactory<KtxDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<MaintenanceRequestDto>> GetRequestsAsync(string? status = null, int? phongId = null, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var query = db.YeuCauSuaChuas
            .Include(y => y.Phong).ThenInclude(p => p.ToaNha)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(y => y.TrangThai == status);
        }

        if (phongId.HasValue && phongId.Value > 0)
        {
            query = query.Where(y => y.PhongId == phongId.Value);
        }

        return await query
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

    public async Task<MaintenanceRequestDto> CreateRequestAsync(MaintenanceCreateDto dto, CancellationToken ct = default)
    {
        if (dto.PhongId <= 0) throw new ArgumentException("Phòng không hợp lệ.", nameof(dto.PhongId));
        if (string.IsNullOrWhiteSpace(dto.MoTa)) throw new ArgumentException("Mô tả sự cố không được để trống.", nameof(dto.MoTa));

        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var entity = new YeuCauSuaChua
        {
            PhongId = dto.PhongId,
            MoTa = dto.MoTa.Trim(),
            TrangThai = "ChuaXuLy",
            NgayTao = DateTime.Now,
            ChiPhi = 0m,
            GhiChu = dto.GhiChu?.Trim()
        };

        db.YeuCauSuaChuas.Add(entity);
        await db.SaveChangesAsync(ct);

        var room = await db.Phongs.Include(p => p.ToaNha).FirstOrDefaultAsync(p => p.Id == dto.PhongId, ct);

        return new MaintenanceRequestDto
        {
            Id = entity.Id,
            PhongId = entity.PhongId,
            MaPhong = room?.MaPhong ?? "",
            ToaTen = room?.ToaNha.Ten ?? "",
            MoTa = entity.MoTa,
            TrangThai = entity.TrangThai,
            NgayTao = entity.NgayTao,
            ChiPhi = entity.ChiPhi,
            GhiChu = entity.GhiChu
        };
    }

    public async Task<bool> UpdateRequestStatusAsync(MaintenanceStatusUpdateDto dto, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var entity = await db.YeuCauSuaChuas.FirstOrDefaultAsync(y => y.Id == dto.Id, ct);
        if (entity == null) return false;

        entity.TrangThai = dto.TrangThai;
        entity.ChiPhi = dto.ChiPhi;
        if (!string.IsNullOrWhiteSpace(dto.GhiChu))
        {
            entity.GhiChu = dto.GhiChu.Trim();
        }

        if (dto.TrangThai == "HoanThanh")
        {
            entity.NgayHoanThanh = DateTime.Now;
        }

        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<MaintenanceStatsDto> GetStatsAsync(CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var list = await db.YeuCauSuaChuas.AsNoTracking().ToListAsync(ct);

        return new MaintenanceStatsDto
        {
            TotalRequests = list.Count,
            PendingCount = list.Count(y => y.TrangThai == "ChuaXuLy"),
            ProcessingCount = list.Count(y => y.TrangThai == "DangXuLy"),
            CompletedCount = list.Count(y => y.TrangThai == "HoanThanh"),
            TotalCost = list.Sum(y => y.ChiPhi)
        };
    }
}

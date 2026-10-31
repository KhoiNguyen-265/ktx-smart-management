using KTX.Data;
using KTX.Domain.DTOs;
using KTX.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace KTX.Services.Facility;

public class RoomService : IRoomService
{
    private readonly IDbContextFactory<KtxDbContext> _contextFactory;

    public RoomService(IDbContextFactory<KtxDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<BuildingDto>> GetBuildingsAsync(CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        return await db.ToaNhas.AsNoTracking()
            .Select(b => new BuildingDto
            {
                Id = b.Id,
                Ten = b.Ten,
                GioiTinhText = b.GioiTinh == GioiTinh.Nam ? "Dành cho Nam" : "Dành cho Nữ",
                SoTang = b.SoTang,
                TongSoPhong = b.Phongs.Count
            })
            .ToListAsync(ct);
    }

    public async Task<List<RoomTypeDto>> GetRoomTypesAsync(CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        return await db.LoaiPhongs.AsNoTracking()
            .Select(lt => new RoomTypeDto
            {
                Id = lt.Id,
                Ten = lt.Ten,
                SucChua = lt.SucChua,
                GiaThang = lt.GiaThang,
                CoDieuHoa = lt.CoDieuHoa
            })
            .ToListAsync(ct);
    }

    public async Task<List<RoomSummaryDto>> GetRoomsByBuildingAndFloorAsync(int buildingId, int floor, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var rooms = await db.Phongs.AsNoTracking()
            .Where(p => p.ToaId == buildingId && p.Tang == floor)
            .OrderBy(p => p.MaPhong)
            .Select(p => new
            {
                p.Id,
                p.MaPhong,
                p.ToaId,
                TenToa = p.ToaNha.Ten,
                p.Tang,
                p.LoaiPhongId,
                TenLoaiPhong = p.LoaiPhong.Ten,
                p.LoaiPhong.SucChua,
                p.LoaiPhong.GiaThang,
                p.LoaiPhong.CoDieuHoa,
                p.TrangThai,
                ActiveResidents = p.HopDongs
                    .Where(h => h.TrangThai == TrangThaiHopDong.HieuLuc)
                    .Select(h => h.SinhVien.HoTen)
                    .ToList()
            })
            .ToListAsync(ct);

        return rooms.Select(r => new RoomSummaryDto
        {
            Id = r.Id,
            MaPhong = r.MaPhong,
            ToaId = r.ToaId,
            TenToa = r.TenToa,
            Tang = r.Tang,
            LoaiPhongId = r.LoaiPhongId,
            TenLoaiPhong = r.TenLoaiPhong,
            SucChua = r.SucChua,
            GiaThang = r.GiaThang,
            CoDieuHoa = r.CoDieuHoa,
            TrangThai = r.TrangThai,
            SoNguoiDangO = r.ActiveResidents.Count,
            TenSinhVienDangO = r.ActiveResidents
        }).ToList();
    }

    public async Task<RoomDetailDto?> GetRoomDetailAsync(int roomId, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var room = await db.Phongs.AsNoTracking()
            .Include(p => p.ToaNha)
            .Include(p => p.LoaiPhong)
            .Include(p => p.TaiSanPhongs)
            .FirstOrDefaultAsync(p => p.Id == roomId, ct);

        if (room == null) return null;

        var residents = await db.HopDongs.AsNoTracking()
            .Where(h => h.PhongId == roomId && h.TrangThai == TrangThaiHopDong.HieuLuc)
            .Include(h => h.SinhVien)
            .Select(h => new RoomResidentDto
            {
                MSSV = h.MSSV,
                HoTen = h.SinhVien.HoTen,
                Khoa = h.SinhVien.Khoa,
                KhoaHoc = h.SinhVien.KhoaHoc,
                SoDienThoai = h.SinhVien.SoDienThoai,
                NgayBatDau = h.NgayBatDau,
                NgayKetThuc = h.NgayKetThuc,
                GiaThang = h.GiaThang
            })
            .ToListAsync(ct);

        var latestMeter = await db.ChiSoDienNuocs.AsNoTracking()
            .Where(m => m.PhongId == roomId)
            .OrderByDescending(m => m.Nam)
            .ThenByDescending(m => m.Thang)
            .Select(m => new MeterReadingDto
            {
                Thang = m.Thang,
                Nam = m.Nam,
                DienMoi = m.DienMoi,
                SanLuongDien = m.SanLuongDien,
                NuocMoi = m.NuocMoi,
                SanLuongNuoc = m.SanLuongNuoc
            })
            .FirstOrDefaultAsync(ct);

        return new RoomDetailDto
        {
            Summary = new RoomSummaryDto
            {
                Id = room.Id,
                MaPhong = room.MaPhong,
                ToaId = room.ToaId,
                TenToa = room.ToaNha.Ten,
                Tang = room.Tang,
                LoaiPhongId = room.LoaiPhongId,
                TenLoaiPhong = room.LoaiPhong.Ten,
                SucChua = room.LoaiPhong.SucChua,
                GiaThang = room.LoaiPhong.GiaThang,
                CoDieuHoa = room.LoaiPhong.CoDieuHoa,
                TrangThai = room.TrangThai,
                SoNguoiDangO = residents.Count,
                TenSinhVienDangO = residents.Select(r => r.HoTen).ToList()
            },
            Residents = residents,
            Assets = room.TaiSanPhongs.Select(a => new RoomAssetDto
            {
                Id = a.Id,
                TenTaiSan = a.TenTaiSan,
                SoLuong = a.SoLuong,
                TinhTrang = a.TinhTrang
            }).ToList(),
            LatestMeterReading = latestMeter
        };
    }

    public async Task<bool> UpdateRoomStatusAsync(int roomId, bool isUnderMaintenance, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var room = await db.Phongs.FirstOrDefaultAsync(p => p.Id == roomId, ct);
        if (room == null) return false;

        room.TrangThai = isUnderMaintenance ? TrangThaiPhong.BaoTri : TrangThaiPhong.HoatDong;
        await db.SaveChangesAsync(ct);
        return true;
    }
}

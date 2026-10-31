using KTX.Data;
using KTX.Domain.DTOs;
using KTX.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace KTX.Services.Dashboard;

public class DashboardService : IDashboardService
{
    private readonly IDbContextFactory<KtxDbContext> _contextFactory;

    public DashboardService(IDbContextFactory<KtxDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var today = DateTime.Today;
        var in30Days = today.AddDays(30);

        // 1. Thống kê Phòng và Giường
        var rooms = await db.Phongs.AsNoTracking()
            .Include(p => p.LoaiPhong)
            .Include(p => p.ToaNha)
            .Include(p => p.HopDongs.Where(h => h.TrangThai == TrangThaiHopDong.HieuLuc))
            .ToListAsync(ct);

        int totalRooms = rooms.Count;
        int totalBeds = rooms.Sum(r => r.LoaiPhong.SucChua);
        int occupiedBeds = rooms.Sum(r => r.HopDongs.Count);

        // 2. Thống kê Hợp đồng
        var activeContracts = await db.HopDongs.AsNoTracking()
            .Where(h => h.TrangThai == TrangThaiHopDong.HieuLuc)
            .ToListAsync(ct);

        int activeContractCount = activeContracts.Count;
        int expiringContracts = activeContracts.Count(h => h.NgayKetThuc >= today && h.NgayKetThuc <= in30Days);

        // 3. Sự cố và Hóa đơn chưa đóng
        int pendingMaintenance = await db.YeuCauSuaChuas.AsNoTracking()
            .CountAsync(y => y.TrangThai != "HoanThanh", ct);

        int unpaidInvoices = await db.HoaDonDienNuocs.AsNoTracking()
            .CountAsync(h => h.TrangThai != TrangThaiHoaDon.DaThanhToan, ct);

        // 4. Doanh thu tháng hiện tại
        int currentMonth = today.Month;
        int currentYear = today.Year;

        decimal currentMonthUtilityPaid = await db.HoaDonDienNuocs.AsNoTracking()
            .Where(h => h.Thang == currentMonth && h.Nam == currentYear)
            .SumAsync(h => h.DaThu, ct);

        decimal currentMonthRoomFeePaid = await db.KhoanThuTienPhongs.AsNoTracking()
            .Where(k => k.TuNgay.Month == currentMonth && k.TuNgay.Year == currentYear)
            .SumAsync(k => k.DaThu, ct);

        decimal currentMonthRevenue = currentMonthUtilityPaid + currentMonthRoomFeePaid;

        // Nếu chưa có giao dịch thật trong tháng này, lấy tổng thanh toán thực tế đã ghi nhận
        if (currentMonthRevenue == 0)
        {
            currentMonthRevenue = await db.ThanhToans.AsNoTracking()
                .Where(t => t.ThoiGian.Month == currentMonth && t.ThoiGian.Year == currentYear)
                .SumAsync(t => t.SoTien, ct);
        }

        // 5. Thống kê theo từng Tòa
        var buildings = rooms.GroupBy(r => r.ToaNha.Ten)
            .Select(g => new BuildingOccupancyDto
            {
                BuildingName = g.Key,
                TotalBeds = g.Sum(r => r.LoaiPhong.SucChua),
                OccupiedBeds = g.Sum(r => r.HopDongs.Count)
            })
            .OrderBy(b => b.BuildingName)
            .ToList();

        // 6. Lịch sử doanh thu 6 tháng gần nhất
        var history = new List<MonthlyRevenueDto>();
        for (int i = 5; i >= 0; i--)
        {
            var targetDate = today.AddMonths(-i);
            int m = targetDate.Month;
            int y = targetDate.Year;

            decimal util = await db.HoaDonDienNuocs.AsNoTracking()
                .Where(h => h.Thang == m && h.Nam == y)
                .SumAsync(h => h.DaThu, ct);

            decimal rf = await db.KhoanThuTienPhongs.AsNoTracking()
                .Where(k => k.TuNgay.Month == m && k.TuNgay.Year == y)
                .SumAsync(k => k.DaThu, ct);

            history.Add(new MonthlyRevenueDto
            {
                MonthName = $"T{m}/{y % 100}",
                UtilityRevenue = util,
                RoomFeeRevenue = rf
            });
        }

        return new DashboardStatsDto
        {
            TotalRooms = totalRooms,
            TotalBeds = totalBeds,
            OccupiedBeds = occupiedBeds,
            ActiveContracts = activeContractCount,
            ExpiringContracts30Days = expiringContracts,
            PendingMaintenance = pendingMaintenance,
            UnpaidInvoices = unpaidInvoices,
            MonthlyRevenue = currentMonthRevenue,
            Buildings = buildings,
            RevenueHistory = history
        };
    }
}

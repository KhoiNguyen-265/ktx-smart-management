using KTX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace KTX.Data.Interceptors;

public class AuditLogInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var db = eventData.Context;
        if (db == null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        var entries = db.ChangeTracker.Entries()
            .Where(e => e.Entity is not NhatKy &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
            .ToList();

        foreach (var entry in entries)
        {
            var tableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name;
            var action = entry.State switch
            {
                EntityState.Added => "Thêm mới",
                EntityState.Modified => "Cập nhật",
                EntityState.Deleted => "Xóa",
                _ => entry.State.ToString()
            };

            var primaryKey = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString() ?? "N/A";

            // Tạo bản ghi nhật ký tự động
            db.Set<NhatKy>().Add(new NhatKy
            {
                HanhDong = action,
                ChiTiet = $"Thao tác [{action}] trên bảng [{tableName}] với khóa chính ID: {primaryKey}",
                BangLienQuan = tableName,
                KhoaChinhLienQuan = primaryKey,
                ThoiGian = DateTime.Now
            });
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}

using KTX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace KTX.Data.Interceptors;

public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is KtxDbContext context)
        {
            var entries = context.ChangeTracker.Entries()
                .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .Where(e => e.Entity is not NhatKy) // Tránh đệ quy ghi nhật ký
                .ToList();

            foreach (var entry in entries)
            {
                var action = entry.State switch
                {
                    EntityState.Added => "Thêm mới",
                    EntityState.Modified => "Cập nhật",
                    EntityState.Deleted => "Xóa",
                    _ => "Khác"
                };

                var tableName = entry.Entity.GetType().Name;
                var keyValues = string.Join(", ", entry.Properties
                    .Where(p => p.Metadata.IsPrimaryKey())
                    .Select(p => $"{p.Metadata.Name}: {p.CurrentValue}"));

                // Ghi nhận thay đổi nếu là thao tác quan trọng
                if (tableName is "HopDong" or "HoaDonDienNuoc" or "KhoanThuTienPhong" or "Phong" or "ChiSoDienNuoc")
                {
                    context.NhatKys.Add(new NhatKy
                    {
                        HanhDong = action,
                        BangLienQuan = tableName,
                        KhoaChinhLienQuan = keyValues,
                        ChiTiet = $"Thực hiện thao tác '{action}' trên bảng {tableName}.",
                        ThoiGian = DateTime.Now
                    });
                }
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}

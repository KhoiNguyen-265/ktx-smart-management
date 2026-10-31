using KTX.Domain.Entities;
using KTX.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace KTX.Data;

public class KtxDbContext : DbContext
{
    public KtxDbContext(DbContextOptions<KtxDbContext> options) : base(options)
    {
    }

    // Facility
    public DbSet<ToaNha> ToaNhas => Set<ToaNha>();
    public DbSet<LoaiPhong> LoaiPhongs => Set<LoaiPhong>();
    public DbSet<Phong> Phongs => Set<Phong>();
    public DbSet<TaiSanPhong> TaiSanPhongs => Set<TaiSanPhong>();

    // Student & Priority
    public DbSet<TinhThanh> TinhThanhs => Set<TinhThanh>();
    public DbSet<TinhThanhCu> TinhThanhCus => Set<TinhThanhCu>();
    public DbSet<DienUuTien> DienUuTiens => Set<DienUuTien>();
    public DbSet<SinhVien> SinhViens => Set<SinhVien>();

    // Registration & Allocation
    public DbSet<DotDangKy> DotDangKys => Set<DotDangKy>();
    public DbSet<NhomDangKy> NhomDangKys => Set<NhomDangKy>();
    public DbSet<DonDangKy> DonDangKys => Set<DonDangKy>();
    public DbSet<PhienXepPhong> PhienXepPhongs => Set<PhienXepPhong>();
    public DbSet<ChiTietXepPhong> ChiTietXepPhongs => Set<ChiTietXepPhong>();

    // Contract & Stay
    public DbSet<HopDong> HopDongs => Set<HopDong>();
    public DbSet<LichSuChuyenPhong> LichSuChuyenPhongs => Set<LichSuChuyenPhong>();

    // Finance & Reconcile
    public DbSet<BacGia> BacGias => Set<BacGia>();
    public DbSet<ChiSoDienNuoc> ChiSoDienNuocs => Set<ChiSoDienNuoc>();
    public DbSet<HoaDonDienNuoc> HoaDonDienNuocs => Set<HoaDonDienNuoc>();
    public DbSet<ChiTietHoaDon> ChiTietHoaDons => Set<ChiTietHoaDon>();
    public DbSet<KhoanThuTienPhong> KhoanThuTienPhongs => Set<KhoanThuTienPhong>();
    public DbSet<GiaoDichNganHang> GiaoDichNganHangs => Set<GiaoDichNganHang>();
    public DbSet<ThanhToan> ThanhToans => Set<ThanhToan>();

    // System
    public DbSet<VaiTro> VaiTros => Set<VaiTro>();
    public DbSet<NguoiDung> NguoiDungs => Set<NguoiDung>();
    public DbSet<NhatKy> NhatKys => Set<NhatKy>();
    public DbSet<CauHinh> CauHinhs => Set<CauHinh>();
    public DbSet<YeuCauSuaChua> YeuCauSuaChuas => Set<YeuCauSuaChua>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Decimal precision configuration ---
        foreach (var property in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetPrecision(18);
            property.SetScale(2);
        }

        // --- ToaNha & Phong ---
        modelBuilder.Entity<ToaNha>(entity =>
        {
            entity.Property(e => e.Ten).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<LoaiPhong>(entity =>
        {
            entity.Property(e => e.Ten).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Phong>(entity =>
        {
            entity.Property(e => e.MaPhong).HasMaxLength(50).IsRequired();
            entity.Property(e => e.RowVersion).IsRowVersion();
            entity.HasIndex(e => new { e.ToaId, e.MaPhong }).IsUnique();

            entity.HasOne(e => e.ToaNha)
                  .WithMany(t => t.Phongs)
                  .HasForeignKey(e => e.ToaId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.LoaiPhong)
                  .WithMany(l => l.Phongs)
                  .HasForeignKey(e => e.LoaiPhongId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // --- SinhVien ---
        modelBuilder.Entity<SinhVien>(entity =>
        {
            entity.HasKey(e => e.MSSV);
            entity.Property(e => e.MSSV).HasMaxLength(20);
            entity.Property(e => e.HoTen).HasMaxLength(150).IsRequired();
            entity.Property(e => e.CCCD).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Khoa).HasMaxLength(100).IsRequired();
            entity.Property(e => e.SoDienThoai).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(150);

            entity.HasIndex(e => e.CCCD).IsUnique();

            entity.HasOne(e => e.TinhThanh)
                  .WithMany(t => t.SinhViens)
                  .HasForeignKey(e => e.TinhThanhId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DienUuTien)
                  .WithMany(d => d.SinhViens)
                  .HasForeignKey(e => e.DienUuTienId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TinhThanh>(entity =>
        {
            entity.Property(e => e.Ten).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<TinhThanhCu>(entity =>
        {
            entity.Property(e => e.TenCu).HasMaxLength(100).IsRequired();
            entity.HasOne(e => e.TinhThanh)
                  .WithMany(t => t.TinhThanhCus)
                  .HasForeignKey(e => e.TinhThanhId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // --- DonDangKy & Registration ---
        modelBuilder.Entity<NhomDangKy>(entity =>
        {
            entity.Property(e => e.MaNhom).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.MaNhom).IsUnique();
        });

        modelBuilder.Entity<DonDangKy>(entity =>
        {
            entity.HasIndex(e => new { e.DotId, e.MSSV }).IsUnique();

            entity.HasOne(e => e.SinhVien)
                  .WithMany(s => s.DonDangKys)
                  .HasForeignKey(e => e.MSSV)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.DotDangKy)
                  .WithMany(d => d.DonDangKys)
                  .HasForeignKey(e => e.DotId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.NhomDangKy)
                  .WithMany(n => n.DonDangKys)
                  .HasForeignKey(e => e.NhomId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ChiTietXepPhong>(entity =>
        {
            entity.HasOne(e => e.PhienXepPhong)
                  .WithMany(p => p.ChiTietXepPhongs)
                  .HasForeignKey(e => e.PhienId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.DonDangKy)
                  .WithMany(d => d.ChiTietXepPhongs)
                  .HasForeignKey(e => e.DonId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Phong)
                  .WithMany(p => p.ChiTietXepPhongs)
                  .HasForeignKey(e => e.PhongId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // --- HopDong & Stay ---
        modelBuilder.Entity<HopDong>(entity =>
        {
            entity.Property(e => e.RowVersion).IsRowVersion();

            // Filtered index: 1 student only 1 active contract
            entity.HasIndex(e => e.MSSV)
                  .IsUnique()
                  .HasFilter("[TrangThai] = 1"); // 1 = HieuLuc

            entity.HasOne(e => e.SinhVien)
                  .WithMany(s => s.HopDongs)
                  .HasForeignKey(e => e.MSSV)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Phong)
                  .WithMany(p => p.HopDongs)
                  .HasForeignKey(e => e.PhongId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LichSuChuyenPhong>(entity =>
        {
            entity.HasOne(e => e.HopDong)
                  .WithMany(h => h.LichSuChuyenPhongs)
                  .HasForeignKey(e => e.HopDongId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.TuPhong)
                  .WithMany()
                  .HasForeignKey(e => e.TuPhongId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DenPhong)
                  .WithMany()
                  .HasForeignKey(e => e.DenPhongId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Finance ---
        modelBuilder.Entity<ChiSoDienNuoc>(entity =>
        {
            entity.HasIndex(e => new { e.PhongId, e.Thang, e.Nam }).IsUnique();

            entity.HasOne(e => e.Phong)
                  .WithMany(p => p.ChiSoDienNuocs)
                  .HasForeignKey(e => e.PhongId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<HoaDonDienNuoc>(entity =>
        {
            entity.Property(e => e.MaThanhToan).HasMaxLength(50).IsRequired();
            entity.Property(e => e.RowVersion).IsRowVersion();
            entity.HasIndex(e => e.MaThanhToan).IsUnique();

            entity.HasOne(e => e.Phong)
                  .WithMany(p => p.HoaDonDienNuocs)
                  .HasForeignKey(e => e.PhongId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<KhoanThuTienPhong>(entity =>
        {
            entity.Property(e => e.MaThanhToan).HasMaxLength(50).IsRequired();
            entity.Property(e => e.RowVersion).IsRowVersion();
            entity.HasIndex(e => e.MaThanhToan).IsUnique();

            entity.HasOne(e => e.HopDong)
                  .WithMany(h => h.KhoanThuTienPhongs)
                  .HasForeignKey(e => e.HopDongId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GiaoDichNganHang>(entity =>
        {
            entity.Property(e => e.HashSaoKe).HasMaxLength(64).IsRequired();
            entity.HasIndex(e => e.HashSaoKe).IsUnique();

            entity.Property(e => e.MaGiaoDich).HasMaxLength(100);
            entity.HasIndex(e => e.MaGiaoDich)
                  .IsUnique()
                  .HasFilter("[MaGiaoDich] IS NOT NULL");
        });

        modelBuilder.Entity<ThanhToan>(entity =>
        {
            entity.HasOne(e => e.HoaDonDienNuoc)
                  .WithMany(h => h.ThanhToans)
                  .HasForeignKey(e => e.HoaDonDienNuocId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.KhoanThuTienPhong)
                  .WithMany(k => k.ThanhToans)
                  .HasForeignKey(e => e.KhoanThuTienPhongId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.GiaoDichNganHang)
                  .WithMany(g => g.ThanhToans)
                  .HasForeignKey(e => e.GiaoDichNganHangId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.NguoiThu)
                  .WithMany(u => u.ThanhToansThu)
                  .HasForeignKey(e => e.NguoiThuId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // --- System & User ---
        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.Property(e => e.TenDangNhap).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.TenDangNhap).IsUnique();

            entity.HasOne(e => e.VaiTro)
                  .WithMany(v => v.NguoiDungs)
                  .HasForeignKey(e => e.MaVaiTro)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CauHinh>(entity =>
        {
            entity.Property(e => e.Khoa).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Khoa).IsUnique();
        });
    }
}

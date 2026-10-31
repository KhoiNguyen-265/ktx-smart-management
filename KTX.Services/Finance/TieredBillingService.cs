using KTX.Data;
using KTX.Domain.DTOs;
using KTX.Domain.Entities;
using KTX.Domain.Enums;
using KTX.Infrastructure.VietQR;
using KTX.Services.Configuration;
using Microsoft.EntityFrameworkCore;

namespace KTX.Services.Finance;

public class TieredBillingService : ITieredBillingService
{
    private readonly IDbContextFactory<KtxDbContext> _contextFactory;
    private readonly IConfigurationService _configService;

    public TieredBillingService(
        IDbContextFactory<KtxDbContext> contextFactory,
        IConfigurationService configService)
    {
        _contextFactory = contextFactory;
        _configService = configService;
    }

    public async Task<List<UtilityReadingDto>> GetReadingsAsync(int thang, int nam, int? toaId = null, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var query = db.Phongs.AsNoTracking().Where(p => p.TrangThai == TrangThaiPhong.HoatDong);
        if (toaId.HasValue && toaId > 0)
        {
            query = query.Where(p => p.ToaId == toaId.Value);
        }

        var rooms = await query
            .Include(p => p.ToaNha)
            .OrderBy(p => p.ToaId).ThenBy(p => p.MaPhong)
            .ToListAsync(ct);

        var roomIds = rooms.Select(r => r.Id).ToList();

        // Nạp chỉ số của tháng đang chọn
        var currentReadings = await db.ChiSoDienNuocs.AsNoTracking()
            .Where(c => c.Thang == thang && c.Nam == nam && roomIds.Contains(c.PhongId))
            .ToDictionaryAsync(c => c.PhongId, ct);

        // Nạp chỉ số của 3 tháng trước để tính trung bình phát hiện tiêu thụ bất thường
        var rawPastReadings = await db.ChiSoDienNuocs.AsNoTracking()
            .Where(c => roomIds.Contains(c.PhongId) && !c.CoThayDongHoDien &&
                       ((c.Nam == nam && c.Thang < thang && c.Thang >= thang - 3) ||
                        (c.Nam == nam - 1 && thang <= 3 && c.Thang >= 12 + thang - 3)))
            .ToListAsync(ct);

        var pastReadings = rawPastReadings
            .GroupBy(c => c.PhongId)
            .ToDictionary(g => g.Key, g => g.Average(x => (double)x.SanLuongDien));

        var result = new List<UtilityReadingDto>();

        foreach (var r in rooms)
        {
            if (currentReadings.TryGetValue(r.Id, out var cr))
            {
                bool isAnomaly = false;
                if (!cr.CoThayDongHoDien && pastReadings.TryGetValue(r.Id, out var avg) && avg > 0)
                {
                    if ((double)cr.SanLuongDien > 1.5 * avg) isAnomaly = true;
                }

                result.Add(new UtilityReadingDto
                {
                    Id = cr.Id,
                    PhongId = r.Id,
                    MaPhong = r.MaPhong,
                    ToaTen = r.ToaNha.Ten,
                    Thang = thang,
                    Nam = nam,
                    DienCu = cr.DienCu,
                    DienMoi = cr.DienMoi,
                    CoThayDongHoDien = cr.CoThayDongHoDien,
                    DienChotDongHoCu = cr.DienChotDongHoCu,
                    DienDauDongHoMoi = cr.DienDauDongHoMoi,
                    SanLuongDien = cr.SanLuongDien,
                    NuocCu = cr.NuocCu,
                    NuocMoi = cr.NuocMoi,
                    CoThayDongHoNuoc = cr.CoThayDongHoNuoc,
                    NuocChotDongHoCu = cr.NuocChotDongHoCu,
                    NuocDauDongHoMoi = cr.NuocDauDongHoMoi,
                    SanLuongNuoc = cr.SanLuongNuoc,
                    GhiChu = cr.GhiChu,
                    CanhBaoTieuThuBatThuong = isAnomaly
                });
            }
            else
            {
                // Chưa có chỉ số tháng này -> lấy chỉ số mới của tháng trước làm chỉ số cũ
                var lastReading = await db.ChiSoDienNuocs.AsNoTracking()
                    .Where(c => c.PhongId == r.Id && (c.Nam < nam || (c.Nam == nam && c.Thang < thang)))
                    .OrderByDescending(c => c.Nam).ThenByDescending(c => c.Thang)
                    .FirstOrDefaultAsync(ct);

                decimal lastDien = lastReading?.DienMoi ?? 0m;
                decimal lastNuoc = lastReading?.NuocMoi ?? 0m;

                result.Add(new UtilityReadingDto
                {
                    Id = 0,
                    PhongId = r.Id,
                    MaPhong = r.MaPhong,
                    ToaTen = r.ToaNha.Ten,
                    Thang = thang,
                    Nam = nam,
                    DienCu = lastDien,
                    DienMoi = lastDien,
                    NuocCu = lastNuoc,
                    NuocMoi = lastNuoc
                });
            }
        }

        return result;
    }

    public async Task<bool> SaveReadingAsync(UtilityReadingSaveDto dto, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        // 1. Tính toán sản lượng điện
        decimal sanLuongDien = 0;
        if (!dto.CoThayDongHoDien)
        {
            if (dto.DienMoi < dto.DienCu)
            {
                throw new InvalidOperationException($"Chỉ số điện mới ({dto.DienMoi}) không được nhỏ hơn chỉ số cũ ({dto.DienCu}) khi không thay đồng hồ.");
            }
            sanLuongDien = dto.DienMoi - dto.DienCu;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(dto.GhiChu))
            {
                throw new InvalidOperationException("Bắt buộc phải nhập ghi chú lý do khi thay đồng hồ điện.");
            }
            decimal chotCu = dto.DienChotDongHoCu ?? dto.DienCu;
            decimal dauMoi = dto.DienDauDongHoMoi ?? 0m;
            sanLuongDien = (chotCu - dto.DienCu) + (dto.DienMoi - dauMoi);
        }

        // 2. Tính toán sản lượng nước
        decimal sanLuongNuoc = 0;
        if (!dto.CoThayDongHoNuoc)
        {
            if (dto.NuocMoi < dto.NuocCu)
            {
                throw new InvalidOperationException($"Chỉ số nước mới ({dto.NuocMoi}) không được nhỏ hơn chỉ số cũ ({dto.NuocCu}) khi không thay đồng hồ.");
            }
            sanLuongNuoc = dto.NuocMoi - dto.NuocCu;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(dto.GhiChu))
            {
                throw new InvalidOperationException("Bắt buộc phải nhập ghi chú lý do khi thay đồng hồ nước.");
            }
            decimal chotCu = dto.NuocChotDongHoCu ?? dto.NuocCu;
            decimal dauMoi = dto.NuocDauDongHoMoi ?? 0m;
            sanLuongNuoc = (chotCu - dto.NuocCu) + (dto.NuocMoi - dauMoi);
        }

        var reading = await db.ChiSoDienNuocs.FirstOrDefaultAsync(
            c => c.PhongId == dto.PhongId && c.Thang == dto.Thang && c.Nam == dto.Nam, ct);

        if (reading == null)
        {
            reading = new ChiSoDienNuoc
            {
                PhongId = dto.PhongId,
                Thang = dto.Thang,
                Nam = dto.Nam
            };
            db.ChiSoDienNuocs.Add(reading);
        }

        reading.DienCu = dto.DienCu;
        reading.DienMoi = dto.DienMoi;
        reading.CoThayDongHoDien = dto.CoThayDongHoDien;
        reading.DienChotDongHoCu = dto.DienChotDongHoCu;
        reading.DienDauDongHoMoi = dto.DienDauDongHoMoi;
        reading.SanLuongDien = sanLuongDien;

        reading.NuocCu = dto.NuocCu;
        reading.NuocMoi = dto.NuocMoi;
        reading.CoThayDongHoNuoc = dto.CoThayDongHoNuoc;
        reading.NuocChotDongHoCu = dto.NuocChotDongHoCu;
        reading.NuocDauDongHoMoi = dto.NuocDauDongHoMoi;
        reading.SanLuongNuoc = sanLuongNuoc;
        reading.GhiChu = dto.GhiChu;

        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<TieredCalculationResultDto> CalculateBillPreviewAsync(
        decimal sanLuongDien,
        decimal sanLuongNuoc,
        int soNguoiDinhMuc,
        CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        var config = await _configService.GetSystemConfigAsync(ct);

        var date = DateTime.Now;
        var electricityTiers = await db.BacGias.AsNoTracking()
            .Where(b => b.LoaiDichVu == LoaiDichVu.Dien && b.ApDungTu <= date)
            .OrderBy(b => b.Bac)
            .ToListAsync(ct);

        var waterTiers = await db.BacGias.AsNoTracking()
            .Where(b => b.LoaiDichVu == LoaiDichVu.Nuoc && b.ApDungTu <= date)
            .OrderBy(b => b.Bac)
            .ToListAsync(ct);

        int m = Math.Max(1, (int)Math.Ceiling(soNguoiDinhMuc / 4.0));

        var result = new TieredCalculationResultDto
        {
            SanLuongDien = sanLuongDien,
            SanLuongNuoc = sanLuongNuoc,
            SoNguoiDinhMuc = soNguoiDinhMuc,
            HeSoDinhMuc = m
        };

        // Tính điện bậc thang
        decimal remainingElec = sanLuongDien;
        decimal totalElec = 0;
        foreach (var t in electricityTiers)
        {
            if (remainingElec <= 0) break;

            decimal tierCapacity = t.DenMuc.HasValue ? (t.DenMuc.Value - t.TuMuc) * m : decimal.MaxValue;
            decimal consumedInTier = Math.Min(remainingElec, tierCapacity);

            var item = new TierDetailDto
            {
                Bac = t.Bac,
                LoaiDichVu = LoaiDichVu.Dien,
                NoiDung = $"Điện Bậc {t.Bac} ({t.TuMuc * m} - {(t.DenMuc.HasValue ? (t.DenMuc.Value * m).ToString() : "∞")} kWh)",
                TuMuc = t.TuMuc * m,
                DenMuc = t.DenMuc.HasValue ? t.DenMuc.Value * m : null,
                SanLuong = consumedInTier,
                DonGia = t.DonGia
            };
            result.ChiTietTiers.Add(item);

            totalElec += item.ThanhTien;
            remainingElec -= consumedInTier;
        }
        result.TienDien = totalElec;

        // Tính nước bậc thang
        decimal remainingWater = sanLuongNuoc;
        decimal totalWater = 0;
        foreach (var t in waterTiers)
        {
            if (remainingWater <= 0) break;

            decimal tierCapacity = t.DenMuc.HasValue ? (t.DenMuc.Value - t.TuMuc) * m : decimal.MaxValue;
            decimal consumedInTier = Math.Min(remainingWater, tierCapacity);

            var item = new TierDetailDto
            {
                Bac = t.Bac,
                LoaiDichVu = LoaiDichVu.Nuoc,
                NoiDung = $"Nước Bậc {t.Bac} ({t.TuMuc * m} - {(t.DenMuc.HasValue ? (t.DenMuc.Value * m).ToString() : "∞")} m³)",
                TuMuc = t.TuMuc * m,
                DenMuc = t.DenMuc.HasValue ? t.DenMuc.Value * m : null,
                SanLuong = consumedInTier,
                DonGia = t.DonGia
            };
            result.ChiTietTiers.Add(item);

            totalWater += item.ThanhTien;
            remainingWater -= consumedInTier;
        }
        result.TienNuoc = totalWater;

        // Phí dịch vụ chung & VAT
        result.PhiDichVuChung = 50000m; // 50.000 đ vệ sinh & bảo vệ chung / phòng
        result.TienVat = Math.Round((result.TienDien + result.TienNuoc + result.PhiDichVuChung) * config.VatRate, 0);

        return result;
    }

    public async Task<List<InvoiceDto>> GenerateMonthlyInvoicesAsync(int thang, int nam, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        var config = await _configService.GetSystemConfigAsync(ct);

        var readings = await db.ChiSoDienNuocs
            .Include(c => c.Phong).ThenInclude(p => p.ToaNha)
            .Include(c => c.Phong).ThenInclude(p => p.HopDongs.Where(h => h.TrangThai == TrangThaiHopDong.HieuLuc))
            .Where(c => c.Thang == thang && c.Nam == nam)
            .ToListAsync(ct);

        var existingInvoices = await db.HoaDonDienNuocs
            .Where(h => h.Thang == thang && h.Nam == nam)
            .ToDictionaryAsync(h => h.PhongId, ct);

        var invoices = new List<HoaDonDienNuoc>();

        foreach (var r in readings)
        {
            int residentsCount = Math.Max(1, r.Phong.HopDongs.Count(h => h.TrangThai == TrangThaiHopDong.HieuLuc));
            var bill = await CalculateBillPreviewAsync(r.SanLuongDien, r.SanLuongNuoc, residentsCount, ct);

            if (existingInvoices.TryGetValue(r.PhongId, out var inv))
            {
                inv.SoNguoiTinhDinhMuc = residentsCount;
                inv.TongTien = bill.TongCong;
                // Recreate detail items
                db.ChiTietHoaDons.RemoveRange(db.ChiTietHoaDons.Where(d => d.HoaDonId == inv.Id));
                foreach (var detail in bill.ChiTietTiers)
                {
                    db.ChiTietHoaDons.Add(new ChiTietHoaDon
                    {
                        HoaDonId = inv.Id,
                        NoiDung = detail.NoiDung,
                        SoLuong = detail.SanLuong,
                        DonGia = detail.DonGia,
                        ThanhTien = detail.ThanhTien
                    });
                }
                invoices.Add(inv);
            }
            else
            {
                var newInv = new HoaDonDienNuoc
                {
                    PhongId = r.PhongId,
                    Thang = thang,
                    Nam = nam,
                    SoNguoiTinhDinhMuc = residentsCount,
                    TongTien = bill.TongCong,
                    DaThu = 0m,
                    TrangThai = TrangThaiHoaDon.ChuaThanhToan
                };
                db.HoaDonDienNuocs.Add(newInv);
                await db.SaveChangesAsync(ct);

                // Gán mã thanh toán KTXDN...
                newInv.MaThanhToan = $"KTXDN{newInv.Id:D6}";

                foreach (var detail in bill.ChiTietTiers)
                {
                    db.ChiTietHoaDons.Add(new ChiTietHoaDon
                    {
                        HoaDonId = newInv.Id,
                        NoiDung = detail.NoiDung,
                        SoLuong = detail.SanLuong,
                        DonGia = detail.DonGia,
                        ThanhTien = detail.ThanhTien
                    });
                }
                invoices.Add(newInv);
            }
        }

        await db.SaveChangesAsync(ct);

        return await GetInvoicesAsync(thang, nam, ct: ct);
    }

    public async Task<List<InvoiceDto>> GetInvoicesAsync(
        int thang,
        int nam,
        TrangThaiHoaDon? status = null,
        string? search = null,
        CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        var config = await _configService.GetSystemConfigAsync(ct);

        var query = db.HoaDonDienNuocs.AsNoTracking()
            .Include(h => h.Phong).ThenInclude(p => p.ToaNha)
            .Include(h => h.ChiTietHoaDons)
            .Where(h => h.Thang == thang && h.Nam == nam);

        if (status.HasValue)
        {
            query = query.Where(h => h.TrangThai == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(h => h.MaThanhToan.Contains(s) || h.Phong.MaPhong.Contains(s));
        }

        var list = await query.OrderBy(h => h.Phong.MaPhong).ToListAsync(ct);

        return list.Select(h =>
        {
            // Sinh mã QR VietQR chuẩn EMVCo offline
            string qrPayload = VietQRHelper.BuildEmvCoString(
                config.BankBin,
                config.BankAccountNumber,
                h.TongTien - h.DaThu,
                h.MaThanhToan);

            return new InvoiceDto
            {
                Id = h.Id,
                MaThanhToan = h.MaThanhToan,
                PhongId = h.PhongId,
                MaPhong = h.Phong.MaPhong,
                ToaTen = h.Phong.ToaNha.Ten,
                Thang = h.Thang,
                Nam = h.Nam,
                SoNguoiTinhDinhMuc = h.SoNguoiTinhDinhMuc,
                TongTien = h.TongTien,
                DaThu = h.DaThu,
                TrangThai = h.TrangThai,
                VietQrPayload = qrPayload,
                ChiTietRows = h.ChiTietHoaDons.Select(d => $"{d.NoiDung}: {d.SoLuong} x {d.DonGia:N0} = {d.ThanhTien:N0} đ").ToList()
            };
        }).ToList();
    }

    public async Task<List<RoomFeeDto>> GenerateRoomFeesAsync(DateTime tuNgay, DateTime denNgay, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        var config = await _configService.GetSystemConfigAsync(ct);

        var activeContracts = await db.HopDongs
            .Include(h => h.SinhVien)
            .Include(h => h.Phong).ThenInclude(p => p.ToaNha)
            .Where(h => h.TrangThai == TrangThaiHopDong.HieuLuc)
            .ToListAsync(ct);

        var existingFees = await db.KhoanThuTienPhongs
            .Where(k => k.TuNgay == tuNgay && k.DenNgay == denNgay)
            .ToDictionaryAsync(k => k.HopDongId, ct);

        int months = Math.Max(1, (int)Math.Round((denNgay - tuNgay).TotalDays / 30.0));

        foreach (var c in activeContracts)
        {
            decimal totalBase = c.GiaThang * months;
            decimal discount = Math.Round(totalBase * (c.MienGiamPhanTram / 100m), 0);
            decimal payable = totalBase - discount;

            if (!existingFees.ContainsKey(c.Id))
            {
                var fee = new KhoanThuTienPhong
                {
                    HopDongId = c.Id,
                    TuNgay = tuNgay,
                    DenNgay = denNgay,
                    SoTienGoc = totalBase,
                    MienGiam = discount,
                    PhaiThu = payable,
                    DaThu = 0m,
                    TrangThai = TrangThaiHoaDon.ChuaThanhToan
                };
                db.KhoanThuTienPhongs.Add(fee);
                await db.SaveChangesAsync(ct);

                fee.MaThanhToan = $"KTXTP{fee.Id:D6}";
            }
        }

        await db.SaveChangesAsync(ct);
        return await GetRoomFeesAsync(ct: ct);
    }

    public async Task<List<RoomFeeDto>> GetRoomFeesAsync(
        TrangThaiHoaDon? status = null,
        string? search = null,
        CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);
        var config = await _configService.GetSystemConfigAsync(ct);

        var query = db.KhoanThuTienPhongs.AsNoTracking()
            .Include(k => k.HopDong).ThenInclude(h => h.SinhVien)
            .Include(k => k.HopDong).ThenInclude(h => h.Phong).ThenInclude(p => p.ToaNha)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(k => k.TrangThai == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(k => k.MaThanhToan.Contains(s) ||
                                     k.HopDong.MSSV.Contains(s) ||
                                     k.HopDong.SinhVien.HoTen.Contains(s) ||
                                     k.HopDong.Phong.MaPhong.Contains(s));
        }

        var list = await query.OrderByDescending(k => k.Id).ToListAsync(ct);

        return list.Select(k =>
        {
            string qrPayload = VietQRHelper.BuildEmvCoString(
                config.BankBin,
                config.BankAccountNumber,
                k.PhaiThu - k.DaThu,
                k.MaThanhToan);

            return new RoomFeeDto
            {
                Id = k.Id,
                MaThanhToan = k.MaThanhToan,
                HopDongId = k.HopDongId,
                MSSV = k.HopDong.MSSV,
                HoTen = k.HopDong.SinhVien.HoTen,
                MaPhong = k.HopDong.Phong.MaPhong,
                ToaTen = k.HopDong.Phong.ToaNha.Ten,
                TuNgay = k.TuNgay,
                DenNgay = k.DenNgay,
                SoTienGoc = k.SoTienGoc,
                MienGiam = k.MienGiam,
                PhaiThu = k.PhaiThu,
                DaThu = k.DaThu,
                TrangThai = k.TrangThai,
                VietQrPayload = qrPayload
            };
        }).ToList();
    }

    public async Task<bool> RecordCashPaymentAsync(int? invoiceId, int? roomFeeId, decimal amount, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        if (invoiceId.HasValue && invoiceId > 0)
        {
            var inv = await db.HoaDonDienNuocs.FindAsync(new object[] { invoiceId.Value }, ct);
            if (inv == null) return false;

            inv.DaThu += amount;
            if (inv.DaThu >= inv.TongTien) inv.TrangThai = TrangThaiHoaDon.DaThanhToan;
            else if (inv.DaThu > 0) inv.TrangThai = TrangThaiHoaDon.ThanhToanMotPhan;

            db.ThanhToans.Add(new ThanhToan
            {
                HoaDonDienNuocId = inv.Id,
                SoTien = amount,
                HinhThuc = HinhThucThanhToan.TienMat,
                ThoiGian = DateTime.Now,
                NguoiThuId = 1
            });
        }
        else if (roomFeeId.HasValue && roomFeeId > 0)
        {
            var fee = await db.KhoanThuTienPhongs.FindAsync(new object[] { roomFeeId.Value }, ct);
            if (fee == null) return false;

            fee.DaThu += amount;
            if (fee.DaThu >= fee.PhaiThu) fee.TrangThai = TrangThaiHoaDon.DaThanhToan;
            else if (fee.DaThu > 0) fee.TrangThai = TrangThaiHoaDon.ThanhToanMotPhan;

            db.ThanhToans.Add(new ThanhToan
            {
                KhoanThuTienPhongId = fee.Id,
                SoTien = amount,
                HinhThuc = HinhThucThanhToan.TienMat,
                ThoiGian = DateTime.Now,
                NguoiThuId = 1
            });
        }
        else
        {
            return false;
        }

        await db.SaveChangesAsync(ct);
        return true;
    }
}

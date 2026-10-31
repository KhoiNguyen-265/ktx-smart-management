using System.Globalization;
using KTX.Data;
using KTX.Domain.DTOs;
using KTX.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KTX.Services.Configuration;

public class ConfigurationService : IConfigurationService
{
    private readonly IDbContextFactory<KtxDbContext> _contextFactory;

    public ConfigurationService(IDbContextFactory<KtxDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<SystemConfigDto> GetSystemConfigAsync(CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var configs = await db.CauHinhs.AsNoTracking().ToDictionaryAsync(c => c.Khoa, c => c.GiaTri, ct);

        var result = new SystemConfigDto();

        if (configs.TryGetValue("w_dien", out var wDien) && double.TryParse(wDien, NumberStyles.Any, CultureInfo.InvariantCulture, out var dDien))
            result.WPriority = dDien;

        if (configs.TryGetValue("w_khoangcach", out var wKc) && double.TryParse(wKc, NumberStyles.Any, CultureInfo.InvariantCulture, out var dKc))
            result.WDistance = dKc;

        if (configs.TryGetValue("w_khoahoc", out var wKh) && double.TryParse(wKh, NumberStyles.Any, CultureInfo.InvariantCulture, out var dKh))
            result.WCourse = dKh;

        if (configs.TryGetValue("w_nguyenvong", out var wNv) && double.TryParse(wNv, NumberStyles.Any, CultureInfo.InvariantCulture, out var dNv))
            result.WPreference = dNv;

        if (configs.TryGetValue("w_nhom", out var wNhom) && double.TryParse(wNhom, NumberStyles.Any, CultureInfo.InvariantCulture, out var dNhom))
            result.WRoommate = dNhom;

        if (configs.TryGetValue("VAT", out var vat) && decimal.TryParse(vat, NumberStyles.Any, CultureInfo.InvariantCulture, out var dVat))
            result.VatRate = dVat;

        if (configs.TryGetValue("BankBin", out var bin))
            result.BankBin = bin;

        if (configs.TryGetValue("BankName", out var bankName))
            result.BankName = bankName;

        if (configs.TryGetValue("BankAccountNumber", out var accNum))
            result.BankAccountNumber = accNum;

        if (configs.TryGetValue("BankAccountHolder", out var accHolder))
            result.BankAccountHolder = accHolder;

        return result;
    }

    public async Task<bool> SaveSystemConfigAsync(SystemConfigDto dto, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var settings = new Dictionary<string, (string Value, string Description)>
        {
            ["w_dien"] = (dto.WPriority.ToString("0.##", CultureInfo.InvariantCulture), "Trọng số diện ưu tiên"),
            ["w_khoangcach"] = (dto.WDistance.ToString("0.##", CultureInfo.InvariantCulture), "Trọng số khoảng cách quê quán"),
            ["w_khoahoc"] = (dto.WCourse.ToString("0.##", CultureInfo.InvariantCulture), "Trọng số khóa học"),
            ["w_nguyenvong"] = (dto.WPreference.ToString("0.##", CultureInfo.InvariantCulture), "Trọng số nguyện vọng"),
            ["w_nhom"] = (dto.WRoommate.ToString("0.##", CultureInfo.InvariantCulture), "Trọng số nhóm bạn"),
            ["VAT"] = (dto.VatRate.ToString("0.##", CultureInfo.InvariantCulture), "Tỉ lệ thuế VAT"),
            ["BankBin"] = (dto.BankBin, "Mã BIN ngân hàng"),
            ["BankName"] = (dto.BankName, "Tên ngân hàng"),
            ["BankAccountNumber"] = (dto.BankAccountNumber, "Số tài khoản nhận chuyển khoản VietQR"),
            ["BankAccountHolder"] = (dto.BankAccountHolder, "Chủ tài khoản ngân hàng")
        };

        var existingList = await db.CauHinhs.ToListAsync(ct);
        var existingDict = existingList.ToDictionary(c => c.Khoa);

        foreach (var kvp in settings)
        {
            if (existingDict.TryGetValue(kvp.Key, out var item))
            {
                item.GiaTri = kvp.Value.Value;
                item.MoTa = kvp.Value.Description;
            }
            else
            {
                db.CauHinhs.Add(new CauHinh
                {
                    Khoa = kvp.Key,
                    GiaTri = kvp.Value.Value,
                    MoTa = kvp.Value.Description
                });
            }
        }

        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<PriorityDto>> GetPrioritiesAsync(CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        return await db.DienUuTiens.AsNoTracking()
            .OrderByDescending(p => p.DiemCong)
            .Select(p => new PriorityDto
            {
                Id = p.Id,
                Ten = p.Ten,
                DiemCong = p.DiemCong
            })
            .ToListAsync(ct);
    }

    public async Task<bool> SavePriorityAsync(PriorityDto dto, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        if (dto.Id <= 0)
        {
            var p = new DienUuTien
            {
                Ten = dto.Ten.Trim(),
                DiemCong = dto.DiemCong
            };
            db.DienUuTiens.Add(p);
        }
        else
        {
            var p = await db.DienUuTiens.FindAsync(new object[] { dto.Id }, ct);
            if (p == null) return false;
            p.Ten = dto.Ten.Trim();
            p.DiemCong = dto.DiemCong;
        }

        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeletePriorityAsync(int id, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var p = await db.DienUuTiens.Include(x => x.SinhViens).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p == null || p.SinhViens.Any()) return false;

        db.DienUuTiens.Remove(p);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<ProvinceDto>> GetProvincesAsync(CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        return await db.TinhThanhs.AsNoTracking()
            .Include(t => t.TinhThanhCus)
            .OrderBy(t => t.Ten)
            .Select(t => new ProvinceDto
            {
                Id = t.Id,
                Ten = t.Ten,
                KhoangCachKm = t.KhoangCachKm,
                NhomKhoangCach = t.NhomKhoangCach,
                TenCus = t.TinhThanhCus.Select(c => c.TenCu).ToList()
            })
            .ToListAsync(ct);
    }

    public async Task<bool> UpdateProvinceDistanceAsync(int id, int km, int group, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var p = await db.TinhThanhs.FindAsync(new object[] { id }, ct);
        if (p == null) return false;

        p.KhoangCachKm = km;
        p.NhomKhoangCach = group;

        await db.SaveChangesAsync(ct);
        return true;
    }
}

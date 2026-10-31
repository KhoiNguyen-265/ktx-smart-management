using ClosedXML.Excel;
using KTX.Data;
using KTX.Domain.DTOs;
using KTX.Domain.Entities;
using KTX.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace KTX.Services.Registration;

public class RegistrationService : IRegistrationService
{
    private readonly IDbContextFactory<KtxDbContext> _contextFactory;

    public RegistrationService(IDbContextFactory<KtxDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<DotDangKyDto>> GetBatchesAsync(CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        return await db.DotDangKys.AsNoTracking()
            .OrderByDescending(d => d.Id)
            .Select(d => new DotDangKyDto
            {
                Id = d.Id,
                Ten = d.Ten,
                NgayMo = d.NgayMo,
                NgayDong = d.NgayDong,
                DangMo = d.DangMo,
                SoDon = d.DonDangKys.Count
            })
            .ToListAsync(ct);
    }

    public async Task<DotDangKyDto?> GetBatchByIdAsync(int batchId, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        return await db.DotDangKys.AsNoTracking()
            .Where(d => d.Id == batchId)
            .Select(d => new DotDangKyDto
            {
                Id = d.Id,
                Ten = d.Ten,
                NgayMo = d.NgayMo,
                NgayDong = d.NgayDong,
                DangMo = d.DangMo,
                SoDon = d.DonDangKys.Count
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> CreateBatchAsync(string tenDot, DateTime ngayMo, DateTime ngayDong, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var batch = new DotDangKy
        {
            Ten = tenDot.Trim(),
            NgayMo = ngayMo,
            NgayDong = ngayDong,
            DangMo = true
        };

        db.DotDangKys.Add(batch);
        await db.SaveChangesAsync(ct);
        return batch.Id;
    }

    public async Task<List<DonDangKyDto>> GetApplicationsAsync(int batchId, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        return await db.DonDangKys.AsNoTracking()
            .Where(d => d.DotId == batchId)
            .Include(d => d.SinhVien).ThenInclude(s => s.TinhThanh)
            .Include(d => d.SinhVien).ThenInclude(s => s.DienUuTien)
            .Include(d => d.NhomDangKy)
            .Include(d => d.LoaiPhongMongMuon)
            .Include(d => d.ToaMongMuon)
            .OrderByDescending(d => d.DiemUuTien)
            .Select(d => new DonDangKyDto
            {
                Id = d.Id,
                DotId = d.DotId,
                MSSV = d.MSSV,
                HoTen = d.SinhVien.HoTen,
                GioiTinh = d.SinhVien.GioiTinh,
                CCCD = d.SinhVien.CCCD,
                Khoa = d.SinhVien.Khoa,
                KhoaHoc = d.SinhVien.KhoaHoc,
                TinhThanh = d.SinhVien.TinhThanh.Ten,
                KhoangCachKm = d.SinhVien.TinhThanh.KhoangCachKm,
                NhomKhoangCach = d.SinhVien.TinhThanh.NhomKhoangCach,
                DienUuTien = d.SinhVien.DienUuTien != null ? d.SinhVien.DienUuTien.Ten : null,
                DiemCongDien = d.SinhVien.DienUuTien != null ? d.SinhVien.DienUuTien.DiemCong : 0,
                DiemUuTien = d.DiemUuTien,
                NhomId = d.NhomId,
                MaNhom = d.NhomDangKy != null ? d.NhomDangKy.MaNhom : null,
                LoaiPhongMongMuonId = d.LoaiPhongMongMuonId,
                LoaiPhongMongMuonTen = d.LoaiPhongMongMuon != null ? d.LoaiPhongMongMuon.Ten : "Tự do",
                BatBuocLoaiPhong = d.BatBuocLoaiPhong,
                ToaMongMuonId = d.ToaMongMuonId,
                ToaMongMuonTen = d.ToaMongMuon != null ? d.ToaMongMuon.Ten : "Tự do",
                TangMongMuon = d.TangMongMuon,
                ThoiGianNop = d.ThoiGianNop,
                TrangThai = d.TrangThai
            })
            .ToListAsync(ct);
    }

    public async Task<byte[]> GenerateApplicationTemplateAsync(CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var roomTypes = await db.LoaiPhongs.AsNoTracking().OrderBy(r => r.SucChua).Select(r => r.Ten).ToListAsync(ct);
        var buildings = await db.ToaNhas.AsNoTracking().OrderBy(t => t.Ten).Select(t => t.Ten).ToListAsync(ct);

        using var workbook = new XLWorkbook();

        var wsMeta = workbook.Worksheets.Add("DanhMuc");
        for (int i = 0; i < roomTypes.Count; i++) wsMeta.Cell(i + 1, 1).Value = roomTypes[i];
        for (int i = 0; i < buildings.Count; i++) wsMeta.Cell(i + 1, 2).Value = buildings[i];

        var ws = workbook.Worksheets.Add("DonDangKy");
        string[] headers =
        {
            "MSSV (*)",
            "Mã nhóm (nếu ở cùng bạn)",
            "Loại phòng mong muốn",
            "Bắt buộc đúng loại phòng (Có/Không)",
            "Tòa mong muốn",
            "Tầng mong muốn",
            "Thời gian nộp (dd/MM/yyyy HH:mm)"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E40AF");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // Sample rows
        ws.Cell(2, 1).Value = "20260001";
        ws.Cell(2, 2).Value = "GROUP_A";
        ws.Cell(2, 3).Value = roomTypes.FirstOrDefault() ?? "Phòng 4 người";
        ws.Cell(2, 4).Value = "Có";
        ws.Cell(2, 5).Value = buildings.FirstOrDefault() ?? "Tòa A";
        ws.Cell(2, 6).Value = 2;
        ws.Cell(2, 7).Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        ws.Cell(3, 1).Value = "20260002";
        ws.Cell(3, 2).Value = "GROUP_A";
        ws.Cell(3, 3).Value = roomTypes.FirstOrDefault() ?? "Phòng 4 người";
        ws.Cell(3, 4).Value = "Có";
        ws.Cell(3, 5).Value = buildings.FirstOrDefault() ?? "Tòa A";
        ws.Cell(3, 6).Value = 2;
        ws.Cell(3, 7).Value = DateTime.Now.AddMinutes(5).ToString("dd/MM/yyyy HH:mm");

        // Validation for room types
        if (roomTypes.Count > 0)
        {
            var vRoom = ws.Range("C2:C500").CreateDataValidation();
            vRoom.List(wsMeta.Range(1, 1, roomTypes.Count, 1), true);
        }

        // Validation for Mandatory Room Type
        var vMandatory = ws.Range("D2:D500").CreateDataValidation();
        vMandatory.List("Có,Không", true);

        // Validation for buildings
        if (buildings.Count > 0)
        {
            var vBld = ws.Range("E2:E500").CreateDataValidation();
            vBld.List(wsMeta.Range(1, 2, buildings.Count, 1), true);
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<ExcelImportResultDto> ImportApplicationsFromExcelAsync(int batchId, Stream excelStream, CancellationToken ct = default)
    {
        var result = new ExcelImportResultDto();

        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var batch = await db.DotDangKys.FindAsync(new object[] { batchId }, ct);
        if (batch == null)
        {
            result.Errors.Add(new ExcelImportRowError { RowIndex = 0, ErrorMessage = "Đợt đăng ký không tồn tại." });
            return result;
        }

        var allStudents = await db.SinhViens.AsNoTracking()
            .Include(s => s.TinhThanh)
            .Include(s => s.DienUuTien)
            .ToDictionaryAsync(s => s.MSSV, s => s, ct);

        var existingApplications = (await db.DonDangKys.AsNoTracking()
            .Where(d => d.DotId == batchId)
            .Select(d => d.MSSV)
            .ToListAsync(ct)).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var roomTypes = await db.LoaiPhongs.AsNoTracking().ToDictionaryAsync(r => r.Ten.Trim(), r => r.Id, StringComparer.OrdinalIgnoreCase);
        var buildings = await db.ToaNhas.AsNoTracking().ToDictionaryAsync(b => b.Ten.Trim(), b => b.Id, StringComparer.OrdinalIgnoreCase);
        int maxCapacity = await db.LoaiPhongs.MaxAsync(r => r.SucChua, ct);

        using var workbook = new XLWorkbook(excelStream);
        var ws = workbook.Worksheet(1);
        var rowCount = ws.LastRowUsed()?.RowNumber() ?? 0;

        if (rowCount < 2)
        {
            result.Errors.Add(new ExcelImportRowError { RowIndex = 0, ErrorMessage = "File Excel không có dữ liệu." });
            return result;
        }

        // Temporary parsed rows
        var rawRows = new List<(int RowIndex, string Mssv, string? MaNhom, string? RoomTypeStr, bool BatBuoc, string? BuildingStr, int? Floor, DateTime SubTime)>();
        var batchMssvSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int r = 2; r <= rowCount; r++)
        {
            var row = ws.Row(r);
            if (row.IsEmpty()) continue;

            string mssv = row.Cell(1).GetString().Trim();
            if (string.IsNullOrWhiteSpace(mssv)) continue;

            result.TotalRows++;

            if (!allStudents.ContainsKey(mssv))
            {
                result.FailedCount++;
                result.Errors.Add(new ExcelImportRowError
                {
                    RowIndex = r,
                    Identifier = mssv,
                    ErrorMessage = $"MSSV '{mssv}' chưa có trong danh sách sinh viên trường."
                });
                continue;
            }

            if (existingApplications.Contains(mssv))
            {
                result.FailedCount++;
                result.Errors.Add(new ExcelImportRowError
                {
                    RowIndex = r,
                    Identifier = mssv,
                    ErrorMessage = $"Sinh viên '{mssv}' đã nộp đơn trong đợt này rồi."
                });
                continue;
            }

            if (batchMssvSet.Contains(mssv))
            {
                result.FailedCount++;
                result.Errors.Add(new ExcelImportRowError
                {
                    RowIndex = r,
                    Identifier = mssv,
                    ErrorMessage = $"MSSV '{mssv}' bị nộp 2 đơn trùng nhau ngay trong file Excel."
                });
                continue;
            }

            string maNhom = row.Cell(2).GetString().Trim();
            string roomType = row.Cell(3).GetString().Trim();
            string batBuocStr = row.Cell(4).GetString().Trim().ToLowerInvariant();
            bool batBuoc = (batBuocStr == "có" || batBuocStr == "co" || batBuocStr == "yes" || batBuocStr == "true" || batBuocStr == "1");
            string building = row.Cell(5).GetString().Trim();

            int? floor = null;
            if (row.Cell(6).TryGetValue(out int f) && f > 0) floor = f;

            DateTime subTime = DateTime.Now;
            if (row.Cell(7).TryGetValue(out DateTime dt)) subTime = dt;

            batchMssvSet.Add(mssv);
            rawRows.Add((r, mssv, string.IsNullOrWhiteSpace(maNhom) ? null : maNhom, roomType, batBuoc, building, floor, subTime));
        }

        // Group-level validation
        var groupBuckets = rawRows
            .Where(x => !string.IsNullOrEmpty(x.MaNhom))
            .GroupBy(x => x.MaNhom!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList());

        var rejectedGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (groupName, members) in groupBuckets)
        {
            if (members.Count > maxCapacity)
            {
                rejectedGroups.Add(groupName);
                foreach (var m in members)
                {
                    result.FailedCount++;
                    result.Errors.Add(new ExcelImportRowError
                    {
                        RowIndex = m.RowIndex,
                        Identifier = m.Mssv,
                        ErrorMessage = $"Nhóm '{groupName}' có {members.Count} người, vượt quá sức chứa lớn nhất ({maxCapacity} người)."
                    });
                }
                continue;
            }

            // Check gender consistency
            var genders = members.Select(m => allStudents[m.Mssv].GioiTinh).Distinct().ToList();
            if (genders.Count > 1)
            {
                rejectedGroups.Add(groupName);
                foreach (var m in members)
                {
                    result.FailedCount++;
                    result.Errors.Add(new ExcelImportRowError
                    {
                        RowIndex = m.RowIndex,
                        Identifier = m.Mssv,
                        ErrorMessage = $"Nhóm '{groupName}' bị từ chối do có cả sinh viên Nam và Nữ trong cùng nhóm."
                    });
                }
            }
        }

        // Create or get NhomDangKy entities for valid groups
        var validGroupEntities = new Dictionary<string, NhomDangKy>(StringComparer.OrdinalIgnoreCase);
        foreach (var (groupName, members) in groupBuckets)
        {
            if (rejectedGroups.Contains(groupName)) continue;

            var existingNhom = await db.NhomDangKys.FirstOrDefaultAsync(n => n.MaNhom == groupName, ct);
            if (existingNhom == null)
            {
                existingNhom = new NhomDangKy
                {
                    MaNhom = groupName,
                    MSSVTruongNhom = members.First().Mssv
                };
                db.NhomDangKys.Add(existingNhom);
                await db.SaveChangesAsync(ct);
            }
            validGroupEntities[groupName] = existingNhom;
        }

        // Add valid applications to DB
        var newApplications = new List<DonDangKy>();

        foreach (var item in rawRows)
        {
            if (!string.IsNullOrEmpty(item.MaNhom) && rejectedGroups.Contains(item.MaNhom))
            {
                continue; // Already counted as failed above
            }

            var sv = allStudents[item.Mssv];

            // Resolve Preferences (if group, adopt leader preferences if divergent)
            int? loaiPhongId = null;
            if (!string.IsNullOrEmpty(item.RoomTypeStr) && roomTypes.TryGetValue(item.RoomTypeStr, out int rId))
                loaiPhongId = rId;

            int? toaId = null;
            if (!string.IsNullOrEmpty(item.BuildingStr) && buildings.TryGetValue(item.BuildingStr, out int bId))
                toaId = bId;

            // Calculate Priority Score P:
            // P = DiemCong_dien + 10*[khoahoc==68] + DiemKC(NhomKC) + tie_breaker
            decimal priorityScore = 0;
            if (sv.DienUuTien != null) priorityScore += sv.DienUuTien.DiemCong;
            if (sv.KhoaHoc == 68) priorityScore += 10; // Tân sinh viên khóa mới nhất
            priorityScore += sv.TinhThanh.NhomKhoangCach switch
            {
                3 => 20,
                2 => 10,
                _ => 0
            };

            int? nhomEntityId = null;
            if (!string.IsNullOrEmpty(item.MaNhom) && validGroupEntities.TryGetValue(item.MaNhom, out var nhomEntity))
            {
                nhomEntityId = nhomEntity.Id;
            }

            var app = new DonDangKy
            {
                DotId = batchId,
                MSSV = item.Mssv,
                NhomId = nhomEntityId,
                LoaiPhongMongMuonId = loaiPhongId,
                BatBuocLoaiPhong = item.BatBuoc,
                ToaMongMuonId = toaId,
                TangMongMuon = item.Floor,
                DiemUuTien = priorityScore,
                ThoiGianNop = item.SubTime,
                TrangThai = TrangThaiDonDangKy.ChoXep
            };

            newApplications.Add(app);
            result.SuccessCount++;
        }

        if (newApplications.Count > 0)
        {
            db.DonDangKys.AddRange(newApplications);
            await db.SaveChangesAsync(ct);
        }

        return result;
    }
}

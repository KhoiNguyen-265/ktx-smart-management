using ClosedXML.Excel;
using KTX.Data;
using KTX.Domain.DTOs;
using KTX.Domain.Entities;
using KTX.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace KTX.Services.Students;

public class StudentService : IStudentService
{
    private readonly IDbContextFactory<KtxDbContext> _contextFactory;

    public StudentService(IDbContextFactory<KtxDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<PagedResult<StudentDto>> GetStudentsPagedAsync(
        string? search = null,
        int? provinceId = null,
        int? priorityId = null,
        int pageIndex = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var query = db.SinhViens.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(sv => sv.MSSV.Contains(s) ||
                                     sv.HoTen.Contains(s) ||
                                     sv.CCCD.Contains(s) ||
                                     sv.Khoa.Contains(s));
        }

        if (provinceId.HasValue && provinceId > 0)
        {
            query = query.Where(sv => sv.TinhThanhId == provinceId.Value);
        }

        if (priorityId.HasValue && priorityId > 0)
        {
            query = query.Where(sv => sv.DienUuTienId == priorityId.Value);
        }

        int totalCount = await query.CountAsync(ct);

        var students = await query
            .OrderBy(sv => sv.MSSV)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(sv => new StudentDto
            {
                MSSV = sv.MSSV,
                HoTen = sv.HoTen,
                NgaySinh = sv.NgaySinh,
                GioiTinh = sv.GioiTinh,
                CCCD = sv.CCCD,
                Khoa = sv.Khoa,
                KhoaHoc = sv.KhoaHoc,
                SoDienThoai = sv.SoDienThoai,
                Email = sv.Email,
                TinhThanhId = sv.TinhThanhId,
                TenTinhThanh = sv.TinhThanh.Ten,
                KhoangCachKm = sv.TinhThanh.KhoangCachKm,
                DienUuTienId = sv.DienUuTienId,
                TenDienUuTien = sv.DienUuTien != null ? sv.DienUuTien.Ten : "Không",
                DiemCong = sv.DienUuTien != null ? sv.DienUuTien.DiemCong : 0,
                PhongHienTai = sv.HopDongs
                    .Where(h => h.TrangThai == TrangThaiHopDong.HieuLuc)
                    .Select(h => h.Phong.MaPhong)
                    .FirstOrDefault(),
                TrangThaiLuuTru = sv.HopDongs.Any(h => h.TrangThai == TrangThaiHopDong.HieuLuc) ? "Đang lưu trú" : "Chưa xếp phòng"
            })
            .ToListAsync(ct);

        return new PagedResult<StudentDto>
        {
            Items = students,
            TotalCount = totalCount,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    public async Task<StudentDto?> GetStudentByMssvAsync(string mssv, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        return await db.SinhViens.AsNoTracking()
            .Where(sv => sv.MSSV == mssv)
            .Select(sv => new StudentDto
            {
                MSSV = sv.MSSV,
                HoTen = sv.HoTen,
                NgaySinh = sv.NgaySinh,
                GioiTinh = sv.GioiTinh,
                CCCD = sv.CCCD,
                Khoa = sv.Khoa,
                KhoaHoc = sv.KhoaHoc,
                SoDienThoai = sv.SoDienThoai,
                Email = sv.Email,
                TinhThanhId = sv.TinhThanhId,
                TenTinhThanh = sv.TinhThanh.Ten,
                KhoangCachKm = sv.TinhThanh.KhoangCachKm,
                DienUuTienId = sv.DienUuTienId,
                TenDienUuTien = sv.DienUuTien != null ? sv.DienUuTien.Ten : "Không",
                DiemCong = sv.DienUuTien != null ? sv.DienUuTien.DiemCong : 0
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> CreateStudentAsync(StudentCreateUpdateDto dto, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        if (await db.SinhViens.AnyAsync(s => s.MSSV == dto.MSSV || s.CCCD == dto.CCCD, ct))
        {
            return false;
        }

        var sv = new SinhVien
        {
            MSSV = dto.MSSV.Trim(),
            HoTen = dto.HoTen.Trim(),
            NgaySinh = dto.NgaySinh,
            GioiTinh = dto.GioiTinh,
            CCCD = dto.CCCD.Trim(),
            Khoa = dto.Khoa.Trim(),
            KhoaHoc = dto.KhoaHoc,
            SoDienThoai = dto.SoDienThoai.Trim(),
            Email = dto.Email.Trim(),
            TinhThanhId = dto.TinhThanhId,
            DienUuTienId = dto.DienUuTienId
        };

        db.SinhViens.Add(sv);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> UpdateStudentAsync(StudentCreateUpdateDto dto, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var sv = await db.SinhViens.FirstOrDefaultAsync(s => s.MSSV == dto.MSSV, ct);
        if (sv == null) return false;

        sv.HoTen = dto.HoTen.Trim();
        sv.NgaySinh = dto.NgaySinh;
        sv.GioiTinh = dto.GioiTinh;
        sv.CCCD = dto.CCCD.Trim();
        sv.Khoa = dto.Khoa.Trim();
        sv.KhoaHoc = dto.KhoaHoc;
        sv.SoDienThoai = dto.SoDienThoai.Trim();
        sv.Email = dto.Email.Trim();
        sv.TinhThanhId = dto.TinhThanhId;
        sv.DienUuTienId = dto.DienUuTienId;

        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteStudentAsync(string mssv, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var sv = await db.SinhViens.Include(s => s.HopDongs).FirstOrDefaultAsync(s => s.MSSV == mssv, ct);
        if (sv == null) return false;

        if (sv.HopDongs.Any(h => h.TrangThai == TrangThaiHopDong.HieuLuc))
        {
            return false; // Đang có hợp đồng hiệu lực thì không xóa
        }

        db.SinhViens.Remove(sv);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<byte[]> ExportStudentsToExcelAsync(CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var students = await db.SinhViens.AsNoTracking()
            .Include(s => s.TinhThanh)
            .Include(s => s.DienUuTien)
            .OrderBy(s => s.MSSV)
            .ToListAsync(ct);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("DanhSachSinhVien");

        // Headers
        string[] headers = { "STT", "MSSV", "Họ và tên", "Ngày sinh", "Giới tính", "Số CCCD", "Khoa", "Khóa", "Số điện thoại", "Email", "Quê quán", "Diện ưu tiên" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2563EB");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // Data rows
        for (int r = 0; r < students.Count; r++)
        {
            var sv = students[r];
            int row = r + 2;
            ws.Cell(row, 1).Value = r + 1;
            ws.Cell(row, 2).Value = sv.MSSV;
            ws.Cell(row, 3).Value = sv.HoTen;
            ws.Cell(row, 4).Value = sv.NgaySinh.ToString("dd/MM/yyyy");
            ws.Cell(row, 5).Value = sv.GioiTinh == GioiTinh.Nam ? "Nam" : "Nữ";
            ws.Cell(row, 6).Value = sv.CCCD;
            ws.Cell(row, 7).Value = sv.Khoa;
            ws.Cell(row, 8).Value = sv.KhoaHoc;
            ws.Cell(row, 9).Value = sv.SoDienThoai;
            ws.Cell(row, 10).Value = sv.Email;
            ws.Cell(row, 11).Value = sv.TinhThanh?.Ten ?? "";
            ws.Cell(row, 12).Value = sv.DienUuTien?.Ten ?? "Không";
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> GenerateStudentTemplateAsync(CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var provinces = await db.TinhThanhs.AsNoTracking().OrderBy(t => t.Ten).Select(t => t.Ten).ToListAsync(ct);
        var priorities = await db.DienUuTiens.AsNoTracking().OrderBy(d => d.DiemCong).Select(d => d.Ten).ToListAsync(ct);

        using var workbook = new XLWorkbook();

        // Sheet 2: Danh mục Tỉnh thành (34 tỉnh mới)
        var wsProvinces = workbook.Worksheets.Add("DanhMuc_TinhThanh");
        for (int i = 0; i < provinces.Count; i++)
        {
            wsProvinces.Cell(i + 1, 1).Value = provinces[i];
        }

        // Sheet 3: Danh mục Diện ưu tiên
        var wsPriorities = workbook.Worksheets.Add("DanhMuc_DienUuTien");
        for (int i = 0; i < priorities.Count; i++)
        {
            wsPriorities.Cell(i + 1, 1).Value = priorities[i];
        }

        // Sheet 1: Mẫu nhập sinh viên
        var ws = workbook.Worksheets.Add("NhapSinhVien");

        string[] headers =
        {
            "MSSV (*)",
            "Họ và tên (*)",
            "Ngày sinh (dd/MM/yyyy) (*)",
            "Giới tính (*)",
            "Số CCCD (12 số) (*)",
            "Khoa (*)",
            "Khóa (số) (*)",
            "Số điện thoại",
            "Email",
            "Tỉnh thành (*)",
            "Diện ưu tiên"
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

        // Sample Row 1
        ws.Cell(2, 1).Value = "20261001";
        ws.Cell(2, 2).Value = "Trần Minh Quân";
        ws.Cell(2, 3).Value = "15/08/2005";
        ws.Cell(2, 4).Value = "Nam";
        ws.Cell(2, 5).Value = "001205012345";
        ws.Cell(2, 6).Value = "Công nghệ thông tin";
        ws.Cell(2, 7).Value = 67;
        ws.Cell(2, 8).Value = "0912345678";
        ws.Cell(2, 9).Value = "quan.tm261001@sis.edu.vn";
        ws.Cell(2, 10).Value = provinces.FirstOrDefault() ?? "Hà Nội";
        ws.Cell(2, 11).Value = priorities.FirstOrDefault() ?? "Con thương binh liệt sĩ";

        // Sample Row 2
        ws.Cell(3, 1).Value = "20261002";
        ws.Cell(3, 2).Value = "Lê Thị Thảo";
        ws.Cell(3, 3).Value = "22/11/2005";
        ws.Cell(3, 4).Value = "Nữ";
        ws.Cell(3, 5).Value = "034205098765";
        ws.Cell(3, 6).Value = "Kinh tế & Quản lý";
        ws.Cell(3, 7).Value = 67;
        ws.Cell(3, 8).Value = "0987654321";
        ws.Cell(3, 9).Value = "thao.lt261002@sis.edu.vn";
        ws.Cell(3, 10).Value = provinces.Skip(1).FirstOrDefault() ?? "Hải Phòng";
        ws.Cell(3, 11).Value = "";

        // Dropdown Data Validation for Giới tính
        var genderValidation = ws.Range("D2:D500").CreateDataValidation();
        genderValidation.List("Nam,Nữ", true);

        // Dropdown Data Validation for Tỉnh thành referencing sheet 2
        if (provinces.Count > 0)
        {
            var provValidation = ws.Range("J2:J500").CreateDataValidation();
            provValidation.List(wsProvinces.Range(1, 1, provinces.Count, 1), true);
        }

        // Dropdown Data Validation for Diện ưu tiên referencing sheet 3
        if (priorities.Count > 0)
        {
            var prioValidation = ws.Range("K2:K500").CreateDataValidation();
            prioValidation.List(wsPriorities.Range(1, 1, priorities.Count, 1), true);
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<ExcelImportResultDto> ImportStudentsFromExcelAsync(Stream fileStream, CancellationToken ct = default)
    {
        var result = new ExcelImportResultDto();

        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        // Load lookups
        var provinces = await db.TinhThanhs.AsNoTracking().ToListAsync(ct);
        var oldProvinces = await db.TinhThanhCus.AsNoTracking().ToListAsync(ct);
        var priorities = await db.DienUuTiens.AsNoTracking().ToListAsync(ct);

        var existingMssvSet = (await db.SinhViens.AsNoTracking().Select(s => s.MSSV).ToListAsync(ct))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var existingCccdSet = (await db.SinhViens.AsNoTracking().Select(s => s.CCCD).ToListAsync(ct))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Normalized province mapping: Name (lowercase trim) -> TinhThanhId
        var provinceMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in provinces)
        {
            provinceMap[p.Ten.Trim()] = p.Id;
            // Also normalize remove prefix "Tỉnh ", "Thành phố "
            var clean = p.Ten.Replace("Tỉnh ", "").Replace("Thành phố ", "").Trim();
            if (!provinceMap.ContainsKey(clean)) provinceMap[clean] = p.Id;
        }

        foreach (var old in oldProvinces)
        {
            if (!provinceMap.ContainsKey(old.TenCu.Trim()))
            {
                provinceMap[old.TenCu.Trim()] = old.TinhThanhId;
            }
            var clean = old.TenCu.Replace("Tỉnh ", "").Replace("Thành phố ", "").Trim();
            if (!provinceMap.ContainsKey(clean)) provinceMap[clean] = old.TinhThanhId;
        }

        // Priority lookup
        var priorityMap = priorities.ToDictionary(p => p.Ten.Trim(), p => p.Id, StringComparer.OrdinalIgnoreCase);

        using var workbook = new XLWorkbook(fileStream);
        var ws = workbook.Worksheet(1); // First worksheet

        var rowCount = ws.LastRowUsed()?.RowNumber() ?? 0;
        if (rowCount < 2)
        {
            result.Errors.Add(new ExcelImportRowError
            {
                RowIndex = 0,
                ErrorMessage = "File Excel không có dữ liệu để nhập."
            });
            return result;
        }

        var newStudents = new List<SinhVien>();
        var batchMssvSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var batchCccdSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int r = 2; r <= rowCount; r++)
        {
            var row = ws.Row(r);
            if (row.IsEmpty()) continue;

            string mssv = row.Cell(1).GetString().Trim();
            string hoTen = row.Cell(2).GetString().Trim();

            if (string.IsNullOrWhiteSpace(mssv) && string.IsNullOrWhiteSpace(hoTen))
            {
                continue; // Skip empty row
            }

            result.TotalRows++;
            var rowErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(mssv))
            {
                rowErrors.Add("MSSV không được để trống");
            }
            else if (existingMssvSet.Contains(mssv))
            {
                rowErrors.Add($"MSSV '{mssv}' đã tồn tại trong hệ thống");
            }
            else if (batchMssvSet.Contains(mssv))
            {
                rowErrors.Add($"MSSV '{mssv}' bị trùng lặp ngay trong file Excel");
            }

            if (string.IsNullOrWhiteSpace(hoTen))
            {
                rowErrors.Add("Họ tên không được để trống");
            }

            // Ngày sinh
            DateTime ngaySinh = DateTime.MinValue;
            var cellDob = row.Cell(3);
            if (cellDob.TryGetValue(out DateTime dt))
            {
                ngaySinh = dt;
            }
            else
            {
                var dobStr = cellDob.GetString().Trim();
                if (!DateTime.TryParseExact(dobStr, new[] { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd" }, null, System.Globalization.DateTimeStyles.None, out ngaySinh))
                {
                    rowErrors.Add($"Ngày sinh '{dobStr}' không hợp lệ (định dạng dd/MM/yyyy)");
                }
            }

            // Giới tính
            string gioiTinhStr = row.Cell(4).GetString().Trim().ToLowerInvariant();
            GioiTinh gioiTinh = GioiTinh.Nam;
            if (gioiTinhStr == "nam" || gioiTinhStr == "male" || gioiTinhStr == "m")
            {
                gioiTinh = GioiTinh.Nam;
            }
            else if (gioiTinhStr == "nữ" || gioiTinhStr == "nu" || gioiTinhStr == "female" || gioiTinhStr == "f")
            {
                gioiTinh = GioiTinh.Nu;
            }
            else
            {
                rowErrors.Add($"Giới tính '{row.Cell(4).GetString().Trim()}' không hợp lệ (phải là Nam hoặc Nữ)");
            }

            // CCCD
            string cccd = row.Cell(5).GetString().Trim();
            if (string.IsNullOrWhiteSpace(cccd))
            {
                rowErrors.Add("Số CCCD không được để trống");
            }
            else if (cccd.Length != 12 || !cccd.All(char.IsDigit))
            {
                rowErrors.Add($"Số CCCD '{cccd}' không hợp lệ (phải gồm đúng 12 chữ số)");
            }
            else if (existingCccdSet.Contains(cccd))
            {
                rowErrors.Add($"CCCD '{cccd}' đã tồn tại trong hệ thống");
            }
            else if (batchCccdSet.Contains(cccd))
            {
                rowErrors.Add($"CCCD '{cccd}' bị trùng lặp ngay trong file Excel");
            }

            // Khoa
            string khoa = row.Cell(6).GetString().Trim();
            if (string.IsNullOrWhiteSpace(khoa))
            {
                rowErrors.Add("Khoa viện không được để trống");
            }

            // Khóa học
            int khoaHoc = 0;
            var cellKhoaHoc = row.Cell(7);
            if (!cellKhoaHoc.TryGetValue(out khoaHoc) || khoaHoc <= 0)
            {
                if (!int.TryParse(cellKhoaHoc.GetString().Trim(), out khoaHoc) || khoaHoc <= 0)
                {
                    rowErrors.Add("Khóa học không hợp lệ (ví dụ: 67, 68)");
                }
            }

            string sdt = row.Cell(8).GetString().Trim();
            string email = row.Cell(9).GetString().Trim();

            // Tỉnh thành
            string tinhThanhStr = row.Cell(10).GetString().Trim();
            int tinhThanhId = 0;
            if (string.IsNullOrWhiteSpace(tinhThanhStr))
            {
                rowErrors.Add("Tỉnh thành quê quán không được để trống");
            }
            else
            {
                var cleanName = tinhThanhStr.Replace("Tỉnh ", "").Replace("Thành phố ", "").Trim();
                if (provinceMap.TryGetValue(tinhThanhStr, out int pid) || provinceMap.TryGetValue(cleanName, out pid))
                {
                    tinhThanhId = pid;
                }
                else
                {
                    rowErrors.Add($"Không nhận diện được tỉnh thành '{tinhThanhStr}' trong danh mục 34 tỉnh hoặc 63 tỉnh cũ");
                }
            }

            // Diện ưu tiên (optional)
            string dienUuTienStr = row.Cell(11).GetString().Trim();
            int? dienUuTienId = null;
            if (!string.IsNullOrWhiteSpace(dienUuTienStr))
            {
                if (priorityMap.TryGetValue(dienUuTienStr, out int prioId))
                {
                    dienUuTienId = prioId;
                }
                else
                {
                    rowErrors.Add($"Diện ưu tiên '{dienUuTienStr}' không tồn tại trong danh mục hệ thống");
                }
            }

            if (rowErrors.Count > 0)
            {
                result.FailedCount++;
                result.Errors.Add(new ExcelImportRowError
                {
                    RowIndex = r,
                    Identifier = !string.IsNullOrWhiteSpace(mssv) ? mssv : hoTen,
                    ErrorMessage = string.Join("; ", rowErrors)
                });
            }
            else
            {
                var student = new SinhVien
                {
                    MSSV = mssv,
                    HoTen = hoTen,
                    NgaySinh = ngaySinh,
                    GioiTinh = gioiTinh,
                    CCCD = cccd,
                    Khoa = khoa,
                    KhoaHoc = khoaHoc,
                    SoDienThoai = sdt,
                    Email = email,
                    TinhThanhId = tinhThanhId,
                    DienUuTienId = dienUuTienId
                };

                newStudents.Add(student);
                batchMssvSet.Add(mssv);
                batchCccdSet.Add(cccd);
                result.SuccessCount++;
            }
        }

        if (newStudents.Count > 0)
        {
            db.SinhViens.AddRange(newStudents);
            await db.SaveChangesAsync(ct);
        }

        return result;
    }
}

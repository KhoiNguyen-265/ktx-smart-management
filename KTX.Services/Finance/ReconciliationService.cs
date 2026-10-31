using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using KTX.Data;
using KTX.Domain.DTOs;
using KTX.Domain.Entities;
using KTX.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace KTX.Services.Finance;

public class ReconciliationService : IReconciliationService
{
    private readonly IDbContextFactory<KtxDbContext> _contextFactory;

    public ReconciliationService(IDbContextFactory<KtxDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public static string ComputeStatementHash(DateTime date, decimal amount, string content, decimal? balance)
    {
        var raw = $"{date:yyyyMMddHHmmss}|{amount:F0}|{content.Trim()}|{balance?.ToString("F0") ?? "0"}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes);
    }

    public async Task<ReconcileSummaryDto> ImportAndReconcileExcelAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        var summary = new ReconcileSummaryDto();

        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        // Nạp các giao dịch đã có trong DB để kiểm tra trùng 2 lớp
        var existingTxCodes = (await db.GiaoDichNganHangs.AsNoTracking()
            .Where(g => g.MaGiaoDich != null)
            .Select(g => g.MaGiaoDich!)
            .ToListAsync(ct)).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var existingHashes = (await db.GiaoDichNganHangs.AsNoTracking()
            .Select(g => g.HashSaoKe)
            .ToListAsync(ct)).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var batchTxCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var batchHashes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Nạp danh sách hóa đơn điện nước chưa thanh toán đủ
        var pendingInvoices = await db.HoaDonDienNuocs
            .Include(h => h.Phong)
            .Where(h => h.TrangThai != TrangThaiHoaDon.DaThanhToan)
            .ToListAsync(ct);

        // Nạp danh sách khoản thu tiền phòng chưa thanh toán đủ
        var pendingRoomFees = await db.KhoanThuTienPhongs
            .Include(k => k.HopDong).ThenInclude(h => h.SinhVien)
            .Where(k => k.TrangThai != TrangThaiHoaDon.DaThanhToan)
            .ToListAsync(ct);

        using var workbook = new XLWorkbook(fileStream);
        var ws = workbook.Worksheet(1);
        var rowCount = ws.LastRowUsed()?.RowNumber() ?? 0;

        var regex = new Regex(@"KTX\s*(DN|TP)\s*(\d+)", RegexOptions.IgnoreCase);

        for (int r = 2; r <= rowCount; r++)
        {
            var row = ws.Row(r);
            if (row.IsEmpty()) continue;

            summary.TotalRows++;

            // Cột 1: Ngày GD (dd/MM/yyyy HH:mm)
            DateTime ngayGd = DateTime.Now;
            if (row.Cell(1).TryGetValue(out DateTime dt)) ngayGd = dt;

            // Cột 2: Số tiền
            decimal soTien = 0;
            if (row.Cell(2).TryGetValue(out decimal amt)) soTien = amt;
            else decimal.TryParse(row.Cell(2).GetString().Trim(), out soTien);

            if (soTien <= 0) continue;

            // Cột 3: Nội dung chuyển khoản
            string noiDung = row.Cell(3).GetString().Trim();

            // Cột 4: Mã giao dịch ngân hàng (FT... / MB...)
            string maGiaoDich = row.Cell(4).GetString().Trim();
            if (string.IsNullOrWhiteSpace(maGiaoDich)) maGiaoDich = null!;

            // Cột 5: Số dư sau giao dịch
            decimal? soDuSau = null;
            if (row.Cell(5).TryGetValue(out decimal bal)) soDuSau = bal;

            // Băm Hash chống trùng lớp 2
            string hash = ComputeStatementHash(ngayGd, soTien, noiDung, soDuSau);

            // KIỂM TRA CHỐNG TRÙNG 2 LỚP:
            // 1) Trùng mã bút toán ngân hàng
            // 2) Trùng chuỗi băm HashSaoKe
            if ((!string.IsNullOrEmpty(maGiaoDich) && (existingTxCodes.Contains(maGiaoDich) || batchTxCodes.Contains(maGiaoDich))) ||
                existingHashes.Contains(hash) || batchHashes.Contains(hash))
            {
                summary.DuplicateCount++;
                continue; // Bỏ qua dòng trùng, không ghi nhận thêm lần 2!
            }

            if (!string.IsNullOrEmpty(maGiaoDich)) batchTxCodes.Add(maGiaoDich);
            batchHashes.Add(hash);
            summary.TotalAmount += soTien;

            // Tiến hành đối soát khớp mã
            var match = regex.Match(noiDung);
            TrangThaiGiaoDich trangThaiKhop = TrangThaiGiaoDich.CanXuLy;
            string? recognizedCode = null;
            string? matchDetail = null;

            HoaDonDienNuoc? matchedInvoice = null;
            KhoanThuTienPhong? matchedRoomFee = null;

            if (match.Success)
            {
                string prefix = match.Groups[1].Value.ToUpperInvariant(); // "DN" hoặc "TP"
                int number = int.Parse(match.Groups[2].Value);
                recognizedCode = $"KTX{prefix}{number:D6}";

                if (prefix == "DN")
                {
                    matchedInvoice = pendingInvoices.FirstOrDefault(i => i.Id == number || i.MaThanhToan.Equals(recognizedCode, StringComparison.OrdinalIgnoreCase));
                    if (matchedInvoice != null)
                    {
                        decimal remaining = matchedInvoice.TongTien - matchedInvoice.DaThu;
                        if (soTien >= remaining)
                        {
                            trangThaiKhop = TrangThaiGiaoDich.DaKhop;
                            matchDetail = $"Khớp đủ tiền HĐ điện nước {matchedInvoice.MaThanhToan} (Phòng {matchedInvoice.Phong.MaPhong})";
                        }
                        else
                        {
                            trangThaiKhop = TrangThaiGiaoDich.MotPhan;
                            matchDetail = $"Khớp một phần {soTien:N0}/{remaining:N0} đ (Phòng {matchedInvoice.Phong.MaPhong})";
                        }
                    }
                }
                else if (prefix == "TP")
                {
                    matchedRoomFee = pendingRoomFees.FirstOrDefault(k => k.Id == number || k.MaThanhToan.Equals(recognizedCode, StringComparison.OrdinalIgnoreCase));
                    if (matchedRoomFee != null)
                    {
                        decimal remaining = matchedRoomFee.PhaiThu - matchedRoomFee.DaThu;
                        if (soTien >= remaining)
                        {
                            trangThaiKhop = TrangThaiGiaoDich.DaKhop;
                            matchDetail = $"Khớp đủ tiền phòng {matchedRoomFee.MaThanhToan} (SV {matchedRoomFee.HopDong.SinhVien.HoTen})";
                        }
                        else
                        {
                            trangThaiKhop = TrangThaiGiaoDich.MotPhan;
                            matchDetail = $"Khớp một phần {soTien:N0}/{remaining:N0} đ (SV {matchedRoomFee.HopDong.SinhVien.HoTen})";
                        }
                    }
                }
            }

            var bankTx = new GiaoDichNganHang
            {
                NgayGD = ngayGd,
                SoTien = soTien,
                NoiDungGoc = noiDung,
                MaGiaoDich = maGiaoDich,
                SoDuSau = soDuSau,
                HashSaoKe = hash,
                TrangThaiKhop = trangThaiKhop,
                FileNguon = fileName,
                NgayImport = DateTime.Now
            };

            db.GiaoDichNganHangs.Add(bankTx);
            await db.SaveChangesAsync(ct);

            // Tự động tạo bản ghi ThanhToan và cập nhật công nợ
            if (matchedInvoice != null && (trangThaiKhop == TrangThaiGiaoDich.DaKhop || trangThaiKhop == TrangThaiGiaoDich.MotPhan))
            {
                matchedInvoice.DaThu += soTien;
                if (matchedInvoice.DaThu >= matchedInvoice.TongTien)
                {
                    matchedInvoice.TrangThai = TrangThaiHoaDon.DaThanhToan;
                    summary.MatchedCount++;
                }
                else
                {
                    matchedInvoice.TrangThai = TrangThaiHoaDon.ThanhToanMotPhan;
                    summary.PartialCount++;
                }

                db.ThanhToans.Add(new ThanhToan
                {
                    HoaDonDienNuocId = matchedInvoice.Id,
                    GiaoDichNganHangId = bankTx.Id,
                    SoTien = soTien,
                    HinhThuc = HinhThucThanhToan.ChuyenKhoan,
                    ThoiGian = ngayGd,
                    NguoiThuId = 1
                });
            }
            else if (matchedRoomFee != null && (trangThaiKhop == TrangThaiGiaoDich.DaKhop || trangThaiKhop == TrangThaiGiaoDich.MotPhan))
            {
                matchedRoomFee.DaThu += soTien;
                if (matchedRoomFee.DaThu >= matchedRoomFee.PhaiThu)
                {
                    matchedRoomFee.TrangThai = TrangThaiHoaDon.DaThanhToan;
                    summary.MatchedCount++;
                }
                else
                {
                    matchedRoomFee.TrangThai = TrangThaiHoaDon.ThanhToanMotPhan;
                    summary.PartialCount++;
                }

                db.ThanhToans.Add(new ThanhToan
                {
                    KhoanThuTienPhongId = matchedRoomFee.Id,
                    GiaoDichNganHangId = bankTx.Id,
                    SoTien = soTien,
                    HinhThuc = HinhThucThanhToan.ChuyenKhoan,
                    ThoiGian = ngayGd,
                    NguoiThuId = 1
                });
            }
            else
            {
                summary.UnmatchedCount++;
            }

            summary.Transactions.Add(new BankTransactionDto
            {
                Id = bankTx.Id,
                NgayGD = bankTx.NgayGD,
                SoTien = bankTx.SoTien,
                NoiDungGoc = bankTx.NoiDungGoc,
                MaGiaoDich = bankTx.MaGiaoDich,
                SoDuSau = bankTx.SoDuSau,
                HashSaoKe = bankTx.HashSaoKe,
                TrangThaiKhop = bankTx.TrangThaiKhop,
                MaHoaDonNhanDien = recognizedCode,
                ChiTietKhop = matchDetail
            });
        }

        await db.SaveChangesAsync(ct);
        return summary;
    }

    public async Task<List<BankTransactionDto>> GetTransactionsAsync(TrangThaiGiaoDich? filterStatus = null, CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var query = db.GiaoDichNganHangs.AsNoTracking().AsQueryable();
        if (filterStatus.HasValue)
        {
            query = query.Where(g => g.TrangThaiKhop == filterStatus.Value);
        }

        var list = await query.OrderByDescending(g => g.NgayGD).ToListAsync(ct);

        return list.Select(g => new BankTransactionDto
        {
            Id = g.Id,
            NgayGD = g.NgayGD,
            SoTien = g.SoTien,
            NoiDungGoc = g.NoiDungGoc,
            MaGiaoDich = g.MaGiaoDich,
            SoDuSau = g.SoDuSau,
            HashSaoKe = g.HashSaoKe,
            TrangThaiKhop = g.TrangThaiKhop
        }).ToList();
    }

    public async Task<byte[]> GenerateBankStatementSampleExcelAsync(CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var pendingInv = await db.HoaDonDienNuocs.Include(h => h.Phong).Take(3).ToListAsync(ct);
        var pendingFee = await db.KhoanThuTienPhongs.Take(3).ToListAsync(ct);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("SaoKeNganHang");

        string[] headers = { "Ngày giao dịch", "Số tiền (VNĐ)", "Nội dung giao dịch", "Mã giao dịch (Bút toán)", "Số dư sau GD" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#059669");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        int row = 2;
        decimal balance = 150000000m;

        // Mẫu 1: Khớp chuẩn HĐ điện nước
        if (pendingInv.Count > 0)
        {
            var inv = pendingInv[0];
            balance += inv.TongTien;
            ws.Cell(row, 1).Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            ws.Cell(row, 2).Value = inv.TongTien;
            ws.Cell(row, 3).Value = $"NGUYEN VAN A CK TIEN DIEN NUOC {inv.MaThanhToan} PHONG {inv.Phong.MaPhong}";
            ws.Cell(row, 4).Value = $"FT{DateTime.Now:yyyyMMdd}001";
            ws.Cell(row, 5).Value = balance;
            row++;
        }

        // Mẫu 2: Khớp chuẩn Tiền phòng KTXTP...
        if (pendingFee.Count > 0)
        {
            var fee = pendingFee[0];
            balance += fee.PhaiThu;
            ws.Cell(row, 1).Value = DateTime.Now.AddMinutes(-30).ToString("dd/MM/yyyy HH:mm");
            ws.Cell(row, 2).Value = fee.PhaiThu;
            ws.Cell(row, 3).Value = $"TRAN THI B NOP TIEN PHONG {fee.MaThanhToan}";
            ws.Cell(row, 4).Value = $"FT{DateTime.Now:yyyyMMdd}002";
            ws.Cell(row, 5).Value = balance;
            row++;
        }

        // Mẫu 3: Thanh toán một phần
        if (pendingInv.Count > 1)
        {
            var inv = pendingInv[1];
            decimal part = Math.Round(inv.TongTien / 2m, 0);
            balance += part;
            ws.Cell(row, 1).Value = DateTime.Now.AddHours(-1).ToString("dd/MM/yyyy HH:mm");
            ws.Cell(row, 2).Value = part;
            ws.Cell(row, 3).Value = $"PHONG {inv.Phong.MaPhong} DONG MOT PHAN TIEN DIEN {inv.MaThanhToan}";
            ws.Cell(row, 4).Value = $"FT{DateTime.Now:yyyyMMdd}003";
            ws.Cell(row, 5).Value = balance;
            row++;
        }

        // Mẫu 4: Giao dịch không có mã KTX (Cần xử lý)
        balance += 200000m;
        ws.Cell(row, 1).Value = DateTime.Now.AddHours(-2).ToString("dd/MM/yyyy HH:mm");
        ws.Cell(row, 2).Value = 200000m;
        ws.Cell(row, 3).Value = "NGUYEN VAN C CHUYEN TIEN CHO BAN KHONG RO NOI DUNG";
        ws.Cell(row, 4).Value = $"FT{DateTime.Now:yyyyMMdd}004";
        ws.Cell(row, 5).Value = balance;
        row++;

        // Mẫu 5: DÒNG TRÙNG LẶP với Mẫu 1 (để demo chống trùng)
        if (pendingInv.Count > 0)
        {
            var inv = pendingInv[0];
            ws.Cell(row, 1).Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            ws.Cell(row, 2).Value = inv.TongTien;
            ws.Cell(row, 3).Value = $"NGUYEN VAN A CK TIEN DIEN NUOC {inv.MaThanhToan} PHONG {inv.Phong.MaPhong}";
            ws.Cell(row, 4).Value = $"FT{DateTime.Now:yyyyMMdd}001"; // Trùng FT...
            ws.Cell(row, 5).Value = balance;
            row++;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }
}

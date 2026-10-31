using System.IO;
using ClosedXML.Excel;
using KTX.Data;
using KTX.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace KTX.Services.Reporting;

public class ReportingService : IReportingService
{
    private readonly IDbContextFactory<KtxDbContext> _contextFactory;

    public ReportingService(IDbContextFactory<KtxDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<byte[]> ExportOccupancyReportExcelAsync(CancellationToken ct = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        var contracts = await db.HopDongs.AsNoTracking()
            .Include(h => h.SinhVien).ThenInclude(s => s.TinhThanh)
            .Include(h => h.Phong).ThenInclude(p => p.ToaNha)
            .Include(h => h.Phong).ThenInclude(p => p.LoaiPhong)
            .Where(h => h.TrangThai == TrangThaiHopDong.HieuLuc)
            .OrderBy(h => h.Phong.ToaId).ThenBy(h => h.Phong.MaPhong).ThenBy(h => h.SinhVien.HoTen)
            .ToListAsync(ct);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("BaoCaoLuuTru");

        // 1. Title & Header
        ws.Cell("A1").Value = "HỆ THỐNG QUẢN LÝ KÝ TÚC XÁ THÔNG MINH";
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 13;

        ws.Cell("A2").Value = $"BÁO CÁO TÌNH HÌNH LƯU TRÚ SINH VIÊN (Xuất ngày: {DateTime.Now:dd/MM/yyyy HH:mm})";
        ws.Cell("A2").Style.Font.Bold = true;
        ws.Cell("A2").Style.Font.FontSize = 11;
        ws.Cell("A2").Style.Font.FontColor = XLColor.Navy;

        // 2. Table Headers
        string[] headers = new[]
        {
            "STT", "Tòa Nhà", "Mã Phòng", "Loại Phòng", "MSSV", "Họ và Tên",
            "Giới Tính", "Quê Quán", "Khóa", "Số Điện Thoại", "Ngày Bắt Đầu", "Ngày Hết Hạn"
        };

        int headerRow = 4;
        for (int c = 0; c < headers.Length; c++)
        {
            var cell = ws.Cell(headerRow, c + 1);
            cell.Value = headers[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2563EB");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        // 3. Rows
        int row = 5;
        for (int i = 0; i < contracts.Count; i++)
        {
            var h = contracts[i];
            ws.Cell(row, 1).Value = i + 1;
            ws.Cell(row, 2).Value = h.Phong.ToaNha.Ten;
            ws.Cell(row, 3).Value = h.Phong.MaPhong;
            ws.Cell(row, 4).Value = h.Phong.LoaiPhong.Ten;
            ws.Cell(row, 5).Value = h.MSSV;
            ws.Cell(row, 6).Value = h.SinhVien.HoTen;
            ws.Cell(row, 7).Value = h.SinhVien.GioiTinh == GioiTinh.Nam ? "Nam" : "Nữ";
            ws.Cell(row, 8).Value = h.SinhVien.TinhThanh?.Ten ?? "---";
            ws.Cell(row, 9).Value = $"K{h.SinhVien.KhoaHoc}";
            ws.Cell(row, 10).Value = h.SinhVien.SoDienThoai;
            ws.Cell(row, 11).Value = h.NgayBatDau.ToString("dd/MM/yyyy");
            ws.Cell(row, 12).Value = h.NgayKetThuc.ToString("dd/MM/yyyy");

            for (int col = 1; col <= headers.Length; col++)
            {
                ws.Cell(row, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
            row++;
        }

        // Summary footer
        ws.Cell(row + 1, 1).Value = $"Tổng cộng: {contracts.Count} sinh viên đang lưu trú hợp lệ.";
        ws.Cell(row + 1, 1).Style.Font.Bold = true;
        ws.Cell(row + 1, 1).Style.Font.Italic = true;

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportFinancialReportPdfAsync(int thang, int nam, CancellationToken ct = default)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        QuestPDF.Settings.UseSystemFonts = true;
        QuestPDF.Settings.ThrowOnMissingFontFamilies = false;

        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        // Nạp dữ liệu điện nước
        var utilityInvoices = await db.HoaDonDienNuocs.AsNoTracking()
            .Include(h => h.Phong).ThenInclude(p => p.ToaNha)
            .Where(h => h.Thang == thang && h.Nam == nam)
            .ToListAsync(ct);

        decimal totalUtilityPhaiThu = utilityInvoices.Sum(h => h.TongTien);
        decimal totalUtilityDaThu = utilityInvoices.Sum(h => h.DaThu);
        decimal totalUtilityConNo = Math.Max(0, totalUtilityPhaiThu - totalUtilityDaThu);

        // Nạp dữ liệu tiền phòng
        var roomFees = await db.KhoanThuTienPhongs.AsNoTracking()
            .Where(k => k.TuNgay.Month == thang && k.TuNgay.Year == nam)
            .ToListAsync(ct);

        decimal totalRoomPhaiThu = roomFees.Sum(k => k.PhaiThu);
        decimal totalRoomDaThu = roomFees.Sum(k => k.DaThu);
        decimal totalRoomConNo = Math.Max(0, totalRoomPhaiThu - totalRoomDaThu);

        // Nạp dữ liệu sự cố sửa chữa
        var maintenanceList = await db.YeuCauSuaChuas.AsNoTracking()
            .Where(y => y.NgayTao.Month == thang && y.NgayTao.Year == nam)
            .ToListAsync(ct);

        decimal totalMaintenanceCost = maintenanceList.Sum(y => y.ChiPhi);

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontFamily("Lato").FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("TRƯỜNG ĐẠI HỌC BÁCH KHOA").Bold().FontSize(10);
                            c.Item().Text("BAN QUẢN LÝ KÝ TÚC XÁ").Bold().FontSize(10);
                        });

                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM").Bold().FontSize(10).AlignRight();
                            c.Item().Text("Độc lập - Tự do - Hạnh phúc").Italic().FontSize(9).AlignRight();
                        });
                    });

                    col.Item().PaddingTop(15).AlignCenter().Text($"BÁO CÁO THU CHI & CÔNG NỢ KÝ TÚC XÁ").Bold().FontSize(15).FontColor("#1E3A8A");
                    col.Item().AlignCenter().Text($"Tháng {thang:D2}/{nam} (Lập ngày: {DateTime.Now:dd/MM/yyyy})").Italic().FontSize(10);
                    col.Item().PaddingTop(10).LineHorizontal(1).LineColor("#CBD5E1");
                });

                page.Content().PaddingTop(15).Column(col =>
                {
                    col.Spacing(12);

                    // 1. Bảng Doanh thu Tiền Điện Nước
                    col.Item().Text("1. TỔNG HỢP TIỀN ĐIỆN NƯỚC THEO PHÒNG").Bold().FontSize(11).FontColor("#2563EB");
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background("#F1F5F9").Padding(6).Text("Nội dung").Bold();
                            h.Cell().Background("#F1F5F9").Padding(6).AlignRight().Text("Số hóa đơn").Bold();
                            h.Cell().Background("#F1F5F9").Padding(6).AlignRight().Text("Phải thu (VNĐ)").Bold();
                            h.Cell().Background("#F1F5F9").Padding(6).AlignRight().Text("Đã thu (VNĐ)").Bold();
                            h.Cell().Background("#F1F5F9").Padding(6).AlignRight().Text("Còn nợ (VNĐ)").Bold();
                        });

                        table.Cell().BorderBottom(1).BorderColor("#E2E8F0").Padding(6).Text("Điện, Nước & Dịch vụ chung");
                        table.Cell().BorderBottom(1).BorderColor("#E2E8F0").Padding(6).AlignRight().Text($"{utilityInvoices.Count}");
                        table.Cell().BorderBottom(1).BorderColor("#E2E8F0").Padding(6).AlignRight().Text($"{totalUtilityPhaiThu:N0} đ");
                        table.Cell().BorderBottom(1).BorderColor("#E2E8F0").Padding(6).AlignRight().Text($"{totalUtilityDaThu:N0} đ");
                        table.Cell().BorderBottom(1).BorderColor("#E2E8F0").Padding(6).AlignRight().Text($"{totalUtilityConNo:N0} đ");
                    });

                    // 2. Bảng Tiền Phòng
                    col.Item().Text("2. TỔNG HỢP TIỀN PHÒNG SINH VIÊN").Bold().FontSize(11).FontColor("#2563EB");
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background("#F1F5F9").Padding(6).Text("Nội dung").Bold();
                            h.Cell().Background("#F1F5F9").Padding(6).AlignRight().Text("Số sinh viên").Bold();
                            h.Cell().Background("#F1F5F9").Padding(6).AlignRight().Text("Phải thu (VNĐ)").Bold();
                            h.Cell().Background("#F1F5F9").Padding(6).AlignRight().Text("Đã thu (VNĐ)").Bold();
                            h.Cell().Background("#F1F5F9").Padding(6).AlignRight().Text("Còn nợ (VNĐ)").Bold();
                        });

                        table.Cell().BorderBottom(1).BorderColor("#E2E8F0").Padding(6).Text("Tiền phòng theo Hợp đồng");
                        table.Cell().BorderBottom(1).BorderColor("#E2E8F0").Padding(6).AlignRight().Text($"{roomFees.Count}");
                        table.Cell().BorderBottom(1).BorderColor("#E2E8F0").Padding(6).AlignRight().Text($"{totalRoomPhaiThu:N0} đ");
                        table.Cell().BorderBottom(1).BorderColor("#E2E8F0").Padding(6).AlignRight().Text($"{totalRoomDaThu:N0} đ");
                        table.Cell().BorderBottom(1).BorderColor("#E2E8F0").Padding(6).AlignRight().Text($"{totalRoomConNo:N0} đ");
                    });

                    // 3. Chi phí bảo trì
                    col.Item().Text("3. CHI PHÍ BẢO TRÌ & SỬA CHỮA THIẾT BỊ PHÒNG").Bold().FontSize(11).FontColor("#2563EB");
                    col.Item().Text($"• Tổng số sự cố ghi nhận trong tháng: {maintenanceList.Count} vụ việc");
                    col.Item().Text($"• Tổng chi phí vật tư đã thanh toán: {totalMaintenanceCost:N0} đ");

                    // 4. Tổng kết
                    decimal totalThu = totalUtilityDaThu + totalRoomDaThu;
                    col.Item().PaddingTop(10).Background("#EFF6FF").Padding(10).Column(box =>
                    {
                        box.Item().Text($"TỔNG KẾT DOANH THU THỰC THU: {totalThu:N0} VNĐ").Bold().FontSize(12).FontColor("#1E3A8A");
                        box.Item().Text($"TỔNG CÔNG NỢ CHƯA THU: {(totalUtilityConNo + totalRoomConNo):N0} VNĐ").Bold().FontSize(10).FontColor("#B91C1C");
                    });

                    // Signatures
                    col.Item().PaddingTop(30).Row(row =>
                    {
                        row.RelativeItem().AlignCenter().Column(c =>
                        {
                            c.Item().Text("KẾ TOÁN TRƯỞNG").Bold();
                            c.Item().Text("(Ký và ghi rõ họ tên)").Italic().FontSize(8.5f);
                        });

                        row.RelativeItem().AlignCenter().Column(c =>
                        {
                            c.Item().Text("TRƯỞNG BAN QUẢN LÝ KTX").Bold();
                            c.Item().Text("(Ký, đóng dấu và ghi rõ họ tên)").Italic().FontSize(8.5f);
                        });
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Trang ");
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        });

        return doc.GeneratePdf();
    }
}

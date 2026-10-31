using System.Drawing;
using System.IO;
using System.Windows.Forms;
using KTX.Services.Reporting;
using KTX.UI.Controls;
using KTX.UI.Helpers;
using KTX.UI.Themes;

namespace KTX.WinForms.Views;

public class ReportsView : UserControl, IThemable
{
    private readonly IReportingService _reportingService;

    private readonly KCard _cardOccupancy;
    private readonly KButton _btnExportOccupancy;

    private readonly KCard _cardFinance;
    private readonly ComboBox _cboMonth;
    private readonly ComboBox _cboYear;
    private readonly KButton _btnExportFinance;

    public ReportsView(IReportingService reportingService)
    {
        _reportingService = reportingService;

        Dock = DockStyle.Fill;
        DoubleBuffered = true;
        AutoScroll = true;
        Padding = new Padding(16);

        // 1. Occupancy Excel Report Card
        _cardOccupancy = new KCard
        {
            Title = "Báo cáo Tình trạng Lưu trú & Danh sách Sinh viên",
            Subtitle = "Xuất file ClosedXML Excel chuẩn mẫu quản lý KTX, thống kê chi tiết theo từng Tòa, Phòng và Hợp đồng",
            Dock = DockStyle.Top,
            Height = 160,
            Padding = new Padding(16, 40, 16, 16)
        };

        _btnExportOccupancy = new KButton
        {
            Text = "Xuất Báo Cáo Lưu Trú (.xlsx)",
            Style = KButtonStyle.Primary,
            Location = new Point(20, 25),
            Size = new Size(250, 42)
        };
        _btnExportOccupancy.Click += async (s, e) => await ExportOccupancyAsync();

        var pnlOcc = new Panel { Dock = DockStyle.Fill };
        pnlOcc.Controls.Add(_btnExportOccupancy);
        _cardOccupancy.Controls.Add(pnlOcc);

        // Spacer
        var spacer = new Panel { Dock = DockStyle.Top, Height = 16 };

        // 2. Financial PDF Report Card
        _cardFinance = new KCard
        {
            Title = "Báo cáo Tài chính Tháng, Công Nợ & Bảo trì Thiết bị",
            Subtitle = "Xuất file QuestPDF chuẩn văn bản hành chính: thống kê thu chi tiền phòng, điện nước và chi phí sửa chữa",
            Dock = DockStyle.Top,
            Height = 180,
            Padding = new Padding(16, 40, 16, 16)
        };

        var pnlFin = new Panel { Dock = DockStyle.Fill };

        var lblMonth = new Label { Text = "Chọn Tháng:", Location = new Point(20, 25), AutoSize = true };
        _cboMonth = new ComboBox { Location = new Point(105, 22), Width = 70, DropDownStyle = ComboBoxStyle.DropDownList };
        for (int m = 1; m <= 12; m++) _cboMonth.Items.Add(m);
        _cboMonth.SelectedItem = DateTime.Now.Month;

        var lblYear = new Label { Text = "Năm:", Location = new Point(190, 25), AutoSize = true };
        _cboYear = new ComboBox { Location = new Point(230, 22), Width = 90, DropDownStyle = ComboBoxStyle.DropDownList };
        for (int y = 2024; y <= 2028; y++) _cboYear.Items.Add(y);
        _cboYear.SelectedItem = DateTime.Now.Year;

        _btnExportFinance = new KButton
        {
            Text = "Xuất Báo Cáo Tài Chính PDF (.pdf)",
            Style = KButtonStyle.Primary,
            Location = new Point(340, 18),
            Size = new Size(270, 42)
        };
        _btnExportFinance.Click += async (s, e) => await ExportFinanceAsync();

        pnlFin.Controls.AddRange(new Control[] { lblMonth, _cboMonth, lblYear, _cboYear, _btnExportFinance });
        _cardFinance.Controls.Add(pnlFin);

        // Layout
        Controls.Add(_cardFinance);
        Controls.Add(spacer);
        Controls.Add(_cardOccupancy);

        ApplyTheme(ThemeManager.Current);
    }

    private async Task ExportOccupancyAsync()
    {
        using var sfd = new SaveFileDialog
        {
            Filter = "Excel Workbook (*.xlsx)|*.xlsx",
            FileName = $"BaoCao_LuuTru_KTX_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
            Title = "Lưu Báo cáo Lưu trú KTX (.xlsx)"
        };

        if (sfd.ShowDialog() != DialogResult.OK) return;

        try
        {
            byte[] bytes = await _reportingService.ExportOccupancyReportExcelAsync();
            await File.WriteAllBytesAsync(sfd.FileName, bytes);

            MessageBox.Show($"Xuất báo cáo lưu trú thành công!\nFile đã được lưu tại:\n{sfd.FileName}",
                "Xuất file thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi xuất file Excel: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task ExportFinanceAsync()
    {
        int month = (int)_cboMonth.SelectedItem!;
        int year = (int)_cboYear.SelectedItem!;

        using var sfd = new SaveFileDialog
        {
            Filter = "PDF Document (*.pdf)|*.pdf",
            FileName = $"BaoCao_TaiChinh_Thang_{month:D2}_{year}.pdf",
            Title = "Lưu Báo cáo Tài chính KTX (.pdf)"
        };

        if (sfd.ShowDialog() != DialogResult.OK) return;

        try
        {
            byte[] bytes = await _reportingService.ExportFinancialReportPdfAsync(month, year);
            await File.WriteAllBytesAsync(sfd.FileName, bytes);

            MessageBox.Show($"Xuất báo cáo tài chính PDF thành công!\nFile đã được lưu tại:\n{sfd.FileName}",
                "Xuất file thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi xuất file PDF: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public void ApplyTheme(Theme theme)
    {
        BackColor = theme.Background;
    }
}

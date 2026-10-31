using System.Drawing;
using System.IO;
using System.Windows.Forms;
using KTX.Domain.DTOs;
using KTX.Domain.Enums;
using KTX.Services.Finance;
using KTX.UI.Controls;
using KTX.UI.Helpers;
using KTX.UI.Themes;

namespace KTX.WinForms.Views;

public class ReconciliationView : UserControl, IThemable
{
    private readonly IReconciliationService _reconciliationService;

    private readonly KButton _btnImport;
    private readonly KButton _btnDownloadSample;
    private readonly KButton _btnRefresh;

    private readonly Panel _panelStats;
    private readonly Label _lblStatTotal;
    private readonly Label _lblStatMatched;
    private readonly Label _lblStatPartial;
    private readonly Label _lblStatUnmatched;
    private readonly Label _lblDuplicateBanner;

    private readonly TabControl _tabMain;
    private readonly TabPage _tabMatched;
    private readonly TabPage _tabPartial;
    private readonly TabPage _tabUnmatched;

    private readonly DataGridView _gridMatched;
    private readonly DataGridView _gridPartial;
    private readonly DataGridView _gridUnmatched;

    private List<BankTransactionDto> _allTransactions = new();

    public ReconciliationView(IReconciliationService reconciliationService)
    {
        _reconciliationService = reconciliationService;

        Dock = DockStyle.Fill;
        DoubleBuffered = true;

        // 1. Top action bar
        var topBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(0, 10, 0, 10)
        };

        _btnImport = new KButton
        {
            Text = "Nhập file sao kê Excel",
            Style = KButtonStyle.Primary,
            Location = new Point(0, 10),
            Size = new Size(185, 38)
        };
        _btnImport.Click += async (s, e) => await ImportExcelAsync();

        _btnDownloadSample = new KButton
        {
            Text = "Tải file mẫu sao kê",
            Style = KButtonStyle.Secondary,
            Location = new Point(195, 10),
            Size = new Size(170, 38)
        };
        _btnDownloadSample.Click += async (s, e) => await DownloadSampleExcelAsync();

        _btnRefresh = new KButton
        {
            Text = "Làm mới",
            Style = KButtonStyle.Secondary,
            Location = new Point(375, 10),
            Size = new Size(100, 38)
        };
        _btnRefresh.Click += async (s, e) => await LoadTransactionsAsync();

        topBar.Controls.AddRange(new Control[] { _btnImport, _btnDownloadSample, _btnRefresh });

        // 2. Stats & Duplicate Banner
        _panelStats = new Panel
        {
            Dock = DockStyle.Top,
            Height = 65,
            Padding = new Padding(0, 4, 0, 8)
        };

        _lblStatTotal = new Label
        {
            Text = "Tổng GD: 0 | 0 đ",
            Font = UIHelper.GetFont(9.5f, FontStyle.Bold),
            Location = new Point(0, 6),
            AutoSize = true
        };

        _lblStatMatched = new Label
        {
            Text = "Đã khớp: 0",
            ForeColor = Color.FromArgb(46, 125, 50),
            Font = UIHelper.GetFont(9.5f, FontStyle.Bold),
            Location = new Point(250, 6),
            AutoSize = true
        };

        _lblStatPartial = new Label
        {
            Text = "Khớp một phần: 0",
            ForeColor = Color.FromArgb(230, 81, 0),
            Font = UIHelper.GetFont(9.5f, FontStyle.Bold),
            Location = new Point(390, 6),
            AutoSize = true
        };

        _lblStatUnmatched = new Label
        {
            Text = "Cần xử lý: 0",
            ForeColor = Color.FromArgb(198, 40, 40),
            Font = UIHelper.GetFont(9.5f, FontStyle.Bold),
            Location = new Point(560, 6),
            AutoSize = true
        };

        _lblDuplicateBanner = new Label
        {
            Text = "Cơ chế chống trùng 2 lớp: Mã giao dịch + Chuỗi băm SHA256",
            ForeColor = Color.FromArgb(30, 136, 229),
            Font = UIHelper.GetFont(9f, FontStyle.Regular),
            Location = new Point(0, 34),
            AutoSize = true
        };

        _panelStats.Controls.AddRange(new Control[] { _lblStatTotal, _lblStatMatched, _lblStatPartial, _lblStatUnmatched, _lblDuplicateBanner });

        // 3. TabControl for 3 statuses
        _tabMain = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = UIHelper.GetFont(9.5f, FontStyle.Regular)
        };

        _tabMatched = new TabPage("Đã khớp hoàn toàn");
        _tabPartial = new TabPage("Khớp một phần");
        _tabUnmatched = new TabPage("Cần kế toán xử lý");

        _gridMatched = CreateGrid();
        _gridPartial = CreateGrid();
        _gridUnmatched = CreateGrid();

        _tabMatched.Controls.Add(_gridMatched);
        _tabPartial.Controls.Add(_gridPartial);
        _tabUnmatched.Controls.Add(_gridUnmatched);

        _tabMain.TabPages.AddRange(new[] { _tabMatched, _tabPartial, _tabUnmatched });

        // Layout
        Controls.Add(_tabMain);
        Controls.Add(_panelStats);
        Controls.Add(topBar);

        ApplyTheme(ThemeManager.Current);

        Load += async (s, e) => await LoadTransactionsAsync();
    }

    protected override async void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        if (_allTransactions.Count == 0)
        {
            await LoadTransactionsAsync();
        }
    }

    private DataGridView CreateGrid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        KDataGridHelper.ApplyModernStyle(grid);

        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = "Id", Width = 55 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ngày GD", DataPropertyName = "NgayGD", Width = 130 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Số tiền (VNĐ)", DataPropertyName = "SoTien", Width = 115 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mã GD Ngân hàng", DataPropertyName = "MaGiaoDich", Width = 140 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nội dung chuyển khoản", DataPropertyName = "NoiDungGoc", Width = 230 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Số dư sau", DataPropertyName = "SoDuSau", Width = 110 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", DataPropertyName = "TrangThaiText", Width = 110 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mã HĐ nhận diện", DataPropertyName = "MaHoaDonNhanDien", Width = 135 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Chi tiết đối soát", DataPropertyName = "ChiTietKhop", Width = 200 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Hash SHA256", DataPropertyName = "HashSaoKe", Width = 160 });

        return grid;
    }

    private async Task LoadTransactionsAsync()
    {
        try
        {
            _allTransactions = await _reconciliationService.GetTransactionsAsync();
            PopulateGrids();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể nạp dữ liệu sao kê: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void PopulateGrids()
    {
        var matched = _allTransactions.Where(t => t.TrangThaiKhop == TrangThaiGiaoDich.DaKhop).ToList();
        var partial = _allTransactions.Where(t => t.TrangThaiKhop == TrangThaiGiaoDich.MotPhan).ToList();
        var unmatched = _allTransactions.Where(t => t.TrangThaiKhop == TrangThaiGiaoDich.CanXuLy).ToList();

        decimal totalAmount = _allTransactions.Sum(t => t.SoTien);

        _lblStatTotal.Text = $"Tổng GD: {_allTransactions.Count:N0} dòng | {totalAmount:N0} đ";
        _lblStatMatched.Text = $"Đã khớp: {matched.Count:N0}";
        _lblStatPartial.Text = $"Khớp một phần: {partial.Count:N0}";
        _lblStatUnmatched.Text = $"Cần xử lý: {unmatched.Count:N0}";

        _tabMatched.Text = $"Đã khớp hoàn toàn ({matched.Count})";
        _tabPartial.Text = $"Khớp một phần ({partial.Count})";
        _tabUnmatched.Text = $"Cần xử lý ({unmatched.Count})";

        BindGrid(_gridMatched, matched);
        BindGrid(_gridPartial, partial);
        BindGrid(_gridUnmatched, unmatched);
    }

    private void BindGrid(DataGridView grid, List<BankTransactionDto> list)
    {
        grid.Rows.Clear();
        foreach (var t in list)
        {
            string statusText = t.TrangThaiKhop switch
            {
                TrangThaiGiaoDich.DaKhop => "Đã khớp",
                TrangThaiGiaoDich.MotPhan => "Khớp 1 phần",
                _ => "Cần xử lý"
            };

            grid.Rows.Add(
                t.Id,
                t.NgayGD.ToString("dd/MM/yyyy HH:mm"),
                $"{t.SoTien:N0} đ",
                t.MaGiaoDich ?? "(Không có)",
                t.NoiDungGoc,
                t.SoDuSau.HasValue ? $"{t.SoDuSau.Value:N0} đ" : "---",
                statusText,
                t.MaHoaDonNhanDien ?? "---",
                t.ChiTietKhop ?? "---",
                t.HashSaoKe.Length > 16 ? t.HashSaoKe.Substring(0, 16) + "..." : t.HashSaoKe
            );
        }
    }

    private async Task ImportExcelAsync()
    {
        using var ofd = new OpenFileDialog
        {
            Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
            Title = "Chọn file sao kê ngân hàng (.xlsx)"
        };

        if (ofd.ShowDialog() != DialogResult.OK) return;

        try
        {
            await using var fs = File.OpenRead(ofd.FileName);
            var result = await _reconciliationService.ImportAndReconcileExcelAsync(fs, Path.GetFileName(ofd.FileName));

            _lblDuplicateBanner.Text = $"Kết quả Import: Tổng {result.TotalRows} dòng | Khớp đủ: {result.MatchedCount} | Một phần: {result.PartialCount} | Cần xử lý: {result.UnmatchedCount} | Bỏ qua trùng: {result.DuplicateCount} dòng";
            _lblDuplicateBanner.ForeColor = result.DuplicateCount > 0 ? Color.FromArgb(239, 108, 0) : Color.FromArgb(46, 125, 50);

            MessageBox.Show(
                $"Đối soát sao kê ngân hàng thành công!\n\n" +
                $"• Tổng số dòng: {result.TotalRows}\n" +
                $"• Khớp đủ hoàn toàn: {result.MatchedCount}\n" +
                $"• Khớp một phần: {result.PartialCount}\n" +
                $"• Cần xử lý bổ sung: {result.UnmatchedCount}\n" +
                $"• Bỏ qua dòng trùng lặp (chống trùng 2 lớp): {result.DuplicateCount}\n" +
                $"• Tổng số tiền ghi nhận: {result.TotalAmount:N0} đ",
                "Hoàn thành đối soát",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            await LoadTransactionsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi trong quá trình đối soát sao kê: {ex.Message}", "Lỗi Import", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task DownloadSampleExcelAsync()
    {
        using var sfd = new SaveFileDialog
        {
            Filter = "Excel Files (*.xlsx)|*.xlsx",
            FileName = $"Mau_Sao_Ke_Ngan_Hang_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
            Title = "Lưu file mẫu sao kê ngân hàng"
        };

        if (sfd.ShowDialog() != DialogResult.OK) return;

        try
        {
            byte[] bytes = await _reconciliationService.GenerateBankStatementSampleExcelAsync();
            await File.WriteAllBytesAsync(sfd.FileName, bytes);

            MessageBox.Show($"Đã tạo file mẫu sao kê ngân hàng tại:\n{sfd.FileName}\n\nFile mẫu chứa sẵn các giao dịch chuyển khoản VietQR khớp với các hóa đơn thực tế trong hệ thống.",
                "Tải file mẫu thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải file mẫu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public void ApplyTheme(Theme theme)
    {
        BackColor = theme.Background;
        _panelStats.BackColor = theme.Surface;
        _tabMain.BackColor = theme.Surface;
        _tabMatched.BackColor = theme.Background;
        _tabPartial.BackColor = theme.Background;
        _tabUnmatched.BackColor = theme.Background;

        _lblStatTotal.ForeColor = theme.TextPrimary;

        KDataGridHelper.ApplyModernStyle(_gridMatched);
        KDataGridHelper.ApplyModernStyle(_gridPartial);
        KDataGridHelper.ApplyModernStyle(_gridUnmatched);
    }
}

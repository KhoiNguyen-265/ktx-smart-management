using System.Globalization;
using KTX.Domain.DTOs;
using KTX.Services.Configuration;
using KTX.UI.Controls;
using KTX.UI.Helpers;
using KTX.UI.Themes;

namespace KTX.WinForms.Views;

public class SettingsView : UserControl
{
    private readonly IConfigurationService _configService;

    private TabControl _tabMain = null!;
    private TabPage _tabWeights = null!;
    private TabPage _tabBank = null!;
    private TabPage _tabPriorities = null!;
    private TabPage _tabProvinces = null!;

    // Weights controls
    private NumericUpDown _nudWPrio = null!;
    private NumericUpDown _nudWDist = null!;
    private NumericUpDown _nudWCourse = null!;
    private NumericUpDown _nudWPref = null!;
    private NumericUpDown _nudWRoommate = null!;
    private Label _lblWeightTotal = null!;

    // Bank controls
    private KTextBox _txtBankBin = null!;
    private KTextBox _txtBankName = null!;
    private KTextBox _txtAccountNum = null!;
    private KTextBox _txtAccountHolder = null!;
    private NumericUpDown _nudVat = null!;

    // Priorities
    private DataGridView _gridPriorities = null!;
    private KTextBox _txtNewPrioName = null!;
    private NumericUpDown _nudNewPrioPoints = null!;

    // Provinces
    private DataGridView _gridProvinces = null!;

    public SettingsView(IConfigurationService configService)
    {
        _configService = configService;
        InitializeComponents();
        ThemeManager.ThemeChanged += OnThemeChanged;
    }

    private void InitializeComponents()
    {
        this.Dock = DockStyle.Fill;
        this.BackColor = ThemeManager.Current.Background;
        this.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

        // Header
        var headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(20, 15, 20, 0)
        };
        var lblTitle = new Label
        {
            Text = "CẤU HÌNH HỆ THỐNG & TRỌNG SỐ THUẬT TOÁN",
            Font = new Font("Segoe UI", 15F, FontStyle.Bold),
            ForeColor = ThemeManager.Current.TextPrimary,
            AutoSize = true
        };
        headerPanel.Controls.Add(lblTitle);

        _tabMain = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10.5F, FontStyle.Regular)
        };

        _tabWeights = new TabPage("Trọng số Xếp phòng (AI)");
        _tabBank = new TabPage("Tài chính & VietQR");
        _tabPriorities = new TabPage("Diện ưu tiên");
        _tabProvinces = new TabPage("Danh mục 34 Tỉnh thành");

        _tabMain.TabPages.Add(_tabWeights);
        _tabMain.TabPages.Add(_tabBank);
        _tabMain.TabPages.Add(_tabPriorities);
        _tabMain.TabPages.Add(_tabProvinces);

        BuildWeightsTab();
        BuildBankTab();
        BuildPrioritiesTab();
        BuildProvincesTab();

        this.Controls.Add(_tabMain);
        this.Controls.Add(headerPanel);

        this.Load += async (s, e) => await LoadDataAsync();
    }

    protected override async void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        await LoadDataAsync();
    }

    private void BuildWeightsTab()
    {
        var card = new KCard
        {
            Title = "Thiết lập trọng số cho hàm mục tiêu tối ưu (Objective Function)",
            Dock = DockStyle.Fill,
            Padding = new Padding(24)
        };

        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 320,
            ColumnCount = 2,
            RowCount = 7,
            Padding = new Padding(10)
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

        _nudWPrio = CreateNumericInput(0.35m);
        _nudWDist = CreateNumericInput(0.25m);
        _nudWCourse = CreateNumericInput(0.15m);
        _nudWPref = CreateNumericInput(0.15m);
        _nudWRoommate = CreateNumericInput(0.10m);

        _nudWPrio.ValueChanged += (s, e) => UpdateWeightSum();
        _nudWDist.ValueChanged += (s, e) => UpdateWeightSum();
        _nudWCourse.ValueChanged += (s, e) => UpdateWeightSum();
        _nudWPref.ValueChanged += (s, e) => UpdateWeightSum();
        _nudWRoommate.ValueChanged += (s, e) => UpdateWeightSum();

        AddWeightRow(panel, 0, "1. Trọng số Diện ưu tiên (w_dien):", _nudWPrio);
        AddWeightRow(panel, 1, "2. Trọng số Khoảng cách quê quán (w_khoangcach):", _nudWDist);
        AddWeightRow(panel, 2, "3. Trọng số Khóa học - ưu tiên SV mới vào (w_khoahoc):", _nudWCourse);
        AddWeightRow(panel, 3, "4. Trọng số Nguyện vọng loại phòng/tầng (w_nguyenvong):", _nudWPref);
        AddWeightRow(panel, 4, "5. Trọng số Giữ nhóm bạn cùng phòng (w_nhom):", _nudWRoommate);

        _lblWeightTotal = new Label
        {
            Text = "Tổng trọng số: 1.00 (100%) ✓",
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = ThemeManager.Current.Success,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        panel.Controls.Add(_lblWeightTotal, 0, 5);
        panel.SetColumnSpan(_lblWeightTotal, 2);

        var btnSaveWeights = new KButton
        {
            Text = "Lưu trọng số xếp phòng",
            Style = KButtonStyle.Primary,
            Size = new Size(200, 40)
        };
        btnSaveWeights.Click += async (s, e) => await SaveWeightsAsync();

        card.Controls.Add(btnSaveWeights);
        btnSaveWeights.Location = new Point(20, 360);
        card.Controls.Add(panel);
        _tabWeights.Controls.Add(card);
    }

    private NumericUpDown CreateNumericInput(decimal defaultVal)
    {
        return new NumericUpDown
        {
            DecimalPlaces = 2,
            Increment = 0.05m,
            Minimum = 0.0m,
            Maximum = 1.0m,
            Value = defaultVal,
            Width = 120,
            Font = new Font("Segoe UI", 10F)
        };
    }

    private void AddWeightRow(TableLayoutPanel panel, int row, string label, Control input)
    {
        var lbl = new Label
        {
            Text = label,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = ThemeManager.Current.TextPrimary
        };
        panel.Controls.Add(lbl, 0, row);
        panel.Controls.Add(input, 1, row);
    }

    private void UpdateWeightSum()
    {
        decimal sum = _nudWPrio.Value + _nudWDist.Value + _nudWCourse.Value + _nudWPref.Value + _nudWRoommate.Value;
        if (Math.Abs(sum - 1.0m) <= 0.01m)
        {
            _lblWeightTotal.Text = $"Tổng trọng số: {sum:F2} (100%) - Hợp lệ";
            _lblWeightTotal.ForeColor = ThemeManager.Current.Success;
        }
        else
        {
            _lblWeightTotal.Text = $"Tổng trọng số: {sum:F2} (Cảnh báo: Tổng các trọng số nên bằng 1.00)";
            _lblWeightTotal.ForeColor = ThemeManager.Current.Warning;
        }
    }

    private void BuildBankTab()
    {
        var card = new KCard
        {
            Title = "Cấu hình Ngân hàng thụ hưởng & Sinh mã VietQR tĩnh/động",
            Dock = DockStyle.Fill,
            Padding = new Padding(24)
        };

        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 320,
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(10)
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _txtBankName = new KTextBox { Placeholder = "Ví dụ: VietinBank", Width = 300 };
        _txtBankBin = new KTextBox { Placeholder = "Mã BIN chuẩn NAPAS (970415...)", Width = 300 };
        _txtAccountNum = new KTextBox { Placeholder = "Số tài khoản ngân hàng", Width = 300 };
        _txtAccountHolder = new KTextBox { Placeholder = "Tên chủ tài khoản (in hoa không dấu)", Width = 300 };

        _nudVat = new NumericUpDown
        {
            DecimalPlaces = 2,
            Increment = 0.01m,
            Minimum = 0m,
            Maximum = 0.2m,
            Value = 0.08m,
            Width = 120
        };

        AddFormRow(panel, 0, "Ngân hàng:", _txtBankName);
        AddFormRow(panel, 1, "Mã BIN VietQR (NAPAS):", _txtBankBin);
        AddFormRow(panel, 2, "Số tài khoản nhận tiền:", _txtAccountNum);
        AddFormRow(panel, 3, "Chủ tài khoản thụ hưởng:", _txtAccountHolder);
        AddFormRow(panel, 4, "Thuế suất GTGT (VAT):", _nudVat);

        var btnSaveBank = new KButton
        {
            Text = "Lưu cấu hình ngân hàng & VAT",
            Style = KButtonStyle.Primary,
            Size = new Size(240, 40)
        };
        btnSaveBank.Click += async (s, e) => await SaveBankAsync();

        card.Controls.Add(btnSaveBank);
        btnSaveBank.Location = new Point(20, 360);
        card.Controls.Add(panel);
        _tabBank.Controls.Add(card);
    }

    private void AddFormRow(TableLayoutPanel panel, int row, string label, Control input)
    {
        var lbl = new Label
        {
            Text = label,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = ThemeManager.Current.TextPrimary
        };
        panel.Controls.Add(lbl, 0, row);
        panel.Controls.Add(input, 1, row);
    }

    private void BuildPrioritiesTab()
    {
        var card = new KCard
        {
            Title = "Danh mục Diện chính sách & Điểm cộng xét duyệt",
            Dock = DockStyle.Fill,
            Padding = new Padding(16)
        };

        var topPanel = new Panel { Dock = DockStyle.Top, Height = 45 };
        var lblNew = new Label { Text = "Thêm diện mới:", AutoSize = true, Location = new Point(5, 12) };
        _txtNewPrioName = new KTextBox { Placeholder = "Tên diện ưu tiên...", Location = new Point(120, 5), Width = 250 };
        var lblPts = new Label { Text = "Điểm cộng:", AutoSize = true, Location = new Point(390, 12) };
        _nudNewPrioPoints = new NumericUpDown { Minimum = 1, Maximum = 100, Value = 10, Location = new Point(475, 8), Width = 70 };
        var btnAddPrio = new KButton { Text = "+ Thêm mới", Style = KButtonStyle.Primary, Location = new Point(560, 5), Size = new Size(110, 32) };
        btnAddPrio.Click += async (s, e) => await AddPriorityAsync();

        topPanel.Controls.AddRange(new Control[] { lblNew, _txtNewPrioName, lblPts, _nudNewPrioPoints, btnAddPrio });

        _gridPriorities = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        KDataGridHelper.ApplyModernStyle(_gridPriorities);

        card.Controls.Add(_gridPriorities);
        card.Controls.Add(topPanel);
        _tabPriorities.Controls.Add(card);
    }

    private void BuildProvincesTab()
    {
        var card = new KCard
        {
            Title = "Danh mục 34 Tỉnh / Thành phố trực thuộc Trung ương (Từ 01/07/2025)",
            Dock = DockStyle.Fill,
            Padding = new Padding(16)
        };

        _gridProvinces = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        KDataGridHelper.ApplyModernStyle(_gridProvinces);

        var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 50, Padding = new Padding(5) };
        var lblNote = new Label
        {
            Text = "Nhóm 1: <100km (điểm cộng 0) | Nhóm 2: 100-300km (điểm cộng 10) | Nhóm 3: >300km (điểm cộng 20)",
            ForeColor = ThemeManager.Current.TextSecondary,
            Dock = DockStyle.Left,
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleLeft
        };
        bottomPanel.Controls.Add(lblNote);

        card.Controls.Add(_gridProvinces);
        card.Controls.Add(bottomPanel);
        _tabProvinces.Controls.Add(card);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var config = await _configService.GetSystemConfigAsync();
            _nudWPrio.Value = (decimal)config.WPriority;
            _nudWDist.Value = (decimal)config.WDistance;
            _nudWCourse.Value = (decimal)config.WCourse;
            _nudWPref.Value = (decimal)config.WPreference;
            _nudWRoommate.Value = (decimal)config.WRoommate;
            UpdateWeightSum();

            _txtBankName.Text = config.BankName;
            _txtBankBin.Text = config.BankBin;
            _txtAccountNum.Text = config.BankAccountNumber;
            _txtAccountHolder.Text = config.BankAccountHolder;
            _nudVat.Value = config.VatRate;

            await LoadPrioritiesAsync();
            await LoadProvincesAsync();
        }
        catch (Exception ex)
        {
            KToast.Show(this.FindForm() ?? new Form(), $"Lỗi tải cấu hình: {ex.Message}", ToastType.Error);
        }
    }

    private async Task LoadPrioritiesAsync()
    {
        var list = await _configService.GetPrioritiesAsync();
        _gridPriorities.DataSource = list;
        if (_gridPriorities.Columns["Id"] is { } colId) colId.Width = 60;
        if (_gridPriorities.Columns["Ten"] is { } colTen) colTen.HeaderText = "Tên diện ưu tiên";
        if (_gridPriorities.Columns["DiemCong"] is { } colDiem) colDiem.HeaderText = "Điểm cộng";
    }

    private async Task AddPriorityAsync()
    {
        if (string.IsNullOrWhiteSpace(_txtNewPrioName.Text)) return;

        var dto = new PriorityDto
        {
            Ten = _txtNewPrioName.Text.Trim(),
            DiemCong = (int)_nudNewPrioPoints.Value
        };

        var ok = await _configService.SavePriorityAsync(dto);
        if (ok)
        {
            _txtNewPrioName.Text = string.Empty;
            await LoadPrioritiesAsync();
            KToast.Show(this.FindForm() ?? new Form(), "Đã thêm diện ưu tiên mới!", ToastType.Success);
        }
    }

    private async Task LoadProvincesAsync()
    {
        var list = await _configService.GetProvincesAsync();
        _gridProvinces.DataSource = list.Select(p => new
        {
            p.Id,
            TenTinh = p.Ten,
            KhoangCachKm = $"{p.KhoangCachKm} km",
            Nhom = $"Nhóm {p.NhomKhoangCach}",
            CacTenCuAnhXa = string.Join(", ", p.TenCus)
        }).ToList();

        if (_gridProvinces.Columns["Id"] is { } colPId) colPId.Width = 50;
        if (_gridProvinces.Columns["TenTinh"] is { } colTenT) colTenT.HeaderText = "34 Tỉnh mới";
        if (_gridProvinces.Columns["KhoangCachKm"] is { } colKm) colKm.HeaderText = "Khoảng cách";
        if (_gridProvinces.Columns["Nhom"] is { } colNhom) colNhom.HeaderText = "Nhóm khoảng cách";
        if (_gridProvinces.Columns["CacTenCuAnhXa"] is { } colTenCu) colTenCu.HeaderText = "Tên cũ trước sáp nhập";
    }

    private async Task SaveWeightsAsync()
    {
        try
        {
            var config = await _configService.GetSystemConfigAsync();
            config.WPriority = (double)_nudWPrio.Value;
            config.WDistance = (double)_nudWDist.Value;
            config.WCourse = (double)_nudWCourse.Value;
            config.WPreference = (double)_nudWPref.Value;
            config.WRoommate = (double)_nudWRoommate.Value;

            var ok = await _configService.SaveSystemConfigAsync(config);
            if (ok)
            {
                KToast.Show(this.FindForm() ?? new Form(), "Đã lưu trọng số thuật toán thành công!", ToastType.Success);
            }
        }
        catch (Exception ex)
        {
            KToast.Show(this.FindForm() ?? new Form(), $"Lỗi lưu trọng số: {ex.Message}", ToastType.Error);
        }
    }

    private async Task SaveBankAsync()
    {
        try
        {
            var config = await _configService.GetSystemConfigAsync();
            config.BankName = _txtBankName.Text.Trim();
            config.BankBin = _txtBankBin.Text.Trim();
            config.BankAccountNumber = _txtAccountNum.Text.Trim();
            config.BankAccountHolder = _txtAccountHolder.Text.Trim().ToUpper();
            config.VatRate = _nudVat.Value;

            var ok = await _configService.SaveSystemConfigAsync(config);
            if (ok)
            {
                KToast.Show(this.FindForm() ?? new Form(), "Đã lưu cấu hình ngân hàng & VAT thành công!", ToastType.Success);
            }
        }
        catch (Exception ex)
        {
            KToast.Show(this.FindForm() ?? new Form(), $"Lỗi lưu cấu hình: {ex.Message}", ToastType.Error);
        }
    }

    private void OnThemeChanged()
    {
        var theme = ThemeManager.Current;
        this.BackColor = theme.Background;
        _tabWeights.BackColor = theme.Background;
        _tabBank.BackColor = theme.Background;
        _tabPriorities.BackColor = theme.Background;
        _tabProvinces.BackColor = theme.Background;
    }
}

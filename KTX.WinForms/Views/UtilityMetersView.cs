using KTX.Domain.DTOs;
using KTX.Services.Facility;
using KTX.Services.Finance;
using KTX.UI.Controls;
using KTX.UI.Helpers;
using KTX.UI.Themes;

namespace KTX.WinForms.Views;

public class UtilityMetersView : UserControl, IThemable
{
    private readonly ITieredBillingService _billingService;
    private readonly IRoomService _roomService;

    private readonly ComboBox _cboMonth;
    private readonly ComboBox _cboYear;
    private readonly ComboBox _cboBuilding;
    private readonly KButton _btnLoad;
    private readonly KButton _btnSave;
    private readonly DataGridView _grid;
    private List<UtilityReadingDto> _readings = new();
    private List<UtilityReadingRow> _rows = new();
    private bool _isInitialized;

    private class UtilityReadingRow
    {
        public int Id { get; set; }
        public int PhongId { get; set; }
        public string MaPhong { get; set; } = string.Empty;
        public string ToaTen { get; set; } = string.Empty;
        public decimal DienCu { get; set; }
        public decimal DienMoi { get; set; }
        public bool CoThayDongHoDien { get; set; }
        public decimal? DienChotDongHoCu { get; set; }
        public decimal? DienDauDongHoMoi { get; set; }
        public decimal SanLuongDien { get; set; }
        public decimal NuocCu { get; set; }
        public decimal NuocMoi { get; set; }
        public bool CoThayDongHoNuoc { get; set; }
        public decimal? NuocChotDongHoCu { get; set; }
        public decimal? NuocDauDongHoMoi { get; set; }
        public decimal SanLuongNuoc { get; set; }
        public string CanhBaoText { get; set; } = string.Empty;
        public string? GhiChu { get; set; }
    }

    public UtilityMetersView(ITieredBillingService billingService, IRoomService roomService)
    {
        _billingService = billingService;
        _roomService = roomService;

        Dock = DockStyle.Fill;
        DoubleBuffered = true;

        var topBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 65,
            Padding = new Padding(0, 10, 0, 10)
        };

        var lblMonth = new Label { Text = "Tháng:", Location = new Point(0, 18), AutoSize = true };
        _cboMonth = new ComboBox { Location = new Point(52, 14), Width = 70, DropDownStyle = ComboBoxStyle.DropDownList };
        for (int m = 1; m <= 12; m++) _cboMonth.Items.Add(m);
        _cboMonth.SelectedItem = DateTime.Now.Month;
        _cboMonth.SelectedIndexChanged += async (s, e) => { if (_isInitialized) await LoadReadingsAsync(); };

        var lblYear = new Label { Text = "Năm:", Location = new Point(135, 18), AutoSize = true };
        _cboYear = new ComboBox { Location = new Point(178, 14), Width = 90, DropDownStyle = ComboBoxStyle.DropDownList };
        for (int y = 2024; y <= 2028; y++) _cboYear.Items.Add(y);
        _cboYear.SelectedItem = DateTime.Now.Year;
        _cboYear.SelectedIndexChanged += async (s, e) => { if (_isInitialized) await LoadReadingsAsync(); };

        var lblBld = new Label { Text = "Tòa:", Location = new Point(282, 18), AutoSize = true };
        _cboBuilding = new ComboBox { Location = new Point(318, 14), Width = 155, DropDownStyle = ComboBoxStyle.DropDownList };
        _cboBuilding.SelectedIndexChanged += async (s, e) => { if (_isInitialized) await LoadReadingsAsync(); };

        _btnLoad = new KButton
        {
            Text = "Nạp dữ liệu",
            Style = KButtonStyle.Secondary,
            Location = new Point(485, 12),
            Size = new Size(120, 36)
        };
        _btnLoad.Click += async (s, e) => await LoadReadingsAsync();

        _btnSave = new KButton
        {
            Text = "Lưu tất cả chỉ số",
            Style = KButtonStyle.Primary,
            Location = new Point(615, 12),
            Size = new Size(170, 36)
        };
        _btnSave.Click += async (s, e) => await SaveAllReadingsAsync();

        topBar.Controls.AddRange(new Control[] { lblMonth, _cboMonth, lblYear, _cboYear, lblBld, _cboBuilding, _btnLoad, _btnSave });

        _grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            SelectionMode = DataGridViewSelectionMode.CellSelect,
            ScrollBars = ScrollBars.Both
        };
        KDataGridHelper.ApplyModernStyle(_grid, autoFill: false);

        SetupColumns();

        Controls.Add(_grid);
        Controls.Add(topBar);

        ApplyTheme(ThemeManager.Current);
        Load += async (s, e) => await EnsureInitializedAsync();
    }

    protected override async void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        await EnsureInitializedAsync();
    }

    private async Task EnsureInitializedAsync()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        await InitializeAsync();
    }

    private void SetupColumns()
    {
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Phòng", DataPropertyName = "MaPhong", ReadOnly = true, Width = 76, Frozen = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tòa", DataPropertyName = "ToaTen", ReadOnly = true, Width = 72, Frozen = true });

        // Điện
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Điện cũ", DataPropertyName = "DienCu", Width = 88 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Điện mới", DataPropertyName = "DienMoi", Width = 94 });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Thay ĐH (Đ)", DataPropertyName = "CoThayDongHoDien", Width = 112 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Chốt cũ (Đ)", DataPropertyName = "DienChotDongHoCu", Width = 106 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Đầu mới (Đ)", DataPropertyName = "DienDauDongHoMoi", Width = 112 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SL Điện (kWh)", DataPropertyName = "SanLuongDien", ReadOnly = true, Width = 128 });

        // Nước
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nước cũ", DataPropertyName = "NuocCu", Width = 92 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nước mới", DataPropertyName = "NuocMoi", Width = 98 });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Thay ĐH (N)", DataPropertyName = "CoThayDongHoNuoc", Width = 112 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Chốt cũ (N)", DataPropertyName = "NuocChotDongHoCu", Width = 106 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Đầu mới (N)", DataPropertyName = "NuocDauDongHoMoi", Width = 112 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SL Nước (m³)", DataPropertyName = "SanLuongNuoc", ReadOnly = true, Width = 122 });

        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cảnh báo", DataPropertyName = "CanhBaoText", ReadOnly = true, Width = 125 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ghi chú", DataPropertyName = "GhiChu", Width = 220 });

        _grid.CellFormatting += (s, e) =>
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 14 && _readings.Count > e.RowIndex)
            {
                if (_readings[e.RowIndex].CanhBaoTieuThuBatThuong)
                {
                    e.CellStyle.ForeColor = Color.Crimson;
                    e.CellStyle.Font = new Font(_grid.Font, FontStyle.Bold);
                }
            }
        };
    }

    private async Task InitializeAsync()
    {
        var buildings = await _roomService.GetBuildingsAsync();
        _cboBuilding.Items.Clear();
        _cboBuilding.Items.Add(new BuildingDto { Id = 0, Ten = "Tất cả các tòa" });
        foreach (var b in buildings) _cboBuilding.Items.Add(b);
        _cboBuilding.DisplayMember = "Ten";
        _cboBuilding.ValueMember = "Id";
        _cboBuilding.SelectedIndex = 0;

        await LoadReadingsAsync();
    }

    private async Task LoadReadingsAsync()
    {
        int month = (int)(_cboMonth.SelectedItem ?? DateTime.Now.Month);
        int year = (int)(_cboYear.SelectedItem ?? DateTime.Now.Year);
        int? bldId = (_cboBuilding.SelectedItem as BuildingDto)?.Id;

        _readings = await _billingService.GetReadingsAsync(month, year, bldId);

        _rows = _readings.Select(r => new UtilityReadingRow
        {
            Id = r.Id,
            PhongId = r.PhongId,
            MaPhong = r.MaPhong,
            ToaTen = r.ToaTen,
            DienCu = r.DienCu,
            DienMoi = r.DienMoi,
            CoThayDongHoDien = r.CoThayDongHoDien,
            DienChotDongHoCu = r.DienChotDongHoCu,
            DienDauDongHoMoi = r.DienDauDongHoMoi,
            SanLuongDien = r.SanLuongDien,
            NuocCu = r.NuocCu,
            NuocMoi = r.NuocMoi,
            CoThayDongHoNuoc = r.CoThayDongHoNuoc,
            NuocChotDongHoCu = r.NuocChotDongHoCu,
            NuocDauDongHoMoi = r.NuocDauDongHoMoi,
            SanLuongNuoc = r.SanLuongNuoc,
            CanhBaoText = r.CanhBaoTieuThuBatThuong ? "● Bất thường" : "Bình thường",
            GhiChu = r.GhiChu
        }).ToList();

        _grid.AutoGenerateColumns = false;
        _grid.DataSource = null;
        _grid.DataSource = _rows;
    }

    private async Task SaveAllReadingsAsync()
    {
        _grid.EndEdit();
        int month = (int)(_cboMonth.SelectedItem ?? DateTime.Now.Month);
        int year = (int)(_cboYear.SelectedItem ?? DateTime.Now.Year);

        int savedCount = 0;
        int errorCount = 0;
        var errorMessages = new List<string>();

        foreach (var item in _rows)
        {
            try
            {
                var saveDto = new UtilityReadingSaveDto
                {
                    PhongId = item.PhongId,
                    Thang = month,
                    Nam = year,
                    DienCu = item.DienCu,
                    DienMoi = item.DienMoi,
                    CoThayDongHoDien = item.CoThayDongHoDien,
                    DienChotDongHoCu = item.DienChotDongHoCu,
                    DienDauDongHoMoi = item.DienDauDongHoMoi,
                    NuocCu = item.NuocCu,
                    NuocMoi = item.NuocMoi,
                    CoThayDongHoNuoc = item.CoThayDongHoNuoc,
                    NuocChotDongHoCu = item.NuocChotDongHoCu,
                    NuocDauDongHoMoi = item.NuocDauDongHoMoi,
                    GhiChu = item.GhiChu
                };

                await _billingService.SaveReadingAsync(saveDto);
                savedCount++;
            }
            catch (Exception ex)
            {
                errorCount++;
                errorMessages.Add($"Phòng {item.MaPhong}: {ex.Message}");
            }
        }

        if (errorCount == 0)
        {
            KToast.Show(FindForm() ?? new Form(), $"Đã lưu thành công chỉ số cho {savedCount} phòng!", ToastType.Success);
        }
        else
        {
            MessageBox.Show($"Đã lưu {savedCount} phòng.\n{errorCount} phòng gặp lỗi:\n" + string.Join("\n", errorMessages.Take(5)),
                "Kết quả lưu chỉ số", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        await LoadReadingsAsync();
    }

    public void ApplyTheme(Theme theme)
    {
        BackColor = theme.Surface;
        KDataGridHelper.ApplyModernStyle(_grid, autoFill: false);
    }
}

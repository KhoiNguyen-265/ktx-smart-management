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
        _cboMonth = new ComboBox { Location = new Point(50, 14), Width = 70, DropDownStyle = ComboBoxStyle.DropDownList };
        for (int m = 1; m <= 12; m++) _cboMonth.Items.Add(m);
        _cboMonth.SelectedItem = DateTime.Now.Month;

        var lblYear = new Label { Text = "Năm:", Location = new Point(135, 18), AutoSize = true };
        _cboYear = new ComboBox { Location = new Point(175, 14), Width = 90, DropDownStyle = ComboBoxStyle.DropDownList };
        for (int y = 2024; y <= 2028; y++) _cboYear.Items.Add(y);
        _cboYear.SelectedItem = DateTime.Now.Year;

        var lblBld = new Label { Text = "Tòa:", Location = new Point(280, 18), AutoSize = true };
        _cboBuilding = new ComboBox { Location = new Point(315, 14), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };

        _btnLoad = new KButton
        {
            Text = "🔄 Nạp dữ liệu",
            Style = KButtonStyle.Secondary,
            Location = new Point(480, 12),
            Size = new Size(120, 36)
        };
        _btnLoad.Click += async (s, e) => await LoadReadingsAsync();

        _btnSave = new KButton
        {
            Text = "💾 Lưu Tất Cả Chỉ Số",
            Style = KButtonStyle.Primary,
            Location = new Point(610, 12),
            Size = new Size(180, 36)
        };
        _btnSave.Click += async (s, e) => await SaveAllReadingsAsync();

        topBar.Controls.AddRange(new Control[] { lblMonth, _cboMonth, lblYear, _cboYear, lblBld, _cboBuilding, _btnLoad, _btnSave });

        _grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            SelectionMode = DataGridViewSelectionMode.CellSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        KDataGridHelper.ApplyModernStyle(_grid);

        SetupColumns();

        Controls.Add(_grid);
        Controls.Add(topBar);

        ApplyTheme(ThemeManager.Current);
        Load += async (s, e) => await InitializeAsync();
    }

    private void SetupColumns()
    {
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Phòng", DataPropertyName = "MaPhong", ReadOnly = true, FillWeight = 65 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tòa", DataPropertyName = "ToaTen", ReadOnly = true, FillWeight = 65 });

        // Điện
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Điện cũ", DataPropertyName = "DienCu", FillWeight = 75 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Điện mới", DataPropertyName = "DienMoi", FillWeight = 75 });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Thay ĐH Điện", DataPropertyName = "CoThayDongHoDien", FillWeight = 80 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Chốt cũ (Đ)", DataPropertyName = "DienChotDongHoCu", FillWeight = 75 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Đầu mới (Đ)", DataPropertyName = "DienDauDongHoMoi", FillWeight = 75 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Sản lượng điện", DataPropertyName = "SanLuongDien", ReadOnly = true, FillWeight = 85 });

        // Nước
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nước cũ", DataPropertyName = "NuocCu", FillWeight = 75 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nước mới", DataPropertyName = "NuocMoi", FillWeight = 75 });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Thay ĐH Nước", DataPropertyName = "CoThayDongHoNuoc", FillWeight = 80 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Chốt cũ (N)", DataPropertyName = "NuocChotDongHoCu", FillWeight = 75 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Đầu mới (N)", DataPropertyName = "NuocDauDongHoMoi", FillWeight = 75 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Sản lượng nước", DataPropertyName = "SanLuongNuoc", ReadOnly = true, FillWeight = 85 });

        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cảnh báo", DataPropertyName = "CanhBaoText", ReadOnly = true, FillWeight = 90 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ghi chú", DataPropertyName = "GhiChu", FillWeight = 110 });

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

        _grid.AutoGenerateColumns = false;
        _grid.DataSource = _readings.Select(r => new
        {
            r.Id,
            r.PhongId,
            r.MaPhong,
            r.ToaTen,
            r.DienCu,
            r.DienMoi,
            r.CoThayDongHoDien,
            r.DienChotDongHoCu,
            r.DienDauDongHoMoi,
            r.SanLuongDien,
            r.NuocCu,
            r.NuocMoi,
            r.CoThayDongHoNuoc,
            r.NuocChotDongHoCu,
            r.NuocDauDongHoMoi,
            r.SanLuongNuoc,
            CanhBaoText = r.CanhBaoTieuThuBatThuong ? "⚠️ Bất thường" : "Bình thường",
            r.GhiChu
        }).ToList();
    }

    private async Task SaveAllReadingsAsync()
    {
        int month = (int)(_cboMonth.SelectedItem ?? DateTime.Now.Month);
        int year = (int)(_cboYear.SelectedItem ?? DateTime.Now.Year);

        int savedCount = 0;
        int errorCount = 0;
        var errorMessages = new List<string>();

        foreach (DataGridViewRow row in _grid.Rows)
        {
            if (row.DataBoundItem == null) continue;
            dynamic item = row.DataBoundItem;
            int phongId = item.PhongId;

            try
            {
                var saveDto = new UtilityReadingSaveDto
                {
                    PhongId = phongId,
                    Thang = month,
                    Nam = year,
                    DienCu = (decimal)item.DienCu,
                    DienMoi = (decimal)item.DienMoi,
                    CoThayDongHoDien = (bool)item.CoThayDongHoDien,
                    DienChotDongHoCu = item.DienChotDongHoCu != null ? (decimal)item.DienChotDongHoCu : null,
                    DienDauDongHoMoi = item.DienDauDongHoMoi != null ? (decimal)item.DienDauDongHoMoi : null,
                    NuocCu = (decimal)item.NuocCu,
                    NuocMoi = (decimal)item.NuocMoi,
                    CoThayDongHoNuoc = (bool)item.CoThayDongHoNuoc,
                    NuocChotDongHoCu = item.NuocChotDongHoCu != null ? (decimal)item.NuocChotDongHoCu : null,
                    NuocDauDongHoMoi = item.NuocDauDongHoMoi != null ? (decimal)item.NuocDauDongHoMoi : null,
                    GhiChu = item.GhiChu?.ToString()
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
        KDataGridHelper.ApplyModernStyle(_grid);
    }
}

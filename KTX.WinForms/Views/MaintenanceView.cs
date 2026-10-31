using System.Drawing;
using System.Windows.Forms;
using KTX.Domain.DTOs;
using KTX.Services.Facility;
using KTX.UI.Controls;
using KTX.UI.Helpers;
using KTX.UI.Themes;

namespace KTX.WinForms.Views;

public class MaintenanceView : UserControl, IThemable
{
    private readonly IMaintenanceService _maintenanceService;
    private readonly IRoomService _roomService;

    private readonly ComboBox _cboStatusFilter;
    private readonly KButton _btnCreate;
    private readonly KButton _btnUpdateStatus;
    private readonly KButton _btnRefresh;

    private readonly Panel _panelStats;
    private readonly Label _lblStatTotal;
    private readonly Label _lblStatPending;
    private readonly Label _lblStatProcessing;
    private readonly Label _lblStatCompleted;
    private readonly Label _lblStatCost;

    private readonly DataGridView _grid;
    private List<MaintenanceRequestDto> _requests = new();

    public MaintenanceView(IMaintenanceService maintenanceService, IRoomService roomService)
    {
        _maintenanceService = maintenanceService;
        _roomService = roomService;

        Dock = DockStyle.Fill;
        DoubleBuffered = true;

        // Top Bar
        var topBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(0, 10, 0, 10)
        };

        var lblFilter = new Label { Text = "Trạng thái:", Location = new Point(0, 16), AutoSize = true };
        _cboStatusFilter = new ComboBox
        {
            Location = new Point(70, 12),
            Width = 140,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _cboStatusFilter.Items.AddRange(new object[] { "Tất cả", "Chưa xử lý", "Đang xử lý", "Đã hoàn thành" });
        _cboStatusFilter.SelectedIndex = 0;
        _cboStatusFilter.SelectedIndexChanged += async (s, e) => await LoadDataAsync();

        _btnCreate = new KButton
        {
            Text = "➕ Báo Hỏng / Tạo Yêu Cầu",
            Style = KButtonStyle.Primary,
            Location = new Point(230, 10),
            Size = new Size(205, 38)
        };
        _btnCreate.Click += async (s, e) => await HandleCreateAsync();

        _btnUpdateStatus = new KButton
        {
            Text = "🛠️ Cập nhật Tiến độ & Chi phí",
            Style = KButtonStyle.Secondary,
            Location = new Point(445, 10),
            Size = new Size(225, 38)
        };
        _btnUpdateStatus.Click += async (s, e) => await HandleUpdateStatusAsync();

        _btnRefresh = new KButton
        {
            Text = "🔄 Nạp lại",
            Style = KButtonStyle.Secondary,
            Location = new Point(680, 10),
            Size = new Size(100, 38)
        };
        _btnRefresh.Click += async (s, e) => await LoadDataAsync();

        topBar.Controls.AddRange(new Control[] { lblFilter, _cboStatusFilter, _btnCreate, _btnUpdateStatus, _btnRefresh });

        // Stats Banner
        _panelStats = new Panel
        {
            Dock = DockStyle.Top,
            Height = 45,
            Padding = new Padding(0, 4, 0, 4)
        };

        _lblStatTotal = new Label { Text = "📋 Tổng: 0", Location = new Point(0, 10), AutoSize = true, Font = UIHelper.GetFont(9.5f, FontStyle.Bold) };
        _lblStatPending = new Label { Text = "⏳ Chưa xử lý: 0", Location = new Point(120, 10), AutoSize = true, Font = UIHelper.GetFont(9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(198, 40, 40) };
        _lblStatProcessing = new Label { Text = "🛠️ Đang sửa: 0", Location = new Point(260, 10), AutoSize = true, Font = UIHelper.GetFont(9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(230, 81, 0) };
        _lblStatCompleted = new Label { Text = "✅ Đã xong: 0", Location = new Point(390, 10), AutoSize = true, Font = UIHelper.GetFont(9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(46, 125, 50) };
        _lblStatCost = new Label { Text = "💰 Tổng chi phí: 0 đ", Location = new Point(530, 10), AutoSize = true, Font = UIHelper.GetFont(9.5f, FontStyle.Bold) };

        _panelStats.Controls.AddRange(new Control[] { _lblStatTotal, _lblStatPending, _lblStatProcessing, _lblStatCompleted, _lblStatCost });

        // Grid
        _grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        KDataGridHelper.ApplyModernStyle(_grid);

        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = "Id", Width = 50 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tòa", DataPropertyName = "ToaTen", Width = 110 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Phòng", DataPropertyName = "MaPhong", Width = 90 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mô tả sự cố / Hỏng hóc", DataPropertyName = "MoTa", Width = 280 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", DataPropertyName = "TrangThaiText", Width = 130 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ngày báo", DataPropertyName = "NgayTaoText", Width = 120 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ngày xong", DataPropertyName = "NgayHoanThanhText", Width = 120 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Chi phí vật tư", DataPropertyName = "ChiPhiText", Width = 120 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ghi chú xử lý", DataPropertyName = "GhiChu", Width = 180 });

        Controls.Add(_grid);
        Controls.Add(_panelStats);
        Controls.Add(topBar);

        ApplyTheme(ThemeManager.Current);

        Load += async (s, e) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            string? status = _cboStatusFilter.SelectedIndex switch
            {
                1 => "ChuaXuLy",
                2 => "DangXuLy",
                3 => "HoanThanh",
                _ => null
            };

            _requests = await _maintenanceService.GetRequestsAsync(status);
            var stats = await _maintenanceService.GetStatsAsync();

            _lblStatTotal.Text = $"📋 Tổng: {stats.TotalRequests}";
            _lblStatPending.Text = $"⏳ Chưa xử lý: {stats.PendingCount}";
            _lblStatProcessing.Text = $"🛠️ Đang sửa: {stats.ProcessingCount}";
            _lblStatCompleted.Text = $"✅ Đã xong: {stats.CompletedCount}";
            _lblStatCost.Text = $"💰 Chi phí: {stats.TotalCost:N0} đ";

            _grid.Rows.Clear();
            foreach (var r in _requests)
            {
                _grid.Rows.Add(
                    r.Id,
                    r.ToaTen,
                    r.MaPhong,
                    r.MoTa,
                    r.TrangThaiText,
                    r.NgayTao.ToString("dd/MM/yyyy HH:mm"),
                    r.NgayHoanThanh.HasValue ? r.NgayHoanThanh.Value.ToString("dd/MM/yyyy HH:mm") : "---",
                    $"{r.ChiPhi:N0} đ",
                    r.GhiChu ?? ""
                );
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể nạp danh sách sự cố: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private record RoomComboItem(int Id, string Display)
    {
        public override string ToString() => Display;
    }

    private async Task HandleCreateAsync()
    {
        var rooms = await _roomService.GetAllRoomsAsync();

        using var dlg = new Form
        {
            Text = "Tạo Yêu Cầu Báo Hỏng / Sửa Chữa Thiết Bị",
            ClientSize = new Size(480, 360),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var lblRoom = new Label { Text = "Chọn phòng xảy ra sự cố:", Location = new Point(20, 20), AutoSize = true };
        var cboRoom = new ComboBox { Location = new Point(20, 45), Width = 430, DropDownStyle = ComboBoxStyle.DropDownList };
        foreach (var r in rooms)
        {
            cboRoom.Items.Add(new RoomComboItem(r.Id, $"{r.MaPhong} ({r.TenToa} - Tầng {r.Tang})"));
        }
        if (cboRoom.Items.Count > 0) cboRoom.SelectedIndex = 0;

        var lblDesc = new Label { Text = "Mô tả sự cố / thiết bị hư hỏng (bắt buộc):", Location = new Point(20, 85), AutoSize = true };
        var txtDesc = new TextBox { Location = new Point(20, 110), Width = 430, Height = 65, Multiline = true };

        var lblNote = new Label { Text = "Ghi chú thêm (người báo, thời gian tiện kiểm tra):", Location = new Point(20, 185), AutoSize = true };
        var txtNote = new TextBox { Location = new Point(20, 210), Width = 430, Height = 45, Multiline = true };

        var btnSubmit = new KButton { Text = "Lưu yêu cầu", Style = KButtonStyle.Primary, Location = new Point(330, 290), Size = new Size(120, 36) };
        btnSubmit.Click += async (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(txtDesc.Text))
            {
                MessageBox.Show("Vui lòng nhập mô tả sự cố.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selRoom = cboRoom.SelectedItem as RoomComboItem;
            if (selRoom == null) return;

            try
            {
                await _maintenanceService.CreateRequestAsync(new MaintenanceCreateDto
                {
                    PhongId = selRoom.Id,
                    MoTa = txtDesc.Text.Trim(),
                    GhiChu = txtNote.Text.Trim()
                });

                dlg.DialogResult = DialogResult.OK;
                dlg.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tạo yêu cầu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        var btnCancel = new KButton { Text = "Hủy", Style = KButtonStyle.Secondary, Location = new Point(230, 290), Size = new Size(90, 36) };
        btnCancel.Click += (s, e) => dlg.Close();

        dlg.Controls.AddRange(new Control[] { lblRoom, cboRoom, lblDesc, txtDesc, lblNote, txtNote, btnSubmit, btnCancel });

        if (dlg.ShowDialog() == DialogResult.OK)
        {
            await LoadDataAsync();
        }
    }

    private async Task HandleUpdateStatusAsync()
    {
        if (_grid.SelectedRows.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn một sự cố từ danh sách.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        int reqId = Convert.ToInt32(_grid.SelectedRows[0].Cells[0].Value);
        var req = _requests.FirstOrDefault(r => r.Id == reqId);
        if (req == null) return;

        using var dlg = new Form
        {
            Text = $"Cập Nhật Xử Lý Sự Cố #{req.Id} - Phòng {req.MaPhong}",
            ClientSize = new Size(460, 310),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var lblStatus = new Label { Text = "Trạng thái mới:", Location = new Point(20, 20), AutoSize = true };
        var cboStatus = new ComboBox { Location = new Point(20, 45), Width = 410, DropDownStyle = ComboBoxStyle.DropDownList };
        cboStatus.Items.AddRange(new object[] { "Chưa xử lý", "Đang xử lý", "Đã hoàn thành" });
        cboStatus.SelectedIndex = req.TrangThai switch
        {
            "DangXuLy" => 1,
            "HoanThanh" => 2,
            _ => 0
        };

        var lblCost = new Label { Text = "Chi phí vật tư thay thế (VNĐ):", Location = new Point(20, 85), AutoSize = true };
        var numCost = new NumericUpDown { Location = new Point(20, 110), Width = 410, Maximum = 100000000, Increment = 50000, Value = req.ChiPhi };

        var lblNote = new Label { Text = "Ghi chú nhân viên kỹ thuật:", Location = new Point(20, 150), AutoSize = true };
        var txtNote = new TextBox { Location = new Point(20, 175), Width = 410, Height = 55, Multiline = true, Text = req.GhiChu ?? "" };

        var btnSubmit = new KButton { Text = "Cập nhật", Style = KButtonStyle.Primary, Location = new Point(320, 250), Size = new Size(110, 36) };
        btnSubmit.Click += async (s, e) =>
        {
            string newStatus = cboStatus.SelectedIndex switch
            {
                1 => "DangXuLy",
                2 => "HoanThanh",
                _ => "ChuaXuLy"
            };

            await _maintenanceService.UpdateRequestStatusAsync(new MaintenanceStatusUpdateDto
            {
                Id = req.Id,
                TrangThai = newStatus,
                ChiPhi = numCost.Value,
                GhiChu = txtNote.Text.Trim()
            });

            dlg.DialogResult = DialogResult.OK;
            dlg.Close();
        };

        var btnCancel = new KButton { Text = "Hủy", Style = KButtonStyle.Secondary, Location = new Point(220, 250), Size = new Size(90, 36) };
        btnCancel.Click += (s, e) => dlg.Close();

        dlg.Controls.AddRange(new Control[] { lblStatus, cboStatus, lblCost, numCost, lblNote, txtNote, btnSubmit, btnCancel });

        if (dlg.ShowDialog() == DialogResult.OK)
        {
            await LoadDataAsync();
        }
    }

    public void ApplyTheme(Theme theme)
    {
        BackColor = theme.Background;
        _panelStats.BackColor = theme.Surface;
        _lblStatTotal.ForeColor = theme.TextPrimary;
        _lblStatCost.ForeColor = theme.TextPrimary;
        KDataGridHelper.ApplyModernStyle(_grid);
    }
}

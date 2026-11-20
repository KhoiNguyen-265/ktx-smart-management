using System.Drawing;
using System.Windows.Forms;
using KTX.Domain.DTOs;
using KTX.Services.Students;
using KTX.UI.Controls;
using KTX.UI.Helpers;
using KTX.UI.Themes;

namespace KTX.WinForms.Views;

public class StudentMaintenanceView : UserControl, IThemable
{
    private readonly IStudentPortalService _portalService;
    private readonly string _mssv;

    private readonly KButton _btnReport;
    private readonly KButton _btnRefresh;
    private readonly DataGridView _grid;
    private List<MaintenanceRequestDto> _requests = new();
    private bool _isLoaded;

    public StudentMaintenanceView(IStudentPortalService portalService, string mssv)
    {
        _portalService = portalService;
        _mssv = mssv;

        Dock = DockStyle.Fill;
        DoubleBuffered = true;

        var topBar = new Panel { Dock = DockStyle.Top, Height = 58, Padding = new Padding(0, 8, 0, 10) };

        _btnReport = new KButton
        {
            Text = "+ Gửi yêu cầu báo hỏng thiết bị phòng",
            Style = KButtonStyle.Primary,
            Location = new Point(0, 8),
            Size = new Size(270, 38)
        };
        _btnReport.Click += async (s, e) => await HandleReportIssueAsync();

        _btnRefresh = new KButton
        {
            Text = "Làm mới",
            Style = KButtonStyle.Secondary,
            Location = new Point(282, 8),
            Size = new Size(100, 38)
        };
        _btnRefresh.Click += async (s, e) => await LoadDataAsync();

        topBar.Controls.AddRange(new Control[] { _btnReport, _btnRefresh });

        _grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect
        };
        KDataGridHelper.ApplyModernStyle(_grid);

        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mã YC", DataPropertyName = "Id", FillWeight = 55 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Phòng", DataPropertyName = "MaPhong", FillWeight = 70 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mô tả sự cố / Thiết bị hỏng", DataPropertyName = "MoTa", FillWeight = 230 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", DataPropertyName = "TrangThaiText", FillWeight = 110 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ngày báo", DataPropertyName = "NgayTaoText", FillWeight = 115 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ngày hoàn thành", DataPropertyName = "NgayXongText", FillWeight = 115 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Phản hồi kỹ thuật", DataPropertyName = "GhiChu", FillWeight = 180 });

        Controls.Add(_grid);
        Controls.Add(topBar);

        ApplyTheme(ThemeManager.Current);
        Load += async (s, e) => await EnsureLoadedAsync();
    }

    protected override async void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        await EnsureLoadedAsync();
    }

    private async Task EnsureLoadedAsync()
    {
        if (_isLoaded) return;
        _isLoaded = true;
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            _requests = await _portalService.GetRoomMaintenanceRequestsAsync(_mssv);
            _grid.DataSource = _requests.Select(r => new
            {
                r.Id,
                r.MaPhong,
                r.MoTa,
                TrangThaiText = r.TrangThai switch
                {
                    "HoanThanh" => "● Đã hoàn thành",
                    "DangXuLy" => "● Đang xử lý",
                    _ => "● Chưa xử lý"
                },
                NgayTaoText = r.NgayTao.ToString("dd/MM/yyyy HH:mm"),
                NgayXongText = r.NgayHoanThanh.HasValue ? r.NgayHoanThanh.Value.ToString("dd/MM/yyyy HH:mm") : "---",
                GhiChu = r.GhiChu ?? ""
            }).ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải danh sách sự cố phòng: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task HandleReportIssueAsync()
    {
        using var dlg = new Form
        {
            Text = "Gửi Yêu Cầu Báo Hỏng Thiết Bị Phòng KTX",
            ClientSize = new Size(460, 290),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = ThemeManager.Current.Surface
        };

        var lblDesc = new Label { Text = "Mô tả thiết bị hỏng / sự cố tại phòng bạn (bắt buộc):", Location = new Point(20, 20), AutoSize = true };
        var txtDesc = new TextBox { Location = new Point(20, 44), Width = 420, Height = 70, Multiline = true, Font = UIHelper.GetFont(9.5f) };

        var lblNote = new Label { Text = "Ghi chú thêm (khung giờ có người ở phòng để thợ vào sửa):", Location = new Point(20, 126), AutoSize = true };
        var txtNote = new TextBox { Location = new Point(20, 150), Width = 420, Height = 55, Multiline = true, Font = UIHelper.GetFont(9.5f) };

        var btnSend = new KButton { Text = "Gửi yêu cầu", Style = KButtonStyle.Primary, Location = new Point(315, 228), Size = new Size(125, 38) };
        btnSend.Click += async (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(txtDesc.Text))
            {
                MessageBox.Show("Vui lòng nhập mô tả sự cố.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                await _portalService.ReportRoomIssueAsync(_mssv, txtDesc.Text, txtNote.Text);
                dlg.DialogResult = DialogResult.OK;
                dlg.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể gửi yêu cầu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        var btnCancel = new KButton { Text = "Hủy", Style = KButtonStyle.Secondary, Location = new Point(215, 228), Size = new Size(90, 38) };
        btnCancel.Click += (s, e) => dlg.Close();

        dlg.Controls.AddRange(new Control[] { lblDesc, txtDesc, lblNote, txtNote, btnSend, btnCancel });
        if (dlg.ShowDialog(FindForm()) == DialogResult.OK)
        {
            KToast.Show(FindForm() ?? new Form(), "Đã gửi yêu cầu báo hỏng tới Ban quản lý KTX!", ToastType.Success);
            await LoadDataAsync();
        }
    }

    public void ApplyTheme(Theme theme)
    {
        BackColor = theme.Background;
        KDataGridHelper.ApplyModernStyle(_grid);
    }
}

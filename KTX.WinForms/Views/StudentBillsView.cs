using System.Drawing;
using System.Windows.Forms;
using KTX.Domain.DTOs;
using KTX.Domain.Enums;
using KTX.Infrastructure.VietQR;
using KTX.Services.Configuration;
using KTX.Services.Students;
using KTX.UI.Controls;
using KTX.UI.Helpers;
using KTX.UI.Themes;

namespace KTX.WinForms.Views;

public class StudentBillsView : UserControl, IThemable
{
    private readonly IStudentPortalService _portalService;
    private readonly IConfigurationService _configService;
    private readonly string _mssv;

    private readonly TabControl _tabMain;
    private readonly DataGridView _gridRoomFees;
    private readonly DataGridView _gridUtility;
    private readonly Panel _panelQr;
    private readonly Label _lblQrTitle;
    private readonly PictureBox _picQr;
    private readonly Label _lblQrDetails;
    private readonly KButton _btnRefresh;

    private List<RoomFeeDto> _roomFees = new();
    private List<InvoiceDto> _utilityInvoices = new();
    private SystemConfigDto _config = new();
    private bool _isLoaded;

    public StudentBillsView(
        IStudentPortalService portalService,
        IConfigurationService configService,
        string mssv)
    {
        _portalService = portalService;
        _configService = configService;
        _mssv = mssv;

        Dock = DockStyle.Fill;
        DoubleBuffered = true;

        var topBar = new Panel { Dock = DockStyle.Top, Height = 54, Padding = new Padding(0, 6, 0, 8) };
        var lblHeader = new Label
        {
            Text = "Theo dõi khoản thu tiền phòng, điện nước & Quét mã VietQR thanh toán nhanh",
            Font = UIHelper.GetFont(10f, FontStyle.Bold),
            Dock = DockStyle.Left,
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleLeft
        };
        _btnRefresh = new KButton
        {
            Text = "Làm mới",
            Style = KButtonStyle.Secondary,
            Size = new Size(105, 36),
            Dock = DockStyle.Right
        };
        _btnRefresh.Click += async (s, e) => await LoadDataAsync();
        topBar.Controls.Add(_btnRefresh);
        topBar.Controls.Add(lblHeader);

        // Right VietQR Panel
        _panelQr = new Panel
        {
            Dock = DockStyle.Right,
            Width = 330,
            Padding = new Padding(16)
        };

        _lblQrTitle = new Label
        {
            Text = "QUÉT MÃ VIETQR THANH TOÁN",
            Font = UIHelper.GetFont(10.5f, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 30,
            TextAlign = ContentAlignment.MiddleCenter
        };

        _picQr = new PictureBox
        {
            Size = new Size(240, 240),
            SizeMode = PictureBoxSizeMode.Zoom,
            Location = new Point(45, 48)
        };

        _lblQrDetails = new Label
        {
            Text = "Chọn một khoản thu còn nợ trong danh sách để hiển thị mã VietQR chuyển khoản.",
            Font = UIHelper.GetFont(9.2f),
            Location = new Point(16, 304),
            Size = new Size(298, 220),
            TextAlign = ContentAlignment.TopCenter
        };

        _panelQr.Controls.AddRange(new Control[] { _lblQrTitle, _picQr, _lblQrDetails });

        // Tabs
        _tabMain = new TabControl { Dock = DockStyle.Fill, Font = UIHelper.GetFont(10f) };
        var tabFees = new TabPage("Tiền phòng của tôi");
        var tabUtil = new TabPage("Hóa đơn Điện Nước phòng");

        _gridRoomFees = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
        KDataGridHelper.ApplyModernStyle(_gridRoomFees);
        _gridRoomFees.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mã thanh toán", DataPropertyName = "MaThanhToan", FillWeight = 110 });
        _gridRoomFees.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Kỳ hạn", DataPropertyName = "KyHan", FillWeight = 115 });
        _gridRoomFees.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Phải thu", DataPropertyName = "PhaiThuText", FillWeight = 90 });
        _gridRoomFees.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Đã đóng", DataPropertyName = "DaThuText", FillWeight = 90 });
        _gridRoomFees.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Còn nợ", DataPropertyName = "ConNoText", FillWeight = 90 });
        _gridRoomFees.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", DataPropertyName = "TrangThaiText", FillWeight = 110 });
        _gridRoomFees.SelectionChanged += (s, e) => OnRoomFeeSelected();

        _gridUtility = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
        KDataGridHelper.ApplyModernStyle(_gridUtility);
        _gridUtility.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mã hóa đơn", DataPropertyName = "MaThanhToan", FillWeight = 110 });
        _gridUtility.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Kỳ hóa đơn", DataPropertyName = "KyHoaDon", FillWeight = 80 });
        _gridUtility.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tổng phòng", DataPropertyName = "TongTienText", FillWeight = 95 });
        _gridUtility.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Đã đóng", DataPropertyName = "DaThuText", FillWeight = 95 });
        _gridUtility.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Còn nợ", DataPropertyName = "ConNoText", FillWeight = 95 });
        _gridUtility.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", DataPropertyName = "TrangThaiText", FillWeight = 110 });
        _gridUtility.SelectionChanged += (s, e) => OnUtilitySelected();

        tabFees.Controls.Add(_gridRoomFees);
        tabUtil.Controls.Add(_gridUtility);
        _tabMain.TabPages.AddRange(new[] { tabFees, tabUtil });

        Controls.Add(_tabMain);
        Controls.Add(_panelQr);
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
            _config = await _configService.GetSystemConfigAsync();
            _roomFees = await _portalService.GetPersonalRoomFeesAsync(_mssv);
            _utilityInvoices = await _portalService.GetRoomUtilityInvoicesAsync(_mssv);

            _gridRoomFees.DataSource = _roomFees.Select(f => new
            {
                f.MaThanhToan,
                KyHan = $"{f.TuNgay:dd/MM/yyyy} - {f.DenNgay:dd/MM/yyyy}",
                PhaiThuText = $"{f.PhaiThu:N0} đ",
                DaThuText = $"{f.DaThu:N0} đ",
                ConNoText = $"{f.ConNo:N0} đ",
                TrangThaiText = FormatStatus(f.TrangThai)
            }).ToList();

            _gridUtility.DataSource = _utilityInvoices.Select(i => new
            {
                i.MaThanhToan,
                KyHoaDon = $"Tháng {i.Thang}/{i.Nam}",
                TongTienText = $"{i.TongTien:N0} đ",
                DaThuText = $"{i.DaThu:N0} đ",
                ConNoText = $"{i.ConNo:N0} đ",
                TrangThaiText = FormatStatus(i.TrangThai)
            }).ToList();

            // Tự động hiển thị QR cho khoản nợ đầu tiên
            int unpaidFeeIdx = _roomFees.FindIndex(f => f.ConNo > 0);
            if (unpaidFeeIdx >= 0 && unpaidFeeIdx < _gridRoomFees.Rows.Count)
            {
                _gridRoomFees.ClearSelection();
                _gridRoomFees.Rows[unpaidFeeIdx].Selected = true;
                _gridRoomFees.CurrentCell = _gridRoomFees.Rows[unpaidFeeIdx].Cells[0];
                OnRoomFeeSelected();
            }
            else if (_roomFees.Count > 0)
            {
                OnRoomFeeSelected();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải danh sách hóa đơn: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string FormatStatus(TrangThaiHoaDon st) => st switch
    {
        TrangThaiHoaDon.DaThanhToan => "● Đã thanh toán",
        TrangThaiHoaDon.ThanhToanMotPhan => "● Thanh toán 1 phần",
        _ => "● Chưa thanh toán"
    };

    private void OnRoomFeeSelected()
    {
        if (_gridRoomFees.CurrentRow?.Index >= 0 && _gridRoomFees.CurrentRow.Index < _roomFees.Count)
        {
            var f = _roomFees[_gridRoomFees.CurrentRow.Index];
            ShowQr(f.VietQrPayload, f.MaThanhToan, f.ConNo, $"Tiền phòng ({f.TuNgay:MM/yyyy}) - Phòng {f.MaPhong}");
        }
    }

    private void OnUtilitySelected()
    {
        if (_gridUtility.CurrentRow?.Index >= 0 && _gridUtility.CurrentRow.Index < _utilityInvoices.Count)
        {
            var i = _utilityInvoices[_gridUtility.CurrentRow.Index];
            ShowQr(i.VietQrPayload, i.MaThanhToan, i.ConNo, $"Điện Nước Tháng {i.Thang}/{i.Nam} - Phòng {i.MaPhong}");
        }
    }

    private void ShowQr(string payload, string code, decimal amount, string desc)
    {
        if (amount <= 0)
        {
            _picQr.Image = null;
            _lblQrDetails.Text = $"✓ Mã {code}\n{desc}\n\nĐÃ HOÀN THÀNH THANH TOÁN!";
            return;
        }

        try
        {
            _picQr.Image = VietQRHelper.GenerateQrBitmap(payload, 5);
            _lblQrDetails.Text =
                $"NỘI DUNG CK (BẮT BUỘC):\n{code}\n\n" +
                $"Khoản thu: {desc}\n" +
                $"SỐ TIỀN CẦN ĐÓNG: {amount:N0} VNĐ\n\n" +
                $"Ngân hàng: {_config.BankName}\n" +
                $"Số TK: {_config.BankAccountNumber}\n" +
                $"Chủ TK: {_config.BankAccountHolder}";
        }
        catch (Exception ex)
        {
            _lblQrDetails.Text = $"Không thể sinh mã VietQR: {ex.Message}";
        }
    }

    public void ApplyTheme(Theme theme)
    {
        BackColor = theme.Background;
        _panelQr.BackColor = theme.Surface;
        _lblQrTitle.ForeColor = theme.Primary;
        _lblQrDetails.ForeColor = theme.TextPrimary;
        KDataGridHelper.ApplyModernStyle(_gridRoomFees);
        KDataGridHelper.ApplyModernStyle(_gridUtility);
    }
}

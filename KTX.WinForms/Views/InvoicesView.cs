using KTX.Domain.DTOs;
using KTX.Domain.Enums;
using KTX.Infrastructure.VietQR;
using KTX.Services.Configuration;
using KTX.Services.Finance;
using KTX.UI.Controls;
using KTX.UI.Helpers;
using KTX.UI.Themes;

namespace KTX.WinForms.Views;

public class InvoicesView : UserControl, IThemable
{
    private readonly ITieredBillingService _billingService;
    private readonly IConfigurationService _configService;

    private readonly ComboBox _cboMonth;
    private readonly ComboBox _cboYear;
    private readonly KTextBox _txtSearch;
    private readonly KButton _btnGenInvoices;
    private readonly KButton _btnGenRoomFees;
    private readonly KButton _btnCashPay;

    private readonly TabControl _tabMain;
    private readonly TabPage _tabUtility;
    private readonly TabPage _tabRoomFee;

    private readonly DataGridView _gridUtility;
    private readonly DataGridView _gridRoomFee;

    // Right QR Panel
    private readonly Panel _panelQr;
    private readonly PictureBox _picQr;
    private readonly Label _lblQrTitle;
    private readonly Label _lblQrDetails;

    private List<InvoiceDto> _invoices = new();
    private List<RoomFeeDto> _roomFees = new();
    private SystemConfigDto _config = new();
    private bool _isInitialized;
    private readonly System.Windows.Forms.Timer _searchTimer;

    public InvoicesView(ITieredBillingService billingService, IConfigurationService configService)
    {
        _billingService = billingService;
        _configService = configService;

        Dock = DockStyle.Fill;
        DoubleBuffered = true;

        _searchTimer = new System.Windows.Forms.Timer { Interval = 250 };
        _searchTimer.Tick += async (s, e) =>
        {
            _searchTimer.Stop();
            await LoadDataAsync();
        };

        // Top Bar (FlowLayoutPanel: zero overlap & zero truncated buttons!)
        var topBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 56,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 8, 0, 8)
        };

        _cboMonth = new ComboBox { Width = 64, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 4, 8, 0), Font = new Font("Segoe UI", 9.5F) };
        for (int m = 1; m <= 12; m++) _cboMonth.Items.Add(m);
        _cboMonth.SelectedItem = DateTime.Now.Month;
        _cboMonth.SelectedIndexChanged += async (s, e) => { if (_isInitialized) await LoadDataAsync(); };

        _cboYear = new ComboBox { Width = 78, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 4, 10, 0), Font = new Font("Segoe UI", 9.5F) };
        for (int y = 2024; y <= 2028; y++) _cboYear.Items.Add(y);
        _cboYear.SelectedItem = DateTime.Now.Year;
        _cboYear.SelectedIndexChanged += async (s, e) => { if (_isInitialized) await LoadDataAsync(); };

        _txtSearch = new KTextBox { Placeholder = "Tìm KTXDN, KTXTP, MSSV, Phòng...", Size = new Size(250, 38), Margin = new Padding(0, 0, 10, 0) };
        _txtSearch.InnerTextBox.TextChanged += (s, e) =>
        {
            if (!_isInitialized) return;
            _searchTimer.Stop();
            _searchTimer.Start();
        };

        _btnGenInvoices = new KButton
        {
            Text = "Lập HĐ Điện Nước",
            Style = KButtonStyle.Primary,
            Size = new Size(165, 38),
            Margin = new Padding(0, 0, 8, 0)
        };
        _btnGenInvoices.Click += async (s, e) => await GenerateInvoicesAsync();

        _btnGenRoomFees = new KButton
        {
            Text = "Lập Tiền Phòng",
            Style = KButtonStyle.Secondary,
            Size = new Size(145, 38),
            Margin = new Padding(0, 0, 8, 0)
        };
        _btnGenRoomFees.Click += async (s, e) => await GenerateRoomFeesAsync();

        _btnCashPay = new KButton
        {
            Text = "Thu Tiền Mặt",
            Style = KButtonStyle.Secondary,
            Size = new Size(130, 38),
            Margin = new Padding(0)
        };
        _btnCashPay.Click += async (s, e) => await HandleCashPaymentAsync();

        topBar.Controls.AddRange(new Control[] { _cboMonth, _cboYear, _txtSearch, _btnGenInvoices, _btnGenRoomFees, _btnCashPay });

        // Right QR Panel
        _panelQr = new Panel
        {
            Dock = DockStyle.Right,
            Width = 310,
            Padding = new Padding(12),
            BackColor = ThemeManager.Current.Surface
        };

        _lblQrTitle = new Label
        {
            Text = "VIETQR CHUẨN EMVCO",
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = ThemeManager.Current.Primary,
            Dock = DockStyle.Top,
            Height = 28,
            TextAlign = ContentAlignment.MiddleCenter
        };

        _picQr = new PictureBox
        {
            Size = new Size(210, 210),
            SizeMode = PictureBoxSizeMode.Zoom,
            Location = new Point(50, 40)
        };

        _lblQrDetails = new Label
        {
            Text = "Chọn một hóa đơn trong danh sách để hiển thị mã VietQR động.",
            Font = new Font("Segoe UI", 9F),
            ForeColor = ThemeManager.Current.TextSecondary,
            Location = new Point(12, 260),
            Size = new Size(286, 270),
            TextAlign = ContentAlignment.TopCenter
        };

        _panelQr.Controls.AddRange(new Control[] { _lblQrTitle, _picQr, _lblQrDetails });

        // Main TabControl
        _tabMain = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5F) };
        _tabUtility = new TabPage("Hóa đơn điện nước (theo phòng)");
        _tabRoomFee = new TabPage("Khoản thu tiền phòng (theo hợp đồng)");

        _gridUtility = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, ScrollBars = ScrollBars.Both };
        KDataGridHelper.ApplyModernStyle(_gridUtility, autoFill: false);
        SetupUtilityColumns();
        _gridUtility.SelectionChanged += (s, e) => OnUtilityInvoiceSelected();

        _gridRoomFee = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, ScrollBars = ScrollBars.Both };
        KDataGridHelper.ApplyModernStyle(_gridRoomFee, autoFill: false);
        SetupRoomFeeColumns();
        _gridRoomFee.SelectionChanged += (s, e) => OnRoomFeeSelected();

        _tabUtility.Controls.Add(_gridUtility);
        _tabRoomFee.Controls.Add(_gridRoomFee);

        _tabMain.TabPages.Add(_tabUtility);
        _tabMain.TabPages.Add(_tabRoomFee);

        Controls.Add(_tabMain);
        Controls.Add(_panelQr);
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

    private void SetupUtilityColumns()
    {
        _gridUtility.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mã hóa đơn", DataPropertyName = "MaThanhToan", Width = 148 });
        _gridUtility.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Phòng", DataPropertyName = "MaPhong", Width = 78 });
        _gridUtility.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tòa", DataPropertyName = "ToaTen", Width = 68 });
        _gridUtility.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Kỳ HĐ", DataPropertyName = "KyHoaDon", Width = 78 });
        _gridUtility.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Định mức", DataPropertyName = "DinhMucText", Width = 96 });
        _gridUtility.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tổng tiền", DataPropertyName = "TongTienText", Width = 108 });
        _gridUtility.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Đã thu", DataPropertyName = "DaThuText", Width = 104 });
        _gridUtility.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Còn nợ", DataPropertyName = "ConNoText", Width = 104 });
        _gridUtility.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", DataPropertyName = "TrangThaiText", Width = 140 });
    }

    private void SetupRoomFeeColumns()
    {
        _gridRoomFee.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mã khoản thu", DataPropertyName = "MaThanhToan", Width = 148 });
        _gridRoomFee.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "MSSV", DataPropertyName = "MSSV", Width = 110 });
        _gridRoomFee.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Họ và tên", DataPropertyName = "HoTen", Width = 160 });
        _gridRoomFee.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Phòng", DataPropertyName = "MaPhong", Width = 78 });
        _gridRoomFee.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Thời hạn", DataPropertyName = "ThoiHanText", Width = 148 });
        _gridRoomFee.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Phải thu", DataPropertyName = "PhaiThuText", Width = 100 });
        _gridRoomFee.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Đã thu", DataPropertyName = "DaThuText", Width = 100 });
        _gridRoomFee.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Còn nợ", DataPropertyName = "ConNoText", Width = 100 });
        _gridRoomFee.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", DataPropertyName = "TrangThaiText", Width = 140 });
    }

    private async Task InitializeAsync()
    {
        _config = await _configService.GetSystemConfigAsync();
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        int month = (int)(_cboMonth.SelectedItem ?? DateTime.Now.Month);
        int year = (int)(_cboYear.SelectedItem ?? DateTime.Now.Year);
        string? search = _txtSearch.Text;

        _invoices = await _billingService.GetInvoicesAsync(month, year, search: search);
        _gridUtility.AutoGenerateColumns = false;
        _gridUtility.DataSource = _invoices.Select(i => new
        {
            i.Id,
            i.MaThanhToan,
            i.MaPhong,
            i.ToaTen,
            KyHoaDon = $"{i.Thang}/{i.Nam}",
            DinhMucText = $"{i.SoNguoiTinhDinhMuc} người",
            TongTienText = $"{i.TongTien:N0} đ",
            DaThuText = $"{i.DaThu:N0} đ",
            ConNoText = $"{i.ConNo:N0} đ",
            TrangThaiText = i.TrangThai == TrangThaiHoaDon.DaThanhToan ? "● Đã thanh toán" :
                           i.TrangThai == TrangThaiHoaDon.ThanhToanMotPhan ? "● Một phần" : "● Chưa thanh toán"
        }).ToList();

        // Tự động hiển thị VietQR cho hóa đơn chưa thanh toán đầu tiên
        var firstUnpaidIdx = _invoices.FindIndex(i => i.ConNo > 0);
        if (firstUnpaidIdx >= 0 && firstUnpaidIdx < _gridUtility.Rows.Count)
        {
            _gridUtility.ClearSelection();
            _gridUtility.Rows[firstUnpaidIdx].Selected = true;
            _gridUtility.CurrentCell = _gridUtility.Rows[firstUnpaidIdx].Cells[0];
            OnUtilityInvoiceSelected();
        }

        _roomFees = await _billingService.GetRoomFeesAsync(search: search);
        _gridRoomFee.AutoGenerateColumns = false;
        _gridRoomFee.DataSource = _roomFees.Select(f => new
        {
            f.Id,
            f.MaThanhToan,
            f.MSSV,
            f.HoTen,
            f.MaPhong,
            ThoiHanText = $"{f.TuNgay:dd/MM} - {f.DenNgay:dd/MM/yyyy}",
            PhaiThuText = $"{f.PhaiThu:N0} đ",
            DaThuText = $"{f.DaThu:N0} đ",
            ConNoText = $"{f.ConNo:N0} đ",
            TrangThaiText = f.TrangThai == TrangThaiHoaDon.DaThanhToan ? "● Đã thanh toán" :
                           f.TrangThai == TrangThaiHoaDon.ThanhToanMotPhan ? "● Một phần" : "● Chưa thanh toán"
        }).ToList();
    }

    private void OnUtilityInvoiceSelected()
    {
        if (_gridUtility.CurrentRow?.Index >= 0 && _gridUtility.CurrentRow.Index < _invoices.Count)
        {
            var inv = _invoices[_gridUtility.CurrentRow.Index];
            ShowQr(inv.VietQrPayload, inv.MaThanhToan, inv.ConNo, $"Phòng {inv.MaPhong} ({inv.ToaTen})", $"HĐ Điện Nước Tháng {inv.Thang}/{inv.Nam}");
        }
    }

    private void OnRoomFeeSelected()
    {
        if (_gridRoomFee.CurrentRow?.Index >= 0 && _gridRoomFee.CurrentRow.Index < _roomFees.Count)
        {
            var fee = _roomFees[_gridRoomFee.CurrentRow.Index];
            ShowQr(fee.VietQrPayload, fee.MaThanhToan, fee.ConNo, $"{fee.HoTen} ({fee.MSSV})", $"Tiền phòng ({fee.TuNgay:dd/MM} - {fee.DenNgay:dd/MM/yyyy})");
        }
    }

    private void ShowQr(string payload, string code, decimal amount, string target, string description)
    {
        try
        {
            if (amount <= 0)
            {
                _picQr.Image = null;
                _lblQrDetails.Text = $"✓ Hóa đơn {code}\n{target}\n\nĐÃ THANH TOÁN ĐỦ TOÀN BỘ!";
                return;
            }

            var bmp = VietQRHelper.GenerateQrBitmap(payload, 5);
            _picQr.Image = bmp;

            _lblQrDetails.Text =
                $"MÃ THANH TOÁN: {code}\n" +
                $"Đối tượng: {target}\n" +
                $"Khoản thu: {description}\n\n" +
                $"SỐ TIỀN CẦN THANH TOÁN:\n" +
                $"{amount:N0} VNĐ\n\n" +
                $"Ngân hàng: {_config.BankName} (BIN: {_config.BankBin})\n" +
                $"Số TK: {_config.BankAccountNumber}\n" +
                $"Chủ TK: {_config.BankAccountHolder}";
        }
        catch (Exception ex)
        {
            _lblQrDetails.Text = $"Lỗi tạo mã QR: {ex.Message}";
        }
    }

    private async Task GenerateInvoicesAsync()
    {
        int month = (int)(_cboMonth.SelectedItem ?? DateTime.Now.Month);
        int year = (int)(_cboYear.SelectedItem ?? DateTime.Now.Year);

        var confirm = MessageBox.Show($"Xác nhận tính toán lũy tiến bậc thang và lập hóa đơn điện nước tháng {month}/{year} cho tất cả các phòng?",
            "Lập hóa đơn", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (confirm == DialogResult.Yes)
        {
            try
            {
                var list = await _billingService.GenerateMonthlyInvoicesAsync(month, year);
                KToast.Show(FindForm() ?? new Form(), $"Đã lập thành công {list.Count} hóa đơn điện nước!", ToastType.Success);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lập hóa đơn:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private async Task GenerateRoomFeesAsync()
    {
        var start = DateTime.Now.Date;
        var end = start.AddMonths(5);

        var confirm = MessageBox.Show($"Xác nhận lập khoản thu tiền phòng học kỳ (5 tháng: {start:dd/MM/yyyy} - {end:dd/MM/yyyy}) cho tất cả các sinh viên đang lưu trú?",
            "Lập khoản thu tiền phòng", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (confirm == DialogResult.Yes)
        {
            try
            {
                var list = await _billingService.GenerateRoomFeesAsync(start, end);
                KToast.Show(FindForm() ?? new Form(), $"Đã lập thành công {list.Count} khoản thu tiền phòng!", ToastType.Success);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lập khoản thu:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private async Task HandleCashPaymentAsync()
    {
        if (_tabMain.SelectedTab == _tabUtility)
        {
            if (_gridUtility.CurrentRow?.Index >= 0 && _gridUtility.CurrentRow.Index < _invoices.Count)
            {
                var inv = _invoices[_gridUtility.CurrentRow.Index];
                if (inv.ConNo <= 0)
                {
                    MessageBox.Show("Hóa đơn này đã được thanh toán đầy đủ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string prompt = Microsoft.VisualBasic.Interaction.InputBox(
                    $"Thu tiền mặt HĐ điện nước {inv.MaThanhToan} (Phòng {inv.MaPhong}):\nSố tiền còn nợ: {inv.ConNo:N0} đ\n\nNhập số tiền thu:",
                    "Thu tiền mặt",
                    inv.ConNo.ToString("F0"));

                if (decimal.TryParse(prompt, out decimal amt) && amt > 0)
                {
                    var ok = await _billingService.RecordCashPaymentAsync(inv.Id, null, amt);
                    if (ok)
                    {
                        KToast.Show(FindForm() ?? new Form(), $"Đã thu tiền mặt {amt:N0} đ thành công!", ToastType.Success);
                        await LoadDataAsync();
                    }
                }
            }
        }
        else
        {
            if (_gridRoomFee.CurrentRow?.Index >= 0 && _gridRoomFee.CurrentRow.Index < _roomFees.Count)
            {
                var fee = _roomFees[_gridRoomFee.CurrentRow.Index];
                if (fee.ConNo <= 0)
                {
                    MessageBox.Show("Khoản thu này đã được thanh toán đầy đủ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string prompt = Microsoft.VisualBasic.Interaction.InputBox(
                    $"Thu tiền mặt Tiền phòng {fee.MaThanhToan} (SV {fee.HoTen}):\nSố tiền còn nợ: {fee.ConNo:N0} đ\n\nNhập số tiền thu:",
                    "Thu tiền mặt",
                    fee.ConNo.ToString("F0"));

                if (decimal.TryParse(prompt, out decimal amt) && amt > 0)
                {
                    var ok = await _billingService.RecordCashPaymentAsync(null, fee.Id, amt);
                    if (ok)
                    {
                        KToast.Show(FindForm() ?? new Form(), $"Đã thu tiền mặt {amt:N0} đ thành công!", ToastType.Success);
                        await LoadDataAsync();
                    }
                }
            }
        }
    }

    public void ApplyTheme(Theme theme)
    {
        BackColor = theme.Surface;
        _panelQr.BackColor = theme.SurfaceCard;
        _lblQrTitle.ForeColor = theme.Primary;
        KDataGridHelper.ApplyModernStyle(_gridUtility, autoFill: false);
        KDataGridHelper.ApplyModernStyle(_gridRoomFee, autoFill: false);
    }
}

using System.Drawing;
using System.IO;
using System.Windows.Forms;
using KTX.Domain.DTOs;
using KTX.Services.Contracts;
using KTX.Services.Students;
using KTX.UI.Controls;
using KTX.UI.Helpers;
using KTX.UI.Themes;

namespace KTX.WinForms.Views;

public class StudentHomeView : UserControl, IThemable
{
    private readonly IStudentPortalService _portalService;
    private readonly IContractService _contractService;
    private readonly string _mssv;
    private readonly Action<string>? _onNavigate;

    private readonly TableLayoutPanel _pnlKpi;
    private readonly KStatCard _cardRoom;
    private readonly KStatCard _cardContract;
    private readonly KStatCard _cardRoomDebt;
    private readonly KStatCard _cardUtilDebt;

    private readonly TableLayoutPanel _pnlBody;
    private readonly KCard _cardProfile;
    private readonly KCard _cardRoommates;
    private readonly Label _lblProfileInfo;
    private readonly KButton _btnPrintPdf;
    private readonly KButton _btnGoBills;
    private readonly KButton _btnGoMaintenance;
    private readonly DataGridView _gridRoommates;
    private readonly Label _lblAssets;

    private StudentPortalOverviewDto? _overview;
    private bool _isLoaded;

    public StudentHomeView(
        IStudentPortalService portalService,
        IContractService contractService,
        string mssv,
        Action<string>? onNavigate = null)
    {
        _portalService = portalService;
        _contractService = contractService;
        _mssv = mssv;
        _onNavigate = onNavigate;

        Dock = DockStyle.Fill;
        DoubleBuffered = true;
        AutoScroll = true;
        Padding = new Padding(4);

        // 1. KPI Row (4 equal columns)
        _pnlKpi = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 116,
            ColumnCount = 4,
            RowCount = 1,
            Padding = new Padding(0, 0, 0, 10)
        };
        for (int i = 0; i < 4; i++)
            _pnlKpi.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

        _cardRoom = new KStatCard { Title = "Phòng đang lưu trú", Value = "...", TrendText = "● Đang tải...", IsPositiveTrend = true, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 12, 0) };
        _cardContract = new KStatCard { Title = "Thời hạn hợp đồng", Value = "...", TrendText = "● Đang tải...", IsPositiveTrend = true, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 12, 0) };
        _cardRoomDebt = new KStatCard { Title = "Công nợ tiền phòng", Value = "...", TrendText = "● Đang tải...", IsPositiveTrend = true, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 12, 0) };
        _cardUtilDebt = new KStatCard { Title = "Điện nước phòng chưa đóng", Value = "...", TrendText = "● Đang tải...", IsPositiveTrend = true, Dock = DockStyle.Fill, Margin = new Padding(0) };

        _pnlKpi.Controls.Add(_cardRoom, 0, 0);
        _pnlKpi.Controls.Add(_cardContract, 1, 0);
        _pnlKpi.Controls.Add(_cardRoomDebt, 2, 0);
        _pnlKpi.Controls.Add(_cardUtilDebt, 3, 0);

        // 2. Main 2-column body
        _pnlBody = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(0, 6, 0, 0)
        };
        _pnlBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46f));
        _pnlBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54f));

        // Left: Student profile + contract + actions
        _cardProfile = new KCard
        {
            Title = "Thông tin Sinh viên & Hợp đồng nội trú",
            Subtitle = "Hồ sơ cá nhân và quyền lợi lưu trú tại Ký túc xá",
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 12, 0),
            Padding = new Padding(16, 66, 16, 16)
        };

        var pnlProfileContent = new Panel { Dock = DockStyle.Fill };

        _lblProfileInfo = new Label
        {
            Text = "Đang tải hồ sơ sinh viên...",
            Font = UIHelper.GetFont(9.5f),
            Dock = DockStyle.Fill,
            AutoEllipsis = false
        };

        var pnlActions = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 46,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 6, 0, 0)
        };

        _btnPrintPdf = new KButton { Text = "In Hợp đồng PDF", Style = KButtonStyle.Primary, Font = UIHelper.GetFont(8.8f, FontStyle.Bold), Size = new Size(148, 36), Margin = new Padding(0, 0, 8, 0) };
        _btnPrintPdf.Click += async (s, e) => await PrintContractPdfAsync();

        _btnGoBills = new KButton { Text = "Thanh toán VietQR", Style = KButtonStyle.Secondary, Font = UIHelper.GetFont(8.8f, FontStyle.Bold), Size = new Size(152, 36), Margin = new Padding(0, 0, 8, 0) };
        _btnGoBills.Click += (s, e) => _onNavigate?.Invoke("StudentBills");

        _btnGoMaintenance = new KButton { Text = "Báo hỏng", Style = KButtonStyle.Secondary, Font = UIHelper.GetFont(8.8f, FontStyle.Bold), Size = new Size(96, 36), Margin = new Padding(0) };
        _btnGoMaintenance.Click += (s, e) => _onNavigate?.Invoke("StudentMaintenance");

        pnlActions.Controls.AddRange(new Control[] { _btnPrintPdf, _btnGoBills, _btnGoMaintenance });
        pnlProfileContent.Controls.Add(_lblProfileInfo);
        pnlProfileContent.Controls.Add(pnlActions);
        _cardProfile.Controls.Add(pnlProfileContent);

        // Right: Roommates + Room Assets
        _cardRoommates = new KCard
        {
            Title = "Thành viên cùng phòng & Tài sản bàn giao",
            Subtitle = "Danh sách sinh viên đang lưu trú cùng phòng và trang thiết bị hiện có",
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            Padding = new Padding(16, 66, 16, 16)
        };

        _gridRoommates = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        KDataGridHelper.ApplyModernStyle(_gridRoommates);
        _gridRoommates.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "MSSV", DataPropertyName = "MSSV", FillWeight = 95 });
        _gridRoommates.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Họ và tên", DataPropertyName = "HoTen", FillWeight = 165 });
        _gridRoommates.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Khoa", DataPropertyName = "Khoa", FillWeight = 135 });
        _gridRoommates.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Khóa", DataPropertyName = "KhoaHocText", FillWeight = 55 });
        _gridRoommates.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Số điện thoại", DataPropertyName = "SoDienThoai", FillWeight = 100 });

        _lblAssets = new Label
        {
            Text = "Trang thiết bị bàn giao: Đang tải...",
            Font = UIHelper.GetFont(9f, FontStyle.Regular),
            Dock = DockStyle.Bottom,
            Height = 76,
            Padding = new Padding(0, 10, 0, 0)
        };

        _cardRoommates.Controls.Add(_gridRoommates);
        _cardRoommates.Controls.Add(_lblAssets);

        _pnlBody.Controls.Add(_cardProfile, 0, 0);
        _pnlBody.Controls.Add(_cardRoommates, 1, 0);

        Controls.Add(_pnlBody);
        Controls.Add(_pnlKpi);

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
            _overview = await _portalService.GetOverviewAsync(_mssv);
            if (_overview == null)
            {
                _lblProfileInfo.Text = $"Không tìm thấy hồ sơ sinh viên với MSSV: {_mssv}.";
                return;
            }

            _cardRoom.Value = _overview.MaPhong;
            _cardRoom.TrendText = _overview.PhongId.HasValue
                ? $"● {_overview.TenToa} · Tầng {_overview.Tang} · {_overview.TenLoaiPhong}"
                : "● Chưa được phân phòng";
            _cardRoom.IsPositiveTrend = _overview.PhongId.HasValue;

            _cardContract.Value = _overview.ActiveContractId.HasValue ? $"Còn {_overview.SoNgayConLai} ngày" : "Chưa có HĐ";
            _cardContract.TrendText = _overview.NgayKetThuc.HasValue
                ? $"● Hết hạn: {_overview.NgayKetThuc.Value:dd/MM/yyyy}"
                : "● Liên hệ Ban quản lý KTX";
            _cardContract.IsPositiveTrend = _overview.SoNgayConLai > 30;

            _cardRoomDebt.Value = $"{_overview.TongNoTienPhong:N0} đ";
            _cardRoomDebt.TrendText = _overview.TongNoTienPhong == 0 ? "● Đã hoàn thành tiền phòng" : "▲ Cần thanh toán qua VietQR";
            _cardRoomDebt.IsPositiveTrend = _overview.TongNoTienPhong == 0;

            _cardUtilDebt.Value = $"{_overview.TongNoDienNuocPhong:N0} đ";
            _cardUtilDebt.TrendText = _overview.TongNoDienNuocPhong == 0
                ? $"● Đã đóng đủ · {_overview.SoSuCoDangXuLy} sự cố đang mở"
                : $"▲ Còn nợ hóa đơn phòng · {_overview.SoSuCoDangXuLy} sự cố";
            _cardUtilDebt.IsPositiveTrend = _overview.TongNoDienNuocPhong == 0;

            decimal giaThucTe = _overview.GiaThang * (100 - _overview.MienGiamPhanTram) / 100m;
            _lblProfileInfo.Text =
                $"• MSSV: {_overview.MSSV}\n" +
                $"• Họ và tên: {_overview.HoTen} ({_overview.GioiTinhText})\n" +
                $"• Ngày sinh: {_overview.NgaySinh:dd/MM/yyyy}   |   CCCD: {_overview.CCCD}\n" +
                $"• Khoa / Khóa: {_overview.Khoa} (K{_overview.KhoaHoc})\n" +
                $"• Điện thoại: {_overview.SoDienThoai}   |   Email: {_overview.Email}\n" +
                $"• Quê quán: {_overview.QueQuan}\n" +
                $"• Diện ưu tiên: {_overview.DienUuTien}\n\n" +
                $"THÔNG TIN HỢP ĐỒNG HIỆN TẠI:\n" +
                $"• Phòng ở: {_overview.MaPhong} ({_overview.TenToa} - Tầng {_overview.Tang} - {_overview.TenLoaiPhong})\n" +
                $"• Thời hạn: {(_overview.NgayBatDau?.ToString("dd/MM/yyyy") ?? "---")} đến {(_overview.NgayKetThuc?.ToString("dd/MM/yyyy") ?? "---")}\n" +
                $"• Đơn giá phòng: {_overview.GiaThang:N0} đ/tháng (Miễn giảm: {_overview.MienGiamPhanTram}% → Thực đóng: {giaThucTe:N0} đ)\n" +
                $"• Tiền đặt cọc: {_overview.TienCoc:N0} đ";

            _btnPrintPdf.Enabled = _overview.ActiveContractId.HasValue;

            _gridRoommates.DataSource = _overview.Roommates.Select(r => new
            {
                r.MSSV,
                r.HoTen,
                r.Khoa,
                KhoaHocText = $"K{r.KhoaHoc}",
                r.SoDienThoai
            }).ToList();

            if (_overview.RoomAssets.Count > 0)
            {
                var assetsText = string.Join("   |   ", _overview.RoomAssets.Select(a => $"{a.TenTaiSan} (SL: {a.SoLuong} - {a.TinhTrang})"));
                _lblAssets.Text = $"Trang thiết bị bàn giao phòng {_overview.MaPhong}:\n{assetsText}";
            }
            else
            {
                _lblAssets.Text = "Chưa có dữ liệu tài sản bàn giao cho phòng này.";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể tải thông tin Cổng Sinh viên: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task PrintContractPdfAsync()
    {
        if (_overview?.ActiveContractId == null) return;

        using var sfd = new SaveFileDialog
        {
            Filter = "PDF Document (*.pdf)|*.pdf",
            FileName = $"HopDong_KTX_{_overview.MSSV}.pdf"
        };
        if (sfd.ShowDialog() != DialogResult.OK) return;

        try
        {
            var bytes = await _contractService.ExportContractPdfAsync(_overview.ActiveContractId.Value);
            await File.WriteAllBytesAsync(sfd.FileName, bytes);
            KToast.Show(FindForm() ?? new Form(), "Đã xuất Hợp đồng lưu trú PDF thành công!", ToastType.Success);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi xuất PDF: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public void ApplyTheme(Theme theme)
    {
        BackColor = theme.Background;
        _pnlKpi.BackColor = theme.Background;
        _pnlBody.BackColor = theme.Background;
        _lblProfileInfo.ForeColor = theme.TextPrimary;
        _lblAssets.ForeColor = theme.TextSecondary;
        KDataGridHelper.ApplyModernStyle(_gridRoommates);
    }
}

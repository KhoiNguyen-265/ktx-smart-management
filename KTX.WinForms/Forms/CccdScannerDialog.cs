using System.Drawing;
using System.IO;
using System.Windows.Forms;
using KTX.Infrastructure.Cccd;
using KTX.UI.Controls;
using KTX.UI.Helpers;
using KTX.UI.Themes;

namespace KTX.WinForms.Forms;

public class CccdScannerDialog : Form, IThemable
{
    private readonly TabControl _tabs;
    private readonly TabPage _tabText;
    private readonly TabPage _tabImage;

    // Tab 1: Text
    private readonly TextBox _txtRawInput;
    private readonly KButton _btnParseText;

    // Tab 2: Image
    private readonly KButton _btnBrowseImage;
    private readonly PictureBox _picPreview;

    // Output panel
    private readonly KCard _cardResult;
    private readonly Label _lblCccd;
    private readonly Label _lblHoTen;
    private readonly Label _lblNgaySinh;
    private readonly Label _lblGioiTinh;
    private readonly Label _lblDiaChi;
    private readonly Label _lblTinhThanh;

    private readonly KButton _btnApply;
    private readonly KButton _btnCancel;

    public CccdInfo? ScannedResult { get; private set; }

    public CccdScannerDialog()
    {
        Text = "📷 Quét & Trích xuất Mã QR Thẻ Căn Cước Công Dân (CCCD)";
        ClientSize = new Size(680, 520);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        _tabs = new TabControl
        {
            Dock = DockStyle.Top,
            Height = 160,
            Font = UIHelper.GetFont(9.5f, FontStyle.Regular)
        };

        // Tab 1: Máy quét mã vạch / Chuỗi văn bản
        _tabText = new TabPage("⚡ Quét từ máy Barcode / Dán chuỗi QR");
        var pnlText = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };

        var lblGuide = new Label
        {
            Text = "Dùng đầu đọc mã vạch quét trực tiếp vào ô bên dưới, hoặc dán chuỗi QR CCCD:",
            Location = new Point(12, 10),
            AutoSize = true
        };

        _txtRawInput = new TextBox
        {
            Location = new Point(12, 35),
            Width = 510,
            Height = 65,
            Multiline = true,
            Font = UIHelper.GetFont(9.5f)
        };
        _txtRawInput.TextChanged += (s, e) => ProcessRawInput();

        _btnParseText = new KButton
        {
            Text = "Phân tích",
            Style = KButtonStyle.Primary,
            Location = new Point(535, 35),
            Size = new Size(110, 65)
        };
        _btnParseText.Click += (s, e) => ProcessRawInput();

        pnlText.Controls.AddRange(new Control[] { lblGuide, _txtRawInput, _btnParseText });
        _tabText.Controls.Add(pnlText);

        // Tab 2: Tệp hình ảnh
        _tabImage = new TabPage("🖼️ Chọn ảnh chụp CCCD từ máy tính");
        var pnlImage = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };

        _btnBrowseImage = new KButton
        {
            Text = "📁 Chọn tệp ảnh (.png, .jpg)...",
            Style = KButtonStyle.Secondary,
            Location = new Point(12, 20),
            Size = new Size(220, 38)
        };
        _btnBrowseImage.Click += (s, e) => BrowseAndDecodeImage();

        _picPreview = new PictureBox
        {
            Location = new Point(250, 10),
            Size = new Size(390, 100),
            SizeMode = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.FixedSingle
        };

        pnlImage.Controls.AddRange(new Control[] { _btnBrowseImage, _picPreview });
        _tabImage.Controls.Add(pnlImage);

        _tabs.TabPages.AddRange(new[] { _tabText, _tabImage });

        // Result Card
        _cardResult = new KCard
        {
            Title = "Thông tin Căn Cước trích xuất được",
            Subtitle = "Hệ thống tự động chuẩn hóa họ tên và ánh xạ tỉnh thành quê quán",
            Dock = DockStyle.Fill,
            Padding = new Padding(16, 40, 16, 16)
        };

        var pnlDetails = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8)
        };

        _lblCccd = CreateInfoLabel("Số CCCD: (Chưa có dữ liệu)", 10);
        _lblHoTen = CreateInfoLabel("Họ và tên: ---", 40);
        _lblNgaySinh = CreateInfoLabel("Ngày sinh: ---", 70);
        _lblGioiTinh = CreateInfoLabel("Giới tính: ---", 100);
        _lblDiaChi = CreateInfoLabel("Nơi thường trú: ---", 130);
        _lblTinhThanh = CreateInfoLabel("Tỉnh/TP nhận diện: ---", 175);
        _lblTinhThanh.Font = UIHelper.GetFont(9.5f, FontStyle.Bold);
        _lblTinhThanh.ForeColor = Color.FromArgb(21, 101, 192);

        pnlDetails.Controls.AddRange(new Control[] { _lblCccd, _lblHoTen, _lblNgaySinh, _lblGioiTinh, _lblDiaChi, _lblTinhThanh });
        _cardResult.Controls.Add(pnlDetails);

        // Bottom Bar
        var bottomBar = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 55,
            Padding = new Padding(16, 8, 16, 8)
        };

        _btnApply = new KButton
        {
            Text = "✅ Điền vào thông tin Sinh viên",
            Style = KButtonStyle.Primary,
            Size = new Size(230, 36),
            Location = new Point(ClientSize.Width - 250, 8),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
            Enabled = false
        };
        _btnApply.Click += (s, e) =>
        {
            DialogResult = DialogResult.OK;
            Close();
        };

        _btnCancel = new KButton
        {
            Text = "Đóng",
            Style = KButtonStyle.Secondary,
            Size = new Size(90, 36),
            Location = new Point(ClientSize.Width - 350, 8),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right
        };
        _btnCancel.Click += (s, e) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        bottomBar.Controls.AddRange(new Control[] { _btnCancel, _btnApply });

        // Add to Form
        Controls.Add(_cardResult);
        Controls.Add(_tabs);
        Controls.Add(bottomBar);

        ApplyTheme(ThemeManager.Current);
    }

    private Label CreateInfoLabel(string initialText, int top)
    {
        return new Label
        {
            Text = initialText,
            Font = UIHelper.GetFont(9.5f, FontStyle.Regular),
            Location = new Point(12, top),
            Size = new Size(620, 26),
            AutoEllipsis = true
        };
    }

    private void ProcessRawInput()
    {
        string raw = _txtRawInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(raw)) return;

        var info = CccdScannerHelper.ParseCccdQrString(raw);
        DisplayResult(info);
    }

    private void BrowseAndDecodeImage()
    {
        using var ofd = new OpenFileDialog
        {
            Filter = "Ảnh CCCD (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp|All files (*.*)|*.*",
            Title = "Chọn ảnh chứa mã QR trên thẻ CCCD"
        };

        if (ofd.ShowDialog() != DialogResult.OK) return;

        try
        {
            using var bmp = new Bitmap(ofd.FileName);
            _picPreview.Image = new Bitmap(bmp);

            var info = CccdScannerHelper.DecodeQrFromBitmap(bmp);
            if (info != null)
            {
                DisplayResult(info);
            }
            else
            {
                MessageBox.Show("Không tìm thấy mã QR hợp lệ trên ảnh đã chọn. Vui lòng kiểm tra lại chất lượng hoặc độ sáng của ảnh.",
                    "Không thể đọc mã QR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi đọc file ảnh: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DisplayResult(CccdInfo? info)
    {
        if (info == null)
        {
            _btnApply.Enabled = false;
            return;
        }

        ScannedResult = info;

        _lblCccd.Text = $"Số CCCD: {info.CCCD}" + (string.IsNullOrEmpty(info.CMNDCu) ? "" : $" (CMND cũ: {info.CMNDCu})");
        _lblHoTen.Text = $"Họ và tên: {info.HoTen}";
        _lblNgaySinh.Text = $"Ngày sinh: {(info.NgaySinh.HasValue ? info.NgaySinh.Value.ToString("dd/MM/yyyy") : "---")}";
        _lblGioiTinh.Text = $"Giới tính: {(info.GioiTinh == Domain.Enums.GioiTinh.Nu ? "Nữ" : "Nam")}";
        _lblDiaChi.Text = $"Nơi thường trú: {info.DiaChi}";
        _lblTinhThanh.Text = $"Tỉnh/TP quê quán nhận diện: {info.TenTinhThanhNhanDien ?? "(Không xác định)"}";

        _btnApply.Enabled = true;
    }

    public void ApplyTheme(Theme theme)
    {
        BackColor = theme.Background;
        _tabs.BackColor = theme.Surface;
        _tabText.BackColor = theme.Surface;
        _tabImage.BackColor = theme.Surface;

        _lblCccd.ForeColor = theme.TextPrimary;
        _lblHoTen.ForeColor = theme.TextPrimary;
        _lblNgaySinh.ForeColor = theme.TextPrimary;
        _lblGioiTinh.ForeColor = theme.TextPrimary;
        _lblDiaChi.ForeColor = theme.TextPrimary;
    }
}

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using KTX.Services.Auth;
using KTX.UI.Controls;
using KTX.UI.Themes;
using KTX.WinForms.Services;

namespace KTX.WinForms.Forms;

public class LoginForm : Form, IThemable
{
    private readonly IAuthService _authService;
    private readonly IFormFactory _formFactory;

    private readonly KTextBox _txtUsername;
    private readonly KTextBox _txtPassword;
    private readonly KButton _btnLogin;
    private readonly Label _lblError;
    private readonly Panel _leftPanel;
    private readonly Panel _rightPanel;

    public LoginForm(IAuthService authService, IFormFactory formFactory)
    {
        _authService = authService;
        _formFactory = formFactory;

        Text = "Đăng nhập - KTX Smart Management";
        ClientSize = new Size(940, 540);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        DoubleBuffered = true;

        // Container Panels
        _leftPanel = new Panel
        {
            Dock = DockStyle.Left,
            Width = 440
        };
        _leftPanel.Paint += LeftPanel_Paint;

        _rightPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(48)
        };

        // Title and Subtitle in Right Panel
        var lblTitle = new Label
        {
            Text = "Chào mừng trở lại!",
            Font = UIHelper.GetFont(20f, FontStyle.Bold),
            Location = new Point(48, 52),
            AutoSize = true
        };

        var lblSubtitle = new Label
        {
            Text = "Vui lòng đăng nhập để tiếp tục làm việc với hệ thống",
            Font = UIHelper.GetFont(9.5f),
            Location = new Point(48, 94),
            AutoSize = true
        };

        var lblUserTag = new Label
        {
            Text = "Tên đăng nhập",
            Font = UIHelper.GetFont(9.5f, FontStyle.Bold),
            Location = new Point(48, 148),
            AutoSize = true
        };

        _txtUsername = new KTextBox
        {
            Placeholder = "Nhập tài khoản (vd: admin, quanly, ketoan)",
            Location = new Point(48, 174),
            Size = new Size(400, 44)
        };

        var lblPassTag = new Label
        {
            Text = "Mật khẩu",
            Font = UIHelper.GetFont(9.5f, FontStyle.Bold),
            Location = new Point(48, 236),
            AutoSize = true
        };

        _txtPassword = new KTextBox
        {
            Placeholder = "Nhập mật khẩu",
            IsPassword = true,
            Location = new Point(48, 262),
            Size = new Size(400, 44)
        };

        _lblError = new Label
        {
            ForeColor = Color.FromArgb(220, 38, 38),
            Font = UIHelper.GetFont(9f),
            Location = new Point(48, 314),
            Size = new Size(400, 32),
            Text = ""
        };

        _btnLogin = new KButton
        {
            Text = "Đăng nhập hệ thống",
            Location = new Point(48, 354),
            Size = new Size(400, 46),
            Style = KButtonStyle.Primary,
            BorderRadius = 8
        };
        _btnLogin.Click += async (s, e) => await HandleLoginAsync();

        // Quick login sample buttons for professors during evaluation
        var lblQuick = new Label
        {
            Text = "Đăng nhập nhanh tài khoản mẫu:",
            Font = UIHelper.GetFont(8.8f, FontStyle.Italic),
            Location = new Point(48, 422),
            AutoSize = true
        };

        var btnQuickAdmin = new KButton
        {
            Text = "Admin",
            Style = KButtonStyle.Secondary,
            Location = new Point(48, 448),
            Size = new Size(124, 36),
            Font = UIHelper.GetFont(9f, FontStyle.Bold)
        };
        btnQuickAdmin.Click += (s, e) =>
        {
            _txtUsername.Text = "admin";
            _txtPassword.Text = "admin123";
        };

        var btnQuickQuanLy = new KButton
        {
            Text = "Quản lý",
            Style = KButtonStyle.Secondary,
            Location = new Point(186, 448),
            Size = new Size(124, 36),
            Font = UIHelper.GetFont(9f, FontStyle.Bold)
        };
        btnQuickQuanLy.Click += (s, e) =>
        {
            _txtUsername.Text = "quanly";
            _txtPassword.Text = "quanly123";
        };

        var btnQuickKeToan = new KButton
        {
            Text = "Kế toán",
            Style = KButtonStyle.Secondary,
            Location = new Point(324, 448),
            Size = new Size(124, 36),
            Font = UIHelper.GetFont(9f, FontStyle.Bold)
        };
        btnQuickKeToan.Click += (s, e) =>
        {
            _txtUsername.Text = "ketoan";
            _txtPassword.Text = "ketoan123";
        };

        // Key down enter
        _txtUsername.InnerTextBox.KeyDown += async (s, e) => { if (e.KeyCode == Keys.Enter) await HandleLoginAsync(); };
        _txtPassword.InnerTextBox.KeyDown += async (s, e) => { if (e.KeyCode == Keys.Enter) await HandleLoginAsync(); };

        _rightPanel.Controls.AddRange(new Control[]
        {
            lblTitle, lblSubtitle,
            lblUserTag, _txtUsername,
            lblPassTag, _txtPassword,
            _lblError, _btnLogin,
            lblQuick, btnQuickAdmin, btnQuickQuanLy, btnQuickKeToan
        });

        Controls.Add(_rightPanel);
        Controls.Add(_leftPanel);

        ApplyTheme(ThemeManager.Current);
    }

    private void LeftPanel_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        // Elegant deep blue gradient background
        using var brush = new LinearGradientBrush(_leftPanel.ClientRectangle,
            Color.FromArgb(30, 64, 175), Color.FromArgb(15, 23, 42), 55f);
        g.FillRectangle(brush, _leftPanel.ClientRectangle);

        // Subtle modern geometric background circles
        using (var circleBrush = new SolidBrush(Color.FromArgb(22, 255, 255, 255)))
        {
            g.FillEllipse(circleBrush, -60, -60, 220, 220);
            g.FillEllipse(circleBrush, _leftPanel.Width - 140, _leftPanel.Height - 160, 240, 240);
        }

        int padX = 36;
        int contentWidth = _leftPanel.Width - padX * 2;

        // Branding content
        using var logoFont = UIHelper.GetFont(26f, FontStyle.Bold);
        using var subLogoFont = UIHelper.GetFont(11f, FontStyle.Regular);
        using var featureFont = UIHelper.GetFont(9.5f, FontStyle.Regular);
        using var footerFont = UIHelper.GetFont(8.5f, FontStyle.Italic);

        g.DrawString("KTX SMART", logoFont, Brushes.White, new RectangleF(padX, 56, contentWidth, 48));
        g.DrawString("Hệ thống Quản lý Ký túc xá Thông minh", subLogoFont, Brushes.LightSkyBlue, new RectangleF(padX, 108, contentWidth, 32));

        // Subtle divider line
        using (var linePen = new Pen(Color.FromArgb(70, 147, 197, 253), 1f))
        {
            g.DrawLine(linePen, padX, 152, padX + 90, 152);
        }

        // Decorative bullets with safe bounding rectangle so text never clips
        int bulletY = 180;
        string[] features =
        {
            "✦  Thuật toán Auto-Matching xếp phòng tự động",
            "✦  Tính tiền điện nước bậc thang theo định mức",
            "✦  Thanh toán VietQR động & Đối soát sao kê",
            "✦  Sơ đồ phòng trực quan, mượt mà chuẩn 60fps",
            "✦  Nhận diện QR Căn cước công dân gắn chip"
        };

        foreach (var f in features)
        {
            g.DrawString(f, featureFont, Brushes.WhiteSmoke, new RectangleF(padX, bulletY, contentWidth, 32));
            bulletY += 40;
        }

        g.DrawString("Phiên bản 1.0.0 · .NET 10 WinForms · SQL Server", footerFont, Brushes.LightGray, new RectangleF(padX, _leftPanel.Height - 48, contentWidth, 24));
    }

    private async Task HandleLoginAsync()
    {
        _lblError.Text = "";
        _btnLogin.IsLoading = true;
        _btnLogin.Enabled = false;

        try
        {
            var result = await _authService.LoginAsync(new LoginRequest
            {
                Username = _txtUsername.Text,
                Password = _txtPassword.Text
            });

            if (!result.Success)
            {
                _lblError.Text = result.Message;
                return;
            }

            // Login succeeded, open MainForm
            var mainForm = _formFactory.Create<MainForm>();
            mainForm.InitializeSession(result.Session!);
            mainForm.FormClosed += (s, e) => Close();
            Hide();
            mainForm.Show();
        }
        finally
        {
            _btnLogin.IsLoading = false;
            _btnLogin.Enabled = true;
        }
    }

    public void ApplyTheme(Theme theme)
    {
        _rightPanel.BackColor = theme.Surface;
        foreach (Control c in _rightPanel.Controls)
        {
            if (c is Label lbl && lbl != _lblError)
            {
                lbl.ForeColor = lbl.Font.Bold ? theme.TextPrimary : theme.TextSecondary;
            }
        }
    }
}

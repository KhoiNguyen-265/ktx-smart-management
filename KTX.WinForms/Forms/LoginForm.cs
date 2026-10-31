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
        ClientSize = new Size(880, 520);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        // Container Panels
        _leftPanel = new Panel
        {
            Dock = DockStyle.Left,
            Width = 380
        };
        _leftPanel.Paint += LeftPanel_Paint;

        _rightPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(40)
        };

        // Title and Subtitle in Right Panel
        var lblTitle = new Label
        {
            Text = "Chào mừng trở lại!",
            Font = UIHelper.GetFont(18f, FontStyle.Bold),
            Location = new Point(40, 50),
            AutoSize = true
        };

        var lblSubtitle = new Label
        {
            Text = "Vui lòng đăng nhập để tiếp tục làm việc với hệ thống",
            Font = UIHelper.GetFont(9.5f),
            Location = new Point(40, 88),
            AutoSize = true
        };

        var lblUserTag = new Label
        {
            Text = "Tên đăng nhập",
            Font = UIHelper.GetFont(9.5f, FontStyle.Bold),
            Location = new Point(40, 140),
            AutoSize = true
        };

        _txtUsername = new KTextBox
        {
            Placeholder = "Nhập tài khoản (vd: admin, quanly, ketoan)",
            Location = new Point(40, 166),
            Size = new Size(400, 42)
        };

        var lblPassTag = new Label
        {
            Text = "Mật khẩu",
            Font = UIHelper.GetFont(9.5f, FontStyle.Bold),
            Location = new Point(40, 226),
            AutoSize = true
        };

        _txtPassword = new KTextBox
        {
            Placeholder = "Nhập mật khẩu",
            IsPassword = true,
            Location = new Point(40, 252),
            Size = new Size(400, 42)
        };

        _lblError = new Label
        {
            ForeColor = Color.FromArgb(220, 38, 38),
            Font = UIHelper.GetFont(9f),
            Location = new Point(40, 304),
            Size = new Size(400, 36),
            Text = ""
        };

        _btnLogin = new KButton
        {
            Text = "Đăng nhập hệ thống",
            Location = new Point(40, 346),
            Size = new Size(400, 44),
            Style = KButtonStyle.Primary
        };
        _btnLogin.Click += async (s, e) => await HandleLoginAsync();

        // Quick login sample buttons for professors during evaluation
        var lblQuick = new Label
        {
            Text = "Đăng nhập nhanh tài khoản mẫu:",
            Font = UIHelper.GetFont(8.5f, FontStyle.Italic),
            Location = new Point(40, 410),
            AutoSize = true
        };

        var btnQuickAdmin = new KButton
        {
            Text = "Admin",
            Style = KButtonStyle.Secondary,
            Location = new Point(40, 436),
            Size = new Size(90, 32),
            Font = UIHelper.GetFont(8.5f, FontStyle.Bold)
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
            Location = new Point(140, 436),
            Size = new Size(90, 32),
            Font = UIHelper.GetFont(8.5f, FontStyle.Bold)
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
            Location = new Point(240, 436),
            Size = new Size(90, 32),
            Font = UIHelper.GetFont(8.5f, FontStyle.Bold)
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

        // Elegant deep blue gradient background
        using var brush = new LinearGradientBrush(_leftPanel.ClientRectangle,
            Color.FromArgb(29, 78, 216), Color.FromArgb(17, 24, 39), 60f);
        g.FillRectangle(brush, _leftPanel.ClientRectangle);

        // Branding content
        using var logoFont = UIHelper.GetFont(26f, FontStyle.Bold);
        using var subLogoFont = UIHelper.GetFont(12f, FontStyle.Regular);
        using var featureFont = UIHelper.GetFont(9.5f, FontStyle.Regular);
        using var footerFont = UIHelper.GetFont(8.5f, FontStyle.Italic);

        g.DrawString("KTX SMART", logoFont, Brushes.White, 36, 60);
        g.DrawString("Hệ thống Quản lý Ký túc xá Thông minh", subLogoFont, Brushes.LightSkyBlue, 36, 108);

        // Decorative bullets
        int bulletY = 180;
        string[] features =
        {
            "✦ Thuật toán Auto-Matching xếp phòng tự động",
            "✦ Tính tiền điện nước bậc thang theo định mức",
            "✦ Thanh toán VietQR động & Đối soát sao kê",
            "✦ Sơ đồ phòng trực quan, mượt mà chuẩn 60fps",
            "✦ Nhận diện QR Căn cước công dân gắn chip"
        };

        foreach (var f in features)
        {
            g.DrawString(f, featureFont, Brushes.WhiteSmoke, 36, bulletY);
            bulletY += 36;
        }

        g.DrawString("Phiên bản 1.0.0 · .NET 10 WinForms", footerFont, Brushes.LightGray, 36, 460);
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

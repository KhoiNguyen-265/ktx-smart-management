using System.Drawing;
using System.Windows.Forms;
using KTX.Services.Auth;
using KTX.UI.Controls;
using KTX.UI.Themes;
using Microsoft.Extensions.DependencyInjection;

namespace KTX.WinForms.Forms;

public class MainForm : Form, IThemable
{
    private UserSession? _session;
    private readonly IServiceProvider _serviceProvider;
    private readonly IAuthService _authService;
    private readonly KSidebar _sidebar;
    private readonly Panel _topbar;
    private readonly Panel _contentPanel;
    private readonly Label _lblBreadcrumb;
    private readonly Label _lblUser;
    private readonly KButton _btnCommandPalette;
    private readonly KButton _btnThemeToggle;
    private readonly KButton _btnLogout;

    private readonly FlowLayoutPanel _topbarRightPanel;
    private readonly Action _themeChangedHandler;

    public bool IsLogoutRequested { get; private set; }

    public MainForm(IServiceProvider serviceProvider, IAuthService authService)
    {
        _serviceProvider = serviceProvider;
        _authService = authService;

        Text = "KTX Smart Management - Hệ thống Quản lý Ký túc xá Thông minh";
        ClientSize = new Size(1340, 780);
        MinimumSize = new Size(1120, 680);
        StartPosition = FormStartPosition.CenterScreen;
        DoubleBuffered = true;

        // 1. Sidebar
        _sidebar = new KSidebar();
        _sidebar.MenuItemSelected += Sidebar_MenuItemSelected;

        // 2. Topbar
        _topbar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 64,
            Padding = new Padding(20, 0, 20, 0)
        };
        _topbar.Paint += (s, e) =>
        {
            using var pen = new Pen(ThemeManager.Current.Border, 1f);
            e.Graphics.DrawLine(pen, 0, _topbar.Height - 1, _topbar.Width, _topbar.Height - 1);
        };

        _lblBreadcrumb = new Label
        {
            Text = "Trang chủ / Tổng quan",
            Font = UIHelper.GetFont(11.5f, FontStyle.Bold),
            Dock = DockStyle.Fill,
            AutoSize = false,
            AutoEllipsis = true,
            UseMnemonic = false,
            TextAlign = ContentAlignment.MiddleLeft
        };

        _topbarRightPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 14, 0, 0)
        };

        _btnCommandPalette = new KButton
        {
            Text = "Tìm nhanh [Ctrl+K]",
            Style = KButtonStyle.Secondary,
            Size = new Size(178, 36),
            Margin = new Padding(0, 0, 10, 0)
        };
        _btnCommandPalette.Click += (s, e) => ShowCommandPalette();

        _btnThemeToggle = new KButton
        {
            Text = "Chế độ Tối",
            Style = KButtonStyle.Secondary,
            Size = new Size(115, 36),
            Margin = new Padding(0, 0, 14, 0)
        };
        _btnThemeToggle.Click += (s, e) =>
        {
            ThemeManager.ToggleTheme();
            ApplyThemeRecursively(this);
        };

        _lblUser = new Label
        {
            Text = "Đang tải...",
            Font = UIHelper.GetFont(9.5f, FontStyle.Bold),
            AutoSize = true,
            Padding = new Padding(4, 8, 10, 0),
            TextAlign = ContentAlignment.MiddleRight
        };

        _btnLogout = new KButton
        {
            Text = "Đăng xuất",
            Style = KButtonStyle.Danger,
            Size = new Size(98, 36),
            Font = UIHelper.GetFont(9f, FontStyle.Bold),
            Margin = new Padding(0)
        };
        _btnLogout.Click += async (s, e) => await HandleLogoutAsync();

        _topbarRightPanel.Controls.Add(_btnCommandPalette);
        _topbarRightPanel.Controls.Add(_btnThemeToggle);
        _topbarRightPanel.Controls.Add(_lblUser);
        _topbarRightPanel.Controls.Add(_btnLogout);

        // WinForms docks highest Z-index (added last) first: Right panel first, then Fill breadcrumb fills remaining left space
        _topbar.Controls.Add(_lblBreadcrumb);
        _topbar.Controls.Add(_topbarRightPanel);

        // 3. Content Panel
        _contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            AutoScroll = true
        };

        Controls.Add(_contentPanel);
        Controls.Add(_topbar);
        Controls.Add(_sidebar);

        ApplyTheme(ThemeManager.Current);
        _themeChangedHandler = () => ApplyTheme(ThemeManager.Current);
        ThemeManager.ThemeChanged += _themeChangedHandler;
        FormClosed += (s, e) => ThemeManager.ThemeChanged -= _themeChangedHandler;
    }

    public void InitializeSession(UserSession session)
    {
        _session = session;
        string roleDisplay = session.RoleName switch
        {
            "Admin" => "Quản trị viên",
            "QuanLy" => "Quản lý KTX",
            "KeToan" => "Kế toán",
            "SinhVien" => "Sinh viên KTX",
            _ => session.RoleName
        };

        _lblUser.Text = session.RoleName == "SinhVien" && !string.IsNullOrEmpty(session.LinkedMssv)
            ? $"{session.FullName} ({session.LinkedMssv} · {roleDisplay})"
            : $"{session.FullName} ({roleDisplay})";

        _sidebar.ConfigureForRole(session.RoleName);

        if (session.RoleName == "SinhVien")
        {
            Text = "KTX Smart Management - Cổng Thông Tin Sinh Viên Nội Trú";
            NavigateTo("StudentHome");
        }
        else
        {
            Text = "KTX Smart Management - Hệ thống Quản lý Ký túc xá Thông minh";
            NavigateTo("Dashboard");
        }
    }

    private async Task HandleLogoutAsync()
    {
        var confirm = MessageBox.Show(
            $"Bạn có chắc chắn muốn đăng xuất khỏi tài khoản \"{_session?.FullName ?? _session?.Username}\"?",
            "Xác nhận đăng xuất",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes) return;

        if (_session != null)
        {
            await _authService.LogoutAsync(_session);
        }

        IsLogoutRequested = true;
        Close();
    }

    private void Sidebar_MenuItemSelected(string key)
    {
        NavigateTo(key);
    }

    public void NavigateTo(string key)
    {
        foreach (Control old in _contentPanel.Controls.Cast<Control>().ToList())
        {
            _contentPanel.Controls.Remove(old);
            old.Dispose();
        }
        _sidebar.SelectedKey = key;

        string pageTitle = key switch
        {
            "StudentHome" => "Cổng Sinh viên / Phòng & Hợp đồng",
            "StudentBills" => "Cổng Sinh viên / Hóa đơn & VietQR",
            "StudentMaintenance" => "Cổng Sinh viên / Báo hỏng phòng",
            "Dashboard" => "Tổng quan",
            "Rooms" => "Tòa & Phòng",
            "Students" => "Quản lý Sinh viên",
            "Allocation" => "Xếp phòng tự động AI",
            "Contracts" => "Hợp đồng lưu trú",
            "Meters" => "Chỉ số Điện Nước",
            "Invoices" => "Hóa đơn & VietQR",
            "Reconcile" => "Đối soát Sao kê",
            "Maintenance" => "Sự cố & Sửa chữa",
            "Reports" => "Báo cáo Thống kê",
            "Settings" => "Cấu hình Hệ thống",
            _ => key
        };
        _lblBreadcrumb.Text = key.StartsWith("Student") ? pageTitle : $"Trang chủ / {pageTitle}";

        if (key == "StudentHome")
        {
            var mssv = _session?.LinkedMssv ?? _session?.Username ?? string.Empty;
            var view = new Views.StudentHomeView(
                _serviceProvider.GetRequiredService<KTX.Services.Students.IStudentPortalService>(),
                _serviceProvider.GetRequiredService<KTX.Services.Contracts.IContractService>(),
                mssv,
                NavigateTo);
            _contentPanel.Controls.Add(view);
            return;
        }

        if (key == "StudentBills")
        {
            var mssv = _session?.LinkedMssv ?? _session?.Username ?? string.Empty;
            var view = new Views.StudentBillsView(
                _serviceProvider.GetRequiredService<KTX.Services.Students.IStudentPortalService>(),
                _serviceProvider.GetRequiredService<KTX.Services.Configuration.IConfigurationService>(),
                mssv);
            _contentPanel.Controls.Add(view);
            return;
        }

        if (key == "StudentMaintenance")
        {
            var mssv = _session?.LinkedMssv ?? _session?.Username ?? string.Empty;
            var view = new Views.StudentMaintenanceView(
                _serviceProvider.GetRequiredService<KTX.Services.Students.IStudentPortalService>(),
                mssv);
            _contentPanel.Controls.Add(view);
            return;
        }

        if (key == "Dashboard")
        {
            var dash = new Views.DashboardView(_serviceProvider.GetRequiredService<KTX.Services.Dashboard.IDashboardService>(), NavigateTo);
            _contentPanel.Controls.Add(dash);
            return;
        }

        if (key == "Rooms")
        {
            var roomsView = _serviceProvider.GetRequiredService<Views.RoomsView>();
            _contentPanel.Controls.Add(roomsView);
            return;
        }

        if (key == "Students")
        {
            var studentsView = _serviceProvider.GetRequiredService<Views.StudentsView>();
            _contentPanel.Controls.Add(studentsView);
            return;
        }

        if (key == "Allocation")
        {
            var wizardView = _serviceProvider.GetRequiredService<Views.AllocationWizardView>();
            _contentPanel.Controls.Add(wizardView);
            return;
        }

        if (key == "Contracts")
        {
            var contractsView = _serviceProvider.GetRequiredService<Views.ContractsView>();
            _contentPanel.Controls.Add(contractsView);
            return;
        }

        if (key == "Meters")
        {
            var metersView = _serviceProvider.GetRequiredService<Views.UtilityMetersView>();
            _contentPanel.Controls.Add(metersView);
            return;
        }

        if (key == "Invoices")
        {
            var invoicesView = _serviceProvider.GetRequiredService<Views.InvoicesView>();
            _contentPanel.Controls.Add(invoicesView);
            return;
        }

        if (key == "Reconcile")
        {
            var reconcileView = _serviceProvider.GetRequiredService<Views.ReconciliationView>();
            _contentPanel.Controls.Add(reconcileView);
            return;
        }

        if (key == "Maintenance")
        {
            var maintenanceView = _serviceProvider.GetRequiredService<Views.MaintenanceView>();
            _contentPanel.Controls.Add(maintenanceView);
            return;
        }

        if (key == "Reports")
        {
            var reportsView = _serviceProvider.GetRequiredService<Views.ReportsView>();
            _contentPanel.Controls.Add(reportsView);
            return;
        }

        if (key == "Settings")
        {
            var settingsView = _serviceProvider.GetRequiredService<Views.SettingsView>();
            _contentPanel.Controls.Add(settingsView);
            return;
        }

        // Placeholder content card until specific feature UserControls are loaded
        var welcomeCard = new KCard
        {
            Title = $"Màn hình chức năng: {key}",
            Subtitle = "Mô-đun đang hoạt động trên nền tảng .NET 10 WinForms",
            Dock = DockStyle.Top,
            Height = 140
        };

        var statPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 120,
            Padding = new Padding(0, 16, 0, 0)
        };

        statPanel.Controls.Add(new KStatCard
        {
            Title = "Tổng số phòng KTX",
            Value = "120 phòng",
            TrendText = "▲ 95% đang hoạt động",
            IsPositiveTrend = true
        });

        statPanel.Controls.Add(new KStatCard
        {
            Title = "Sinh viên lưu trú",
            Value = "600 sinh viên",
            TrendText = "▲ 100% lấp đầy đợt 1",
            IsPositiveTrend = true
        });

        statPanel.Controls.Add(new KStatCard
        {
            Title = "Đơn đăng ký mới",
            Value = "500 đơn",
            TrendText = "● Đã sẵn sàng xếp phòng AI",
            IsPositiveTrend = true
        });

        statPanel.Controls.Add(new KStatCard
        {
            Title = "Doanh thu tháng 10",
            Value = "342,500,000 đ",
            TrendText = "▲ 8.4% so với tháng trước",
            IsPositiveTrend = true
        });

        _contentPanel.Controls.Add(statPanel);
        _contentPanel.Controls.Add(welcomeCard);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == (Keys.Control | Keys.K))
        {
            ShowCommandPalette();
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private async void ShowCommandPalette()
    {
        using var dlg = new CommandPaletteDialog(roleName: _session?.RoleName ?? "Admin");
        if (dlg.ShowDialog(this) == DialogResult.OK && dlg.SelectedCommand != null)
        {
            var cmd = dlg.SelectedCommand;
            if (cmd.ActionKey == "ToggleTheme")
            {
                ThemeManager.ToggleTheme();
                ApplyThemeRecursively(this);
            }
            else if (cmd.ActionKey == "Logout")
            {
                await HandleLogoutAsync();
            }
            else
            {
                NavigateTo(cmd.ActionKey);
            }
        }
    }

    public void ApplyTheme(Theme theme)
    {
        BackColor = theme.Background;
        _topbar.BackColor = theme.Surface;
        _topbarRightPanel.BackColor = theme.Surface;
        _contentPanel.BackColor = theme.Background;
        _lblBreadcrumb.ForeColor = theme.TextPrimary;
        _lblUser.ForeColor = theme.TextPrimary;
        _btnThemeToggle.Text = theme.Mode == ThemeMode.Dark ? "Chế độ Sáng" : "Chế độ Tối";
        _topbar.Invalidate();
    }

    private void ApplyThemeRecursively(Control root)
    {
        if (root is IThemable themable)
        {
            themable.ApplyTheme(ThemeManager.Current);
        }
        foreach (Control c in root.Controls)
        {
            ApplyThemeRecursively(c);
        }
    }
}

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
    private readonly KSidebar _sidebar;
    private readonly Panel _topbar;
    private readonly Panel _contentPanel;
    private readonly Label _lblBreadcrumb;
    private readonly Label _lblUser;
    private readonly KButton _btnCommandPalette;
    private readonly KButton _btnThemeToggle;

    public MainForm(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        Text = "KTX Smart Management - Hệ thống Quản lý Ký túc xá Thông minh";
        ClientSize = new Size(1280, 760);
        MinimumSize = new Size(1100, 680);
        StartPosition = FormStartPosition.CenterScreen;

        // 1. Sidebar
        _sidebar = new KSidebar();
        _sidebar.MenuItemSelected += Sidebar_MenuItemSelected;

        // 2. Topbar
        _topbar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 64,
            Padding = new Padding(24, 12, 24, 12)
        };

        _lblBreadcrumb = new Label
        {
            Text = "Tổng quan / Dashboard",
            Font = UIHelper.GetFont(12f, FontStyle.Bold),
            Location = new Point(24, 20),
            AutoSize = true
        };

        _btnCommandPalette = new KButton
        {
            Text = "Tìm nhanh [Ctrl+K]",
            Style = KButtonStyle.Secondary,
            Size = new Size(160, 34),
            Location = new Point(ClientSize.Width - 620, 15),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        _btnCommandPalette.Click += (s, e) => ShowCommandPalette();

        _btnThemeToggle = new KButton
        {
            Text = "Đổi giao diện",
            Style = KButtonStyle.Secondary,
            Size = new Size(125, 34),
            Location = new Point(ClientSize.Width - 445, 15),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
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
            Location = new Point(ClientSize.Width - 300, 22),
            Size = new Size(270, 24),
            TextAlign = ContentAlignment.MiddleRight,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };

        _topbar.Controls.Add(_lblBreadcrumb);
        _topbar.Controls.Add(_btnCommandPalette);
        _topbar.Controls.Add(_btnThemeToggle);
        _topbar.Controls.Add(_lblUser);

        // 3. Content Panel
        _contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            AutoScroll = true
        };

        Controls.Add(_contentPanel);
        Controls.Add(_topbar);
        Controls.Add(_sidebar);

        ApplyTheme(ThemeManager.Current);
        ThemeManager.ThemeChanged += () => ApplyTheme(ThemeManager.Current);
    }

    public void InitializeSession(UserSession session)
    {
        _session = session;
        _lblUser.Text = $"{session.FullName} ({session.RoleName})";
        NavigateTo("Dashboard");
    }

    private void Sidebar_MenuItemSelected(string key)
    {
        NavigateTo(key);
    }

    public void NavigateTo(string key)
    {
        _contentPanel.Controls.Clear();
        _sidebar.SelectedKey = key;
        _lblBreadcrumb.Text = $"Trang chủ / {_sidebar.SelectedKey}";

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

    private void ShowCommandPalette()
    {
        using var dlg = new CommandPaletteDialog();
        if (dlg.ShowDialog(this) == DialogResult.OK && dlg.SelectedCommand != null)
        {
            var cmd = dlg.SelectedCommand;
            if (cmd.ActionKey == "ToggleTheme")
            {
                ThemeManager.ToggleTheme();
                ApplyThemeRecursively(this);
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
        _contentPanel.BackColor = theme.Background;
        _lblBreadcrumb.ForeColor = theme.TextPrimary;
        _lblUser.ForeColor = theme.TextPrimary;
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

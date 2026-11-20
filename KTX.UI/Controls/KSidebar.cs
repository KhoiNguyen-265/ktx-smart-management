using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using KTX.UI.Themes;

namespace KTX.UI.Controls;

public class SidebarItem
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string IconText { get; set; } = string.Empty;
    public int RequiredRoleId { get; set; } // 0 = all, 1 = Admin only, 2 = QuanLy, 3 = KeToan
}

public class KSidebar : UserControl, IThemable
{
    private readonly List<SidebarItem> _items = new();
    private string _selectedKey = "Dashboard";
    private int _hoveredIndex = -1;
    private bool _isCollapsed = false;

    public event Action<string>? MenuItemSelected;

    public bool IsCollapsed
    {
        get => _isCollapsed;
        set
        {
            _isCollapsed = value;
            Width = _isCollapsed ? 70 : 240;
            Invalidate();
        }
    }

    public string SelectedKey
    {
        get => _selectedKey;
        set { _selectedKey = value; Invalidate(); }
    }

    public KSidebar()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);

        DoubleBuffered = true;
        Width = 240;
        Dock = DockStyle.Left;

        InitializeDefaultItems();
        ApplyTheme(ThemeManager.Current);
    }

    private static readonly bool HasMdl2 = FontFamily.Families.Any(f => f.Name.Equals("Segoe MDL2 Assets", StringComparison.OrdinalIgnoreCase));

    private void InitializeDefaultItems()
    {
        ConfigureForRole("Admin");
    }

    public void ConfigureForRole(string roleName)
    {
        _items.Clear();
        _hoveredIndex = -1;

        if (string.Equals(roleName, "SinhVien", StringComparison.OrdinalIgnoreCase))
        {
            AddItem("StudentHome", "Phòng & Hợp đồng", "\uE80F", "⌂");
            AddItem("StudentBills", "Hóa đơn & VietQR", "\uE8C7", "₫");
            AddItem("StudentMaintenance", "Báo hỏng phòng", "\uE90F", "⚙");
            _selectedKey = "StudentHome";
            Invalidate();
            return;
        }

        if (string.Equals(roleName, "KeToan", StringComparison.OrdinalIgnoreCase))
        {
            AddItem("Dashboard", "Tổng quan", "\uE9F9", "▦");
            AddItem("Contracts", "Hợp đồng", "\uE8A5", "≡");
            AddItem("Meters", "Chỉ số Điện Nước", "\uE945", "⚡");
            AddItem("Invoices", "Hóa đơn & VietQR", "\uE8C7", "₫");
            AddItem("Reconcile", "Đối soát Sao kê", "\uE777", "↺");
            AddItem("Reports", "Báo cáo Thống kê", "\uE9D9", "▲");
            _selectedKey = "Dashboard";
            Invalidate();
            return;
        }

        if (string.Equals(roleName, "QuanLy", StringComparison.OrdinalIgnoreCase))
        {
            AddItem("Dashboard", "Tổng quan", "\uE9F9", "▦");
            AddItem("Rooms", "Tòa & Phòng", "\uE825", "⊞");
            AddItem("Students", "Sinh viên", "\uE77B", "●");
            AddItem("Allocation", "Xếp phòng AI", "\uE8EC", "◈");
            AddItem("Contracts", "Hợp đồng", "\uE8A5", "≡");
            AddItem("Meters", "Chỉ số Điện Nước", "\uE945", "⚡");
            AddItem("Maintenance", "Sự cố & Sửa chữa", "\uE90F", "⚙");
            AddItem("Reports", "Báo cáo Thống kê", "\uE9D9", "▲");
            _selectedKey = "Dashboard";
            Invalidate();
            return;
        }

        // Admin: Toàn quyền 11 phân hệ
        AddItem("Dashboard", "Tổng quan", "\uE9F9", "▦");
        AddItem("Rooms", "Tòa & Phòng", "\uE825", "⊞");
        AddItem("Students", "Sinh viên", "\uE77B", "●");
        AddItem("Allocation", "Xếp phòng AI", "\uE8EC", "◈");
        AddItem("Contracts", "Hợp đồng", "\uE8A5", "≡");
        AddItem("Meters", "Chỉ số Điện Nước", "\uE945", "⚡");
        AddItem("Invoices", "Hóa đơn & VietQR", "\uE8C7", "₫");
        AddItem("Reconcile", "Đối soát Sao kê", "\uE777", "↺");
        AddItem("Maintenance", "Sự cố & Sửa chữa", "\uE90F", "⚙");
        AddItem("Reports", "Báo cáo Thống kê", "\uE9D9", "▲");
        AddItem("Settings", "Cấu hình Hệ thống", "\uE713", "⚙");
        _selectedKey = "Dashboard";
        Invalidate();
    }

    private void AddItem(string key, string title, string mdl2Glyph, string fallbackGlyph)
    {
        _items.Add(new SidebarItem
        {
            Key = key,
            Title = title,
            IconText = HasMdl2 ? mdl2Glyph : fallbackGlyph
        });
    }

    public void ApplyTheme(Theme theme)
    {
        BackColor = theme.Surface;
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        int itemHeight = 46;
        int startY = 80;

        int index = (e.Y - startY) / itemHeight;
        if (index >= 0 && index < _items.Count)
        {
            if (_hoveredIndex != index)
            {
                _hoveredIndex = index;
                Cursor = Cursors.Hand;
                Invalidate();
            }
        }
        else
        {
            if (_hoveredIndex != -1)
            {
                _hoveredIndex = -1;
                Cursor = Cursors.Default;
                Invalidate();
            }
        }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hoveredIndex = -1;
        Cursor = Cursors.Default;
        Invalidate();
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        int itemHeight = 46;
        int startY = 80;

        int index = (e.Y - startY) / itemHeight;
        if (index >= 0 && index < _items.Count)
        {
            var item = _items[index];
            _selectedKey = item.Key;
            MenuItemSelected?.Invoke(item.Key);
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var theme = ThemeManager.Current;

        // Background
        using (var bgBrush = new SolidBrush(theme.Surface))
        {
            g.FillRectangle(bgBrush, ClientRectangle);
        }

        // Right border line
        using (var borderPen = new Pen(theme.Border, 1f))
        {
            g.DrawLine(borderPen, Width - 1, 0, Width - 1, Height);
        }

        // App Logo / Title Header
        using var iconFont = HasMdl2
            ? new Font("Segoe MDL2 Assets", 12f, FontStyle.Regular)
            : UIHelper.GetFont(11.5f, FontStyle.Regular);
        using var logoFont = UIHelper.GetFont(13f, FontStyle.Bold);
        using var logoBrush = new SolidBrush(theme.Primary);
        string logoIcon = HasMdl2 ? "\uE80F" : "⌂";

        if (!_isCollapsed)
        {
            g.DrawString(logoIcon, iconFont, logoBrush, 20, 24);
            g.DrawString("KTX SMART", logoFont, logoBrush, 46, 24);
        }
        else
        {
            g.DrawString(logoIcon, iconFont, logoBrush, (Width - 20) / 2, 24);
        }

        // Menu Items
        int itemHeight = 46;
        int startY = 80;

        for (int i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            bool isSelected = item.Key == _selectedKey;
            bool isHovered = i == _hoveredIndex;

            int itemY = startY + i * itemHeight;
            var itemRect = new Rectangle(10, itemY, Width - 20, 40);

            if (isSelected)
            {
                using var path = UIHelper.CreateRoundedRectangle(itemRect, 8);
                using var activeBrush = new SolidBrush(theme.PrimaryLight);
                g.FillPath(activeBrush, path);

                // Active left bar
                using var barBrush = new SolidBrush(theme.Primary);
                g.FillRectangle(barBrush, 10, itemY + 8, 4, 24);
            }
            else if (isHovered)
            {
                using var path = UIHelper.CreateRoundedRectangle(itemRect, 8);
                using var hoverBrush = new SolidBrush(theme.SurfaceHover);
                g.FillPath(hoverBrush, path);
            }

            // Draw Icon
            int iconX = _isCollapsed ? (Width - 20) / 2 : 24;
            var iconColor = isSelected ? theme.Primary : (isHovered ? theme.TextPrimary : theme.TextSecondary);
            using var iconBrush = new SolidBrush(iconColor);
            g.DrawString(item.IconText, iconFont, iconBrush, iconX, itemY + 10);

            // Draw Title if expanded
            if (!_isCollapsed)
            {
                var textColor = isSelected ? theme.Primary : (isHovered ? theme.TextPrimary : theme.TextSecondary);
                using var textBrush = new SolidBrush(textColor);
                using var textFont = UIHelper.GetFont(9.5f, isSelected ? FontStyle.Bold : FontStyle.Regular);
                g.DrawString(item.Title, textFont, textBrush, 58, itemY + 10);
            }
        }
    }
}

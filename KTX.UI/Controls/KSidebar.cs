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

    private void InitializeDefaultItems()
    {
        _items.Add(new SidebarItem { Key = "Dashboard", Title = "Tổng quan", IconText = "📊" });
        _items.Add(new SidebarItem { Key = "Rooms", Title = "Tòa & Phòng", IconText = "🏢" });
        _items.Add(new SidebarItem { Key = "Students", Title = "Sinh viên", IconText = "👥" });
        _items.Add(new SidebarItem { Key = "Allocation", Title = "Xếp phòng AI", IconText = "🧩" });
        _items.Add(new SidebarItem { Key = "Contracts", Title = "Hợp đồng", IconText = "📄" });
        _items.Add(new SidebarItem { Key = "Meters", Title = "Chỉ số Điện Nước", IconText = "⚡" });
        _items.Add(new SidebarItem { Key = "Invoices", Title = "Hóa đơn & VietQR", IconText = "💳" });
        _items.Add(new SidebarItem { Key = "Reconcile", Title = "Đối soát Sao kê", IconText = "🔄" });
        _items.Add(new SidebarItem { Key = "Reports", Title = "Báo cáo Thống kê", IconText = "📈" });
        _items.Add(new SidebarItem { Key = "Settings", Title = "Cấu hình Hệ thống", IconText = "⚙" });
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
        using var logoFont = UIHelper.GetFont(13f, FontStyle.Bold);
        using var logoBrush = new SolidBrush(theme.Primary);
        if (!_isCollapsed)
        {
            g.DrawString("🏠 KTX SMART", logoFont, logoBrush, 20, 24);
        }
        else
        {
            g.DrawString("🏠", logoFont, logoBrush, 24, 24);
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
            using var iconFont = UIHelper.GetFont(12f, FontStyle.Regular);
            int iconX = _isCollapsed ? (Width - 20) / 2 : 24;
            g.DrawString(item.IconText, iconFont, Brushes.Black, iconX, itemY + 8);

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

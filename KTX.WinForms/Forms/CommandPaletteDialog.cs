using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using KTX.UI.Helpers;
using KTX.UI.Themes;

namespace KTX.WinForms.Forms;

public class CommandPaletteItem
{
    public string Category { get; set; } = string.Empty; // "Chức năng", "Phòng", "Sinh viên"
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string ActionKey { get; set; } = string.Empty; // "Dashboard", "Rooms", "Students", "ToggleTheme", etc.
    public object? Parameter { get; set; }

    public override string ToString() => $"{Category}: {Title} - {Subtitle}";
}

public class CommandPaletteDialog : Form, IThemable
{
    private readonly TextBox _txtSearch;
    private readonly ListBox _lstItems;
    private readonly List<CommandPaletteItem> _allItems = new();

    public CommandPaletteItem? SelectedCommand { get; private set; }

    public CommandPaletteDialog(List<CommandPaletteItem>? customItems = null)
    {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(580, 400);
        ShowInTaskbar = false;
        KeyPreview = true;
        DoubleBuffered = true;

        // Search Box Panel
        var pnlSearch = new Panel
        {
            Dock = DockStyle.Top,
            Height = 56,
            Padding = new Padding(16, 12, 16, 12)
        };

        _txtSearch = new TextBox
        {
            Dock = DockStyle.Fill,
            Font = UIHelper.GetFont(12f, FontStyle.Regular),
            BorderStyle = BorderStyle.None
        };
        _txtSearch.TextChanged += (s, e) => FilterItems();
        _txtSearch.KeyDown += TxtSearch_KeyDown;

        pnlSearch.Controls.Add(_txtSearch);

        // Divider
        var pnlDivider = new Panel
        {
            Dock = DockStyle.Top,
            Height = 1
        };

        // Results List
        _lstItems = new ListBox
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            DrawMode = DrawMode.OwnerDrawVariable,
            Font = UIHelper.GetFont(10f, FontStyle.Regular),
            IntegralHeight = false,
            ItemHeight = 44
        };
        _lstItems.DrawItem += LstItems_DrawItem;
        _lstItems.MeasureItem += (s, e) => e.ItemHeight = 46;
        _lstItems.DoubleClick += (s, e) => ExecuteSelected();
        _lstItems.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter) ExecuteSelected();
            else if (e.KeyCode == Keys.Escape) Close();
        };

        // Footer hint
        var pnlFooter = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 32,
            Padding = new Padding(16, 6, 16, 6)
        };
        var lblHint = new Label
        {
            Text = "↑↓ để di chuyển  •  [Enter] để chọn  •  [Esc] để đóng",
            Font = UIHelper.GetFont(8.5f),
            ForeColor = Color.Gray,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight
        };
        pnlFooter.Controls.Add(lblHint);

        Controls.Add(_lstItems);
        Controls.Add(pnlDivider);
        Controls.Add(pnlSearch);
        Controls.Add(pnlFooter);

        InitializeCommands(customItems);
        ApplyTheme(ThemeManager.Current);

        KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Escape) Close();
        };
    }

    private void InitializeCommands(List<CommandPaletteItem>? customItems)
    {
        _allItems.Add(new CommandPaletteItem { Category = "Chức năng", Title = "📊 Bảng Tổng quan / Dashboard", Subtitle = "Xem thống kê KPI, lấp đầy phòng, doanh thu", ActionKey = "Dashboard" });
        _allItems.Add(new CommandPaletteItem { Category = "Chức năng", Title = "🏢 Sơ đồ Tòa & Phòng", Subtitle = "Bản đồ phòng trực quan, trạng thái giường", ActionKey = "Rooms" });
        _allItems.Add(new CommandPaletteItem { Category = "Chức năng", Title = "👥 Quản lý Sinh viên", Subtitle = "Hồ sơ, nhập xuất Excel, quét QR CCCD", ActionKey = "Students" });
        _allItems.Add(new CommandPaletteItem { Category = "Chức năng", Title = "🧩 Xếp phòng AI", Subtitle = "Thuật toán Greedy + Local Search, tối ưu điểm", ActionKey = "Allocation" });
        _allItems.Add(new CommandPaletteItem { Category = "Chức năng", Title = "📄 Hợp đồng & Lưu trú", Subtitle = "Nhận/trả phòng, gia hạn, chuyển phòng, xuất hợp đồng PDF", ActionKey = "Contracts" });
        _allItems.Add(new CommandPaletteItem { Category = "Chức năng", Title = "⚡ Chỉ số Điện Nước", Subtitle = "Nhập chỉ số tháng, chế độ thay đồng hồ", ActionKey = "Meters" });
        _allItems.Add(new CommandPaletteItem { Category = "Chức năng", Title = "💳 Hóa đơn & VietQR", Subtitle = "Tính điện nước bậc thang, sinh mã VietQR chuyển khoản", ActionKey = "Invoices" });
        _allItems.Add(new CommandPaletteItem { Category = "Chức năng", Title = "🔄 Đối soát Sao kê", Subtitle = "Chống trùng 2 lớp, khớp sao kê ngân hàng tự động", ActionKey = "Reconcile" });
        _allItems.Add(new CommandPaletteItem { Category = "Chức năng", Title = "🛠️ Báo hỏng & Sự cố", Subtitle = "Quản lý yêu cầu sửa chữa, chi phí vật tư", ActionKey = "Maintenance" });
        _allItems.Add(new CommandPaletteItem { Category = "Chức năng", Title = "📈 Báo cáo Thống kê", Subtitle = "Xuất báo cáo lưu trú Excel, báo cáo tài chính PDF", ActionKey = "Reports" });
        _allItems.Add(new CommandPaletteItem { Category = "Chức năng", Title = "⚙️ Cài đặt Hệ thống", Subtitle = "Trọng số thuật toán, cấu hình ngân hàng VietQR", ActionKey = "Settings" });
        _allItems.Add(new CommandPaletteItem { Category = "Hệ thống", Title = "🌓 Đổi giao diện Sáng / Tối", Subtitle = "Chuyển đổi tức thì giữa Light Mode và Dark Mode", ActionKey = "ToggleTheme" });

        if (customItems != null && customItems.Count > 0)
        {
            _allItems.AddRange(customItems);
        }

        FilterItems();
    }

    private void FilterItems()
    {
        string q = _txtSearch.Text.Trim().ToLowerInvariant();

        _lstItems.BeginUpdate();
        _lstItems.Items.Clear();

        var matches = string.IsNullOrWhiteSpace(q)
            ? _allItems
            : _allItems.Where(i => i.Title.ToLowerInvariant().Contains(q) ||
                                  i.Subtitle.ToLowerInvariant().Contains(q) ||
                                  i.Category.ToLowerInvariant().Contains(q)).ToList();

        foreach (var item in matches)
        {
            _lstItems.Items.Add(item);
        }

        if (_lstItems.Items.Count > 0)
        {
            _lstItems.SelectedIndex = 0;
        }

        _lstItems.EndUpdate();
    }

    private void TxtSearch_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Down)
        {
            if (_lstItems.SelectedIndex < _lstItems.Items.Count - 1)
            {
                _lstItems.SelectedIndex++;
            }
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Up)
        {
            if (_lstItems.SelectedIndex > 0)
            {
                _lstItems.SelectedIndex--;
            }
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Enter)
        {
            ExecuteSelected();
            e.Handled = true;
        }
    }

    private void ExecuteSelected()
    {
        if (_lstItems.SelectedItem is CommandPaletteItem item)
        {
            SelectedCommand = item;
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    private void LstItems_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0 || e.Index >= _lstItems.Items.Count) return;

        var item = (CommandPaletteItem)_lstItems.Items[e.Index];
        bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        var theme = ThemeManager.Current;

        var bg = isSelected ? theme.Primary : theme.Surface;
        using var bgBrush = new SolidBrush(bg);
        e.Graphics.FillRectangle(bgBrush, e.Bounds);

        // Title
        var titleColor = isSelected ? Color.White : theme.TextPrimary;
        using var fontTitle = UIHelper.GetFont(9.5f, FontStyle.Bold);
        using var brushTitle = new SolidBrush(titleColor);
        e.Graphics.DrawString(item.Title, fontTitle, brushTitle, e.Bounds.X + 16, e.Bounds.Y + 6);

        // Subtitle
        var subColor = isSelected ? Color.FromArgb(220, 230, 255) : theme.TextSecondary;
        using var fontSub = UIHelper.GetFont(8.5f, FontStyle.Regular);
        using var brushSub = new SolidBrush(subColor);
        e.Graphics.DrawString(item.Subtitle, fontSub, brushSub, e.Bounds.X + 16, e.Bounds.Y + 25);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        // Draw modern subtle border
        var theme = ThemeManager.Current;
        using var pen = new Pen(theme.Border, 1);
        e.Graphics.DrawRectangle(pen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
    }

    public void ApplyTheme(Theme theme)
    {
        BackColor = theme.Surface;
        _txtSearch.BackColor = theme.Surface;
        _txtSearch.ForeColor = theme.TextPrimary;
        _lstItems.BackColor = theme.Surface;
        Invalidate();
    }
}

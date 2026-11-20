using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using KTX.UI.Themes;

namespace KTX.UI.Controls;

public class KStatCard : Control, IThemable
{
    private string _title = "Tiêu đề";
    private string _value = "0";
    private string _trendText = string.Empty;
    private bool _isPositiveTrend = true;
    private int _borderRadius = 12;

    [Category("K Appearance")]
    public string Title
    {
        get => _title;
        set { _title = value; Invalidate(); }
    }

    [Category("K Appearance")]
    public string Value
    {
        get => _value;
        set { _value = value; Invalidate(); }
    }

    [Category("K Appearance")]
    public string TrendText
    {
        get => _trendText;
        set { _trendText = value; Invalidate(); }
    }

    [Category("K Appearance")]
    public bool IsPositiveTrend
    {
        get => _isPositiveTrend;
        set { _isPositiveTrend = value; Invalidate(); }
    }

    public KStatCard()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);

        DoubleBuffered = true;
        Size = new Size(220, 100);
        ApplyTheme(ThemeManager.Current);
    }

    public void ApplyTheme(Theme theme)
    {
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

        var theme = ThemeManager.Current;
        var parentBg = Parent?.BackColor ?? theme.Background;
        if (parentBg == Color.Transparent) parentBg = theme.Background;
        g.Clear(parentBg);

        var bounds = new Rectangle(1, 1, Width - 3, Height - 3);

        using var path = UIHelper.CreateRoundedRectangle(bounds, _borderRadius);

        // Fill background
        using (var bgBrush = new SolidBrush(theme.SurfaceCard))
        {
            g.FillPath(bgBrush, path);
        }

        // Draw subtle left accent stripe
        var accentColor = _isPositiveTrend ? theme.Primary : theme.Danger;
        using (var accentBrush = new SolidBrush(accentColor))
        {
            g.FillRectangle(accentBrush, bounds.X + 1, bounds.Y + 14, 4, bounds.Height - 28);
        }

        // Draw border
        using (var borderPen = new Pen(theme.Border, 1f))
        {
            g.DrawPath(borderPen, path);
        }

        int textX = 18;
        int textW = Math.Max(10, Width - 30);

        // Draw Title
        using var titleFont = UIHelper.GetFont(9.2f, FontStyle.Regular);
        TextRenderer.DrawText(g, _title, titleFont, new Rectangle(textX, 12, textW, 20), theme.TextSecondary,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

        // Draw Big Value
        using var valueFont = UIHelper.GetFont(17f, FontStyle.Bold);
        TextRenderer.DrawText(g, _value, valueFont, new Rectangle(textX, 34, textW, 34), theme.TextPrimary,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

        // Draw Trend / Subtitle
        if (!string.IsNullOrEmpty(_trendText))
        {
            using var trendFont = UIHelper.GetFont(8.3f, FontStyle.Bold);
            var trendColor = _isPositiveTrend ? theme.Success : theme.Danger;
            TextRenderer.DrawText(g, _trendText, trendFont, new Rectangle(textX, 72, textW, 20), trendColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        }
    }
}

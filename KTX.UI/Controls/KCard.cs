using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using KTX.UI.Themes;

namespace KTX.UI.Controls;

public class KCard : Panel, IThemable
{
    private int _borderRadius = 12;
    private string _title = string.Empty;
    private string _subtitle = string.Empty;

    [Category("K Appearance")]
    public int BorderRadius
    {
        get => _borderRadius;
        set { _borderRadius = value; Invalidate(); }
    }

    [Category("K Appearance")]
    public string Title
    {
        get => _title;
        set { _title = value; Invalidate(); }
    }

    [Category("K Appearance")]
    public string Subtitle
    {
        get => _subtitle;
        set { _subtitle = value; Invalidate(); }
    }

    public KCard()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);

        DoubleBuffered = true;
        Padding = new Padding(16);
        ApplyTheme(ThemeManager.Current);
    }

    public void ApplyTheme(Theme theme)
    {
        BackColor = theme.SurfaceCard;
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
        using (var brush = new SolidBrush(theme.SurfaceCard))
        {
            g.FillPath(brush, path);
        }

        // Draw border
        using (var pen = new Pen(theme.Border, 1f))
        {
            g.DrawPath(pen, path);
        }

        // Draw Title & Subtitle with crisp ClearType TextRenderer
        int currentY = 14;
        int textWidth = Math.Max(10, Width - 32);
        if (!string.IsNullOrEmpty(_title))
        {
            using var titleFont = UIHelper.GetFont(11.5f, FontStyle.Bold);
            var titleRect = new Rectangle(16, currentY, textWidth, 24);
            TextRenderer.DrawText(g, _title, titleFont, titleRect, theme.TextPrimary,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
            currentY += 24;
        }

        if (!string.IsNullOrEmpty(_subtitle))
        {
            using var subFont = UIHelper.GetFont(8.8f, FontStyle.Regular);
            var subRect = new Rectangle(16, currentY, textWidth, 18);
            TextRenderer.DrawText(g, _subtitle, subFont, subRect, theme.TextSecondary,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
        }
    }
}

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

        var theme = ThemeManager.Current;
        var bounds = new Rectangle(0, 0, Width - 1, Height - 1);

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

        // Draw Title & Subtitle if provided
        int currentY = 16;
        if (!string.IsNullOrEmpty(_title))
        {
            using var titleFont = UIHelper.GetFont(12f, FontStyle.Bold);
            using var titleBrush = new SolidBrush(theme.TextPrimary);
            g.DrawString(_title, titleFont, titleBrush, 16, currentY);
            currentY += titleFont.Height + 2;
        }

        if (!string.IsNullOrEmpty(_subtitle))
        {
            using var subFont = UIHelper.GetFont(9f, FontStyle.Regular);
            using var subBrush = new SolidBrush(theme.TextSecondary);
            g.DrawString(_subtitle, subFont, subBrush, 16, currentY);
        }
    }
}

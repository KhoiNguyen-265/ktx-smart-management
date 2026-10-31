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

        var theme = ThemeManager.Current;
        var bounds = new Rectangle(0, 0, Width - 1, Height - 1);

        using var path = UIHelper.CreateRoundedRectangle(bounds, _borderRadius);

        // Fill background
        using (var bgBrush = new SolidBrush(theme.SurfaceCard))
        {
            g.FillPath(bgBrush, path);
        }

        // Draw border
        using (var borderPen = new Pen(theme.Border, 1f))
        {
            g.DrawPath(borderPen, path);
        }

        // Draw Title
        using var titleFont = UIHelper.GetFont(9.5f, FontStyle.Regular);
        using var titleBrush = new SolidBrush(theme.TextSecondary);
        g.DrawString(_title, titleFont, titleBrush, 16, 14);

        // Draw Big Value
        using var valueFont = UIHelper.GetFont(18f, FontStyle.Bold);
        using var valueBrush = new SolidBrush(theme.TextPrimary);
        g.DrawString(_value, valueFont, valueBrush, 16, 34);

        // Draw Trend / Subtitle
        if (!string.IsNullOrEmpty(_trendText))
        {
            using var trendFont = UIHelper.GetFont(8.5f, FontStyle.Bold);
            var trendColor = _isPositiveTrend ? theme.Success : theme.Danger;
            using var trendBrush = new SolidBrush(trendColor);
            g.DrawString(_trendText, trendFont, trendBrush, 16, 70);
        }
    }
}

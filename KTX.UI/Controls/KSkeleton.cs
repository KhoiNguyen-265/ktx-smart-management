using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using KTX.UI.Themes;

namespace KTX.UI.Controls;

public class KSkeleton : Control, IThemable
{
    private readonly System.Windows.Forms.Timer _timer;
    private float _alpha = 0.3f;
    private bool _increasing = true;
    private int _borderRadius = 6;

    public int BorderRadius
    {
        get => _borderRadius;
        set { _borderRadius = value; Invalidate(); }
    }

    public KSkeleton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);

        DoubleBuffered = true;
        Size = new Size(150, 24);

        _timer = new System.Windows.Forms.Timer { Interval = 40 };
        _timer.Tick += (s, e) =>
        {
            if (_increasing)
            {
                _alpha += 0.03f;
                if (_alpha >= 0.7f) _increasing = false;
            }
            else
            {
                _alpha -= 0.03f;
                if (_alpha <= 0.2f) _increasing = true;
            }
            Invalidate();
        };

        VisibleChanged += (s, e) =>
        {
            if (Visible) _timer.Start();
            else _timer.Stop();
        };

        if (Visible) _timer.Start();
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
        var baseColor = theme.Border;
        int alphaByte = (int)(_alpha * 255);
        using var brush = new SolidBrush(Color.FromArgb(alphaByte, baseColor));
        g.FillPath(brush, path);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer.Dispose();
        }
        base.Dispose(disposing);
    }
}

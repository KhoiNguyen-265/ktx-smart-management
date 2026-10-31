using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using KTX.UI.Themes;

namespace KTX.UI.Controls;

public enum ToastType
{
    Success,
    Info,
    Warning,
    Error
}

public class KToast : Form
{
    private readonly System.Windows.Forms.Timer _timer;
    private readonly string _message;
    private readonly ToastType _type;

    public KToast(string message, ToastType type = ToastType.Info)
    {
        _message = message;
        _type = type;

        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        StartPosition = FormStartPosition.Manual;
        Size = new Size(320, 60);

        _timer = new System.Windows.Forms.Timer { Interval = 3000 };
        _timer.Tick += (s, e) => { _timer.Stop(); Close(); };
    }

    public static void Show(Form parent, string message, ToastType type = ToastType.Info)
    {
        var toast = new KToast(message, type);
        var parentLocation = parent.Location;
        var parentSize = parent.Size;

        toast.Location = new Point(
            parentLocation.X + parentSize.Width - toast.Width - 24,
            parentLocation.Y + parentSize.Height - toast.Height - 24
        );

        toast.Show(parent);
        toast._timer.Start();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var theme = ThemeManager.Current;
        var bounds = new Rectangle(0, 0, Width - 1, Height - 1);

        using var path = UIHelper.CreateRoundedRectangle(bounds, 10);

        // Background
        using (var brush = new SolidBrush(theme.SurfaceCard))
        {
            g.FillPath(brush, path);
        }

        // Indicator stripe on left
        var indicatorColor = _type switch
        {
            ToastType.Success => theme.Success,
            ToastType.Warning => theme.Warning,
            ToastType.Error => theme.Danger,
            _ => theme.Info
        };

        using (var indicatorBrush = new SolidBrush(indicatorColor))
        {
            g.FillRectangle(indicatorBrush, 0, 0, 6, Height);
        }

        // Border
        using (var pen = new Pen(theme.Border, 1f))
        {
            g.DrawPath(pen, path);
        }

        // Message text
        using var font = UIHelper.GetFont(9.5f, FontStyle.Regular);
        using var textBrush = new SolidBrush(theme.TextPrimary);
        var textRect = new Rectangle(18, 12, Width - 30, Height - 24);
        g.DrawString(_message, font, textBrush, textRect);
    }
}

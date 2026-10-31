using System.Drawing;
using System.Drawing.Drawing2D;

namespace KTX.UI.Themes;

public interface IThemable
{
    void ApplyTheme(Theme theme);
}

public static class ThemeManager
{
    private static ThemeMode _mode = ThemeMode.Light;
    public static event Action? ThemeChanged;

    public static ThemeMode CurrentMode
    {
        get => _mode;
        set
        {
            if (_mode != value)
            {
                _mode = value;
                ThemeChanged?.Invoke();
            }
        }
    }

    public static Theme Current => _mode == ThemeMode.Light ? Theme.LightTheme : Theme.DarkTheme;

    public static void ToggleTheme()
    {
        CurrentMode = CurrentMode == ThemeMode.Light ? ThemeMode.Dark : ThemeMode.Light;
    }

    public static void ApplyThemeRecursively(Control root)
    {
        var theme = Current;
        ApplyControlTheme(root, theme);
    }

    private static void ApplyControlTheme(Control control, Theme theme)
    {
        if (control is IThemable themable)
        {
            themable.ApplyTheme(theme);
        }
        else
        {
            // Fallback cho form hoặc panel chuẩn
            if (control is Form form)
            {
                form.BackColor = theme.Background;
                form.ForeColor = theme.TextPrimary;
            }
        }

        foreach (Control child in control.Controls)
        {
            ApplyControlTheme(child, theme);
        }
    }
}

public static class UIHelper
{
    public static GraphicsPath CreateRoundedRectangle(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        if (radius <= 0)
        {
            path.AddRectangle(bounds);
            return path;
        }

        int diameter = radius * 2;
        var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));

        // Góc trên trái
        path.AddArc(arc, 180, 90);

        // Góc trên phải
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);

        // Góc dưới phải
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);

        // Góc dưới trái
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);

        path.CloseFigure();
        return path;
    }

    public static Font GetFont(float size = 10f, FontStyle style = FontStyle.Regular)
    {
        // Ưu tiên Segoe UI Variable, dự phòng Segoe UI
        try
        {
            return new Font("Segoe UI Variable Text", size, style);
        }
        catch
        {
            return new Font("Segoe UI", size, style);
        }
    }
}

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using KTX.UI.Themes;

namespace KTX.UI.Controls;

public enum KBadgeType
{
    Success,
    Warning,
    Danger,
    Info,
    Neutral
}

public class KBadge : Control, IThemable
{
    private KBadgeType _badgeType = KBadgeType.Neutral;

    [Category("K Appearance")]
    public KBadgeType BadgeType
    {
        get => _badgeType;
        set { _badgeType = value; Invalidate(); }
    }

    public KBadge()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);

        Font = UIHelper.GetFont(9f, FontStyle.Bold);
        Size = new Size(90, 26);
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

        Color bgColor;
        Color textColor;

        switch (_badgeType)
        {
            case KBadgeType.Success:
                bgColor = theme.SuccessBg;
                textColor = theme.SuccessText;
                break;
            case KBadgeType.Warning:
                bgColor = theme.WarningBg;
                textColor = theme.WarningText;
                break;
            case KBadgeType.Danger:
                bgColor = theme.DangerBg;
                textColor = theme.DangerText;
                break;
            case KBadgeType.Info:
                bgColor = theme.InfoBg;
                textColor = theme.InfoText;
                break;
            case KBadgeType.Neutral:
            default:
                bgColor = theme.SurfaceHover;
                textColor = theme.TextSecondary;
                break;
        }

        int pillRadius = Height / 2;
        using var path = UIHelper.CreateRoundedRectangle(bounds, pillRadius);

        using (var brush = new SolidBrush(bgColor))
        {
            g.FillPath(brush, path);
        }

        TextRenderer.DrawText(
            g,
            Text,
            Font,
            bounds,
            textColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
}

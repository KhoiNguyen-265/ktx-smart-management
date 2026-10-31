using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using KTX.UI.Themes;

namespace KTX.UI.Controls;

public enum KButtonStyle
{
    Primary,
    Secondary,
    Ghost,
    Danger,
    Success
}

public class KButton : Button, IThemable
{
    private bool _isHovered;
    private bool _isPressed;
    private bool _isLoading;
    private int _borderRadius = 8;
    private KButtonStyle _style = KButtonStyle.Primary;

    [Category("K Appearance")]
    public int BorderRadius
    {
        get => _borderRadius;
        set { _borderRadius = value; Invalidate(); }
    }

    [Category("K Appearance")]
    public KButtonStyle Style
    {
        get => _style;
        set { _style = value; Invalidate(); }
    }

    [Category("K Appearance")]
    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; Invalidate(); }
    }

    public KButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);

        Cursor = Cursors.Hand;
        Font = UIHelper.GetFont(10f, FontStyle.Bold);
        Size = new Size(120, 40);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _isHovered = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _isHovered = false;
        _isPressed = false;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs mevent)
    {
        base.OnMouseDown(mevent);
        _isPressed = true;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs mevent)
    {
        base.OnMouseUp(mevent);
        _isPressed = false;
        Invalidate();
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
        Color borderColor = Color.Transparent;

        switch (_style)
        {
            case KButtonStyle.Secondary:
                bgColor = _isPressed ? theme.SurfaceHover : (_isHovered ? theme.SurfaceHover : theme.Surface);
                textColor = theme.TextPrimary;
                borderColor = theme.Border;
                break;
            case KButtonStyle.Ghost:
                bgColor = _isPressed ? theme.SurfaceHover : (_isHovered ? theme.SurfaceHover : Color.Transparent);
                textColor = theme.TextPrimary;
                break;
            case KButtonStyle.Danger:
                bgColor = _isPressed ? theme.DangerText : (_isHovered ? theme.Danger : theme.Danger);
                textColor = Color.White;
                break;
            case KButtonStyle.Success:
                bgColor = _isPressed ? theme.SuccessText : (_isHovered ? theme.Success : theme.Success);
                textColor = Color.White;
                break;
            case KButtonStyle.Primary:
            default:
                bgColor = _isPressed ? theme.PrimaryPressed : (_isHovered ? theme.PrimaryHover : theme.Primary);
                textColor = theme.PrimaryText;
                break;
        }

        if (!Enabled)
        {
            bgColor = theme.Border;
            textColor = theme.TextMuted;
            borderColor = Color.Transparent;
        }

        using var path = UIHelper.CreateRoundedRectangle(bounds, _borderRadius);

        // Vẽ nền
        using (var brush = new SolidBrush(bgColor))
        {
            g.FillPath(brush, path);
        }

        // Vẽ viền nếu có
        if (borderColor != Color.Transparent)
        {
            using var pen = new Pen(borderColor, 1f);
            g.DrawPath(pen, path);
        }

        // Vẽ chữ hoặc Loading
        var displayText = _isLoading ? "Đang xử lý..." : Text;
        var textBounds = new Rectangle(0, 0, Width, Height);

        TextRenderer.DrawText(
            g,
            displayText,
            Font,
            textBounds,
            textColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
}

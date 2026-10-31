using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using KTX.UI.Themes;

namespace KTX.UI.Controls;

public class KTextBox : UserControl, IThemable
{
    private readonly TextBox _innerBox;
    private bool _isFocused;
    private string _placeholder = string.Empty;
    private int _borderRadius = 8;
    private bool _isPassword;
    private Image? _icon;

    [Category("K Appearance")]
    public string Placeholder
    {
        get => _placeholder;
        set { _placeholder = value; Invalidate(); }
    }

    [Category("K Appearance")]
    public int BorderRadius
    {
        get => _borderRadius;
        set { _borderRadius = value; Invalidate(); }
    }

    [Category("K Appearance")]
    public bool IsPassword
    {
        get => _isPassword;
        set
        {
            _isPassword = value;
            _innerBox.UseSystemPasswordChar = value;
        }
    }

    [Category("K Appearance")]
    public Image? Icon
    {
        get => _icon;
        set { _icon = value; AdjustInnerBox(); Invalidate(); }
    }

    [Category("K Appearance")]
    [System.Diagnostics.CodeAnalysis.AllowNull]
    public override string Text
    {
        get => _innerBox.Text;
        set => _innerBox.Text = value ?? string.Empty;
    }

    public TextBox InnerTextBox => _innerBox;

    public KTextBox()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);

        DoubleBuffered = true;
        Padding = new Padding(10, 8, 10, 8);
        Size = new Size(250, 40);

        _innerBox = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Font = UIHelper.GetFont(10f),
            Location = new Point(12, 10),
            Width = Width - 24
        };

        _innerBox.Enter += (s, e) => { _isFocused = true; Invalidate(); };
        _innerBox.Leave += (s, e) => { _isFocused = false; Invalidate(); };
        _innerBox.TextChanged += (s, e) => { OnTextChanged(e); Invalidate(); };

        Controls.Add(_innerBox);
        AdjustInnerBox();
        ApplyTheme(ThemeManager.Current);
    }

    private void AdjustInnerBox()
    {
        int left = _icon != null ? 36 : 12;
        _innerBox.Location = new Point(left, (Height - _innerBox.Height) / 2);
        _innerBox.Width = Width - left - 12;
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        AdjustInnerBox();
    }

    public void ApplyTheme(Theme theme)
    {
        BackColor = theme.Surface;
        _innerBox.BackColor = theme.Surface;
        _innerBox.ForeColor = theme.TextPrimary;
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
        using (var brush = new SolidBrush(theme.Surface))
        {
            g.FillPath(brush, path);
        }

        // Draw border (highlight when focused)
        var borderColor = _isFocused ? theme.Primary : theme.Border;
        using (var pen = new Pen(borderColor, _isFocused ? 1.8f : 1f))
        {
            g.DrawPath(pen, path);
        }

        // Draw icon if present
        if (_icon != null)
        {
            int iconY = (Height - _icon.Height) / 2;
            g.DrawImage(_icon, 10, iconY, _icon.Width, _icon.Height);
        }

        // Draw placeholder if text is empty
        if (string.IsNullOrEmpty(_innerBox.Text) && !string.IsNullOrEmpty(_placeholder) && !_isFocused)
        {
            int textX = _icon != null ? 36 : 12;
            int textY = (Height - _innerBox.Height) / 2;
            using var placeholderBrush = new SolidBrush(theme.TextMuted);
            g.DrawString(_placeholder, _innerBox.Font, placeholderBrush, textX, textY);
        }
    }
}

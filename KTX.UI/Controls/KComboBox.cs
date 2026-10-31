using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using KTX.UI.Themes;

namespace KTX.UI.Controls;

public class KComboBox : ComboBox, IThemable
{
    private int _borderRadius = 8;

    [Category("K Appearance")]
    public int BorderRadius
    {
        get => _borderRadius;
        set { _borderRadius = value; Invalidate(); }
    }

    public KComboBox()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);

        DrawMode = DrawMode.OwnerDrawFixed;
        DropDownStyle = ComboBoxStyle.DropDownList;
        Font = UIHelper.GetFont(10f);
        ItemHeight = 32;
        Size = new Size(200, 38);

        DrawItem += KComboBox_DrawItem;
        ApplyTheme(ThemeManager.Current);
    }

    private void KComboBox_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0) return;

        var theme = ThemeManager.Current;
        bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

        var bg = isSelected ? theme.PrimaryLight : theme.Surface;
        var fg = isSelected ? theme.Primary : theme.TextPrimary;

        using var bgBrush = new SolidBrush(bg);
        e.Graphics.FillRectangle(bgBrush, e.Bounds);

        var text = GetItemText(Items[e.Index]);
        using var textBrush = new SolidBrush(fg);
        e.Graphics.DrawString(text, Font, textBrush, e.Bounds.X + 8, e.Bounds.Y + (e.Bounds.Height - Font.Height) / 2);
    }

    public void ApplyTheme(Theme theme)
    {
        BackColor = theme.Surface;
        ForeColor = theme.TextPrimary;
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

        // Draw border
        using (var pen = new Pen(theme.Border, 1f))
        {
            g.DrawPath(pen, path);
        }

        // Draw selected text
        string text = SelectedItem != null ? (GetItemText(SelectedItem) ?? string.Empty) : Text;
        if (!string.IsNullOrEmpty(text))
        {
            using var textBrush = new SolidBrush(theme.TextPrimary);
            g.DrawString(text, Font, textBrush, 12, (Height - Font.Height) / 2);
        }

        // Draw Arrow
        int arrowX = Width - 24;
        int arrowY = (Height - 6) / 2;
        var arrowPoints = new[]
        {
            new Point(arrowX, arrowY),
            new Point(arrowX + 8, arrowY),
            new Point(arrowX + 4, arrowY + 5)
        };

        using var arrowBrush = new SolidBrush(theme.TextSecondary);
        g.FillPolygon(arrowBrush, arrowPoints);
    }
}

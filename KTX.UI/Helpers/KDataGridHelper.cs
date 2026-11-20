using System.Drawing;
using System.Windows.Forms;
using KTX.UI.Themes;

namespace KTX.UI.Helpers;

public static class KDataGridHelper
{
    public static void ApplyModernStyle(DataGridView grid, bool autoFill = true)
    {
        var theme = ThemeManager.Current;

        grid.AutoGenerateColumns = false;
        grid.BorderStyle = BorderStyle.None;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.EnableHeadersVisualStyles = false;
        grid.RowHeadersVisible = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.AllowUserToResizeRows = false;
        grid.AutoSizeColumnsMode = autoFill
            ? DataGridViewAutoSizeColumnsMode.Fill
            : DataGridViewAutoSizeColumnsMode.None;

        grid.BackgroundColor = theme.Surface;
        grid.GridColor = theme.Border;

        // Header style
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.ColumnHeadersHeight = 42;
        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = theme.SurfaceHover,
            ForeColor = theme.TextSecondary,
            SelectionBackColor = theme.SurfaceHover,
            SelectionForeColor = theme.TextSecondary,
            Font = UIHelper.GetFont(9f, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            Padding = new Padding(6, 0, 4, 0),
            WrapMode = DataGridViewTriState.False
        };

        // Rows style
        grid.RowTemplate.Height = 38;
        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = theme.Surface,
            ForeColor = theme.TextPrimary,
            SelectionBackColor = theme.PrimaryLight,
            SelectionForeColor = theme.Primary,
            Font = UIHelper.GetFont(9f, FontStyle.Regular),
            Padding = new Padding(6, 0, 4, 0)
        };
        grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = theme.Mode == ThemeMode.Dark ? Color.FromArgb(28, 32, 40) : Color.FromArgb(249, 250, 252),
            ForeColor = theme.TextPrimary,
            SelectionBackColor = theme.PrimaryLight,
            SelectionForeColor = theme.Primary,
            Font = UIHelper.GetFont(9f, FontStyle.Regular),
            Padding = new Padding(6, 0, 4, 0)
        };
    }
}

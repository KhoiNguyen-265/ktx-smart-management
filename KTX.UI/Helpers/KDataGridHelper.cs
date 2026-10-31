using System.Drawing;
using System.Windows.Forms;
using KTX.UI.Themes;

namespace KTX.UI.Helpers;

public static class KDataGridHelper
{
    public static void ApplyModernStyle(DataGridView grid)
    {
        var theme = ThemeManager.Current;

        grid.BorderStyle = BorderStyle.None;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.EnableHeadersVisualStyles = false;
        grid.RowHeadersVisible = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.AllowUserToResizeRows = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        grid.BackgroundColor = theme.Surface;
        grid.GridColor = theme.Border;

        // Header style
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.ColumnHeadersHeight = 44;
        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = theme.SurfaceHover,
            ForeColor = theme.TextSecondary,
            Font = UIHelper.GetFont(9.5f, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            Padding = new Padding(12, 0, 0, 0)
        };

        // Rows style
        grid.RowTemplate.Height = 42;
        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = theme.Surface,
            ForeColor = theme.TextPrimary,
            SelectionBackColor = theme.PrimaryLight,
            SelectionForeColor = theme.Primary,
            Font = UIHelper.GetFont(9.5f, FontStyle.Regular),
            Padding = new Padding(12, 0, 0, 0)
        };
    }
}

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using KTX.Domain.Enums;
using KTX.Domain.DTOs;
using KTX.UI.Themes;

namespace KTX.UI.Controls;

public class KRoomMap : Control, IThemable
{
    private List<RoomSummaryDto> _rooms = new();
    private int _hoveredIndex = -1;
    private int _selectedIndex = -1;

    public event Action<RoomSummaryDto>? RoomClicked;

    public List<RoomSummaryDto> Rooms
    {
        get => _rooms;
        set
        {
            _rooms = value ?? new();
            _hoveredIndex = -1;
            _selectedIndex = -1;
            Invalidate();
        }
    }

    public RoomSummaryDto? SelectedRoom => (_selectedIndex >= 0 && _selectedIndex < _rooms.Count)
        ? _rooms[_selectedIndex]
        : null;

    public KRoomMap()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);

        DoubleBuffered = true;
        Size = new Size(800, 320);
        Cursor = Cursors.Default;
        ApplyTheme(ThemeManager.Current);
    }

    public void ApplyTheme(Theme theme)
    {
        BackColor = theme.Surface;
        Invalidate();
    }

    private Rectangle GetCardRect(int index)
    {
        int cardWidth = 175;
        int cardHeight = 110;
        int gap = 16;
        int padding = 16;

        int cardsPerRow = Math.Max(1, (Width - padding * 2 + gap) / (cardWidth + gap));
        int row = index / cardsPerRow;
        int col = index % cardsPerRow;

        int x = padding + col * (cardWidth + gap);
        int y = padding + row * (cardHeight + gap);

        return new Rectangle(x, y, cardWidth, cardHeight);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        int newHover = -1;

        for (int i = 0; i < _rooms.Count; i++)
        {
            if (GetCardRect(i).Contains(e.Location))
            {
                newHover = i;
                break;
            }
        }

        if (newHover != _hoveredIndex)
        {
            _hoveredIndex = newHover;
            Cursor = _hoveredIndex >= 0 ? Cursors.Hand : Cursors.Default;
            Invalidate();
        }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hoveredIndex = -1;
        Cursor = Cursors.Default;
        Invalidate();
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        for (int i = 0; i < _rooms.Count; i++)
        {
            if (GetCardRect(i).Contains(e.Location))
            {
                _selectedIndex = i;
                RoomClicked?.Invoke(_rooms[i]);
                Invalidate();
                break;
            }
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var theme = ThemeManager.Current;

        // Background
        using (var bgBrush = new SolidBrush(theme.Surface))
        {
            g.FillRectangle(bgBrush, ClientRectangle);
        }

        if (_rooms.Count == 0)
        {
            using var font = UIHelper.GetFont(11f, FontStyle.Italic);
            using var brush = new SolidBrush(theme.TextMuted);
            g.DrawString("Không có phòng nào trên tầng này", font, brush, 24, 40);
            return;
        }

        for (int i = 0; i < _rooms.Count; i++)
        {
            var room = _rooms[i];
            var rect = GetCardRect(i);
            bool isHovered = (i == _hoveredIndex);
            bool isSelected = (i == _selectedIndex);

            // Determine Status Color
            Color statusColor;
            string statusText;
            if (room.TrangThai == TrangThaiPhong.BaoTri)
            {
                statusColor = theme.TextMuted;
                statusText = "Bảo trì";
            }
            else if (room.SoNguoiDangO == 0)
            {
                statusColor = theme.Success;
                statusText = "Trống 100%";
            }
            else if (room.SoNguoiDangO < room.SucChua)
            {
                statusColor = theme.Warning;
                statusText = $"Còn {room.SoChoTrong} chỗ";
            }
            else
            {
                statusColor = theme.Danger;
                statusText = "Đã đầy";
            }

            // Draw Card background
            using var path = UIHelper.CreateRoundedRectangle(rect, 8);
            var cardBg = isSelected ? theme.PrimaryLight : (isHovered ? theme.SurfaceHover : theme.SurfaceCard);
            using (var cardBrush = new SolidBrush(cardBg))
            {
                g.FillPath(cardBrush, path);
            }

            // Draw Card border
            var borderColor = isSelected ? theme.Primary : (isHovered ? theme.BorderHover : theme.Border);
            using (var borderPen = new Pen(borderColor, isSelected ? 2f : 1f))
            {
                g.DrawPath(borderPen, path);
            }

            // Left status bar stripe
            using (var statusBrush = new SolidBrush(statusColor))
            {
                g.FillRectangle(statusBrush, rect.X, rect.Y + 8, 4, rect.Height - 16);
            }

            // Room Code (e.g. "A101")
            using var codeFont = UIHelper.GetFont(11f, FontStyle.Bold);
            using var textBrush = new SolidBrush(theme.TextPrimary);
            g.DrawString(room.MaPhong, codeFont, textBrush, rect.X + 14, rect.Y + 12);

            // Room Type
            using var typeFont = UIHelper.GetFont(8.5f, FontStyle.Regular);
            using var subTextBrush = new SolidBrush(theme.TextSecondary);
            g.DrawString(room.TenLoaiPhong, typeFont, subTextBrush, rect.X + 14, rect.Y + 32);

            // Capacity ratio (e.g. "3 / 4")
            string ratioText = $"{room.SoNguoiDangO}/{room.SucChua} người";
            using var ratioFont = UIHelper.GetFont(8.5f, FontStyle.Bold);
            using var statusTextBrush = new SolidBrush(statusColor);
            g.DrawString(ratioText, ratioFont, statusTextBrush, rect.X + 14, rect.Y + 54);

            // Capacity Progress Bar
            int barX = rect.X + 14;
            int barY = rect.Y + 76;
            int barWidth = rect.Width - 28;
            int barHeight = 6;

            var barBgRect = new Rectangle(barX, barY, barWidth, barHeight);
            using var barBgPath = UIHelper.CreateRoundedRectangle(barBgRect, 3);
            using (var barBgBrush = new SolidBrush(theme.Border))
            {
                g.FillPath(barBgBrush, barBgPath);
            }

            if (room.SucChua > 0 && room.SoNguoiDangO > 0)
            {
                float fillRatio = Math.Min(1f, (float)room.SoNguoiDangO / room.SucChua);
                int fillWidth = Math.Max(6, (int)(barWidth * fillRatio));
                var barFillRect = new Rectangle(barX, barY, fillWidth, barHeight);
                using var barFillPath = UIHelper.CreateRoundedRectangle(barFillRect, 3);
                using (var fillBrush = new SolidBrush(statusColor))
                {
                    g.FillPath(fillBrush, barFillPath);
                }
            }

            // Status bottom label
            g.DrawString(statusText, typeFont, subTextBrush, rect.X + 14, rect.Y + 86);
        }
    }
}

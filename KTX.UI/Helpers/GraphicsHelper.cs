using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace KTX.UI.Helpers;

public static class GraphicsHelper
{
    public static void SetHighQuality(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
    }

    public static GraphicsPath GetRoundedPath(RectangleF rect, float radius)
    {
        var path = new GraphicsPath();
        if (radius <= 0)
        {
            path.AddRectangle(rect);
            return path;
        }

        float diameter = radius * 2;
        var size = new SizeF(diameter, diameter);
        var arc = new RectangleF(rect.Location, size);

        // Top-left
        path.AddArc(arc, 180, 90);

        // Top-right
        arc.X = rect.Right - diameter;
        path.AddArc(arc, 270, 90);

        // Bottom-right
        arc.Y = rect.Bottom - diameter;
        path.AddArc(arc, 0, 90);

        // Bottom-left
        arc.X = rect.Left;
        path.AddArc(arc, 90, 90);

        path.CloseFigure();
        return path;
    }

    public static GraphicsPath GetPillPath(RectangleF rect)
    {
        float radius = rect.Height / 2f;
        return GetRoundedPath(rect, radius);
    }
}

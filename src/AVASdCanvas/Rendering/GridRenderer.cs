using Avalonia;
using Avalonia.Media;

namespace AVASdCanvas.Rendering;

/// <summary>
/// Renders a graph-paper style grid onto the canvas.
/// </summary>
public static class GridRenderer
{
    public static void Render(DrawingContext ctx, Size bounds, IBrush lineBrush, double spacing)
    {
        if (spacing <= 0) return;

        var pen = new Pen(lineBrush, 0.5);

        // Vertical lines
        for (double x = spacing; x < bounds.Width; x += spacing)
        {
            ctx.DrawLine(pen, new Point(x, 0), new Point(x, bounds.Height));
        }

        // Horizontal lines
        for (double y = spacing; y < bounds.Height; y += spacing)
        {
            ctx.DrawLine(pen, new Point(0, y), new Point(bounds.Width, y));
        }
    }
}

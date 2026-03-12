using Avalonia;
using Avalonia.Media;
using AVASdCanvas.Models;

namespace AVASdCanvas.Rendering;

/// <summary>
/// Renders connectors (lines with endpoint decorators) between entities.
/// </summary>
public static class ConnectorRenderer
{
    private const double ArrowLength = 12;
    private const double ArrowWidth = 7;
    private const double DotRadius = 5;

    public static void Render(DrawingContext ctx, Connector connector, GraphicEntity source, GraphicEntity target)
    {
        var lineBrush = connector.LineBrush ?? Brushes.DarkSlateGray;
        var pen = new Pen(lineBrush, connector.LineThickness);

        var sourceCenter = source.Center;
        var targetCenter = target.Center;

        // Clip line to entity edges
        var startPoint = ClipToEntityEdge(sourceCenter, targetCenter, source.Bounds);
        var endPoint = ClipToEntityEdge(targetCenter, sourceCenter, target.Bounds);

        ctx.DrawLine(pen, startPoint, endPoint);

        // Draw endpoints
        DrawEndpoint(ctx, startPoint, endPoint, startPoint, connector.SourceEndpoint, lineBrush);
        DrawEndpoint(ctx, startPoint, endPoint, endPoint, connector.TargetEndpoint, lineBrush);
    }

    private static void DrawEndpoint(DrawingContext ctx, Point lineStart, Point lineEnd,
        Point endpointPos, EndpointStyle style, IBrush brush)
    {
        switch (style)
        {
            case EndpointStyle.RoundDot:
                ctx.DrawEllipse(brush, null, endpointPos, DotRadius, DotRadius);
                break;

            case EndpointStyle.PointedArrow:
                DrawArrow(ctx, lineStart, lineEnd, endpointPos, brush);
                break;

            case EndpointStyle.None:
            default:
                break;
        }
    }

    private static void DrawArrow(DrawingContext ctx, Point lineStart, Point lineEnd,
        Point tip, IBrush brush)
    {
        // Determine arrow direction: arrow points away from the other end
        double dx, dy;
        if (tip == lineEnd)
        {
            // Arrow at end, pointing from start to end
            dx = lineEnd.X - lineStart.X;
            dy = lineEnd.Y - lineStart.Y;
        }
        else
        {
            // Arrow at start, pointing from end to start
            dx = lineStart.X - lineEnd.X;
            dy = lineStart.Y - lineEnd.Y;
        }

        double length = Math.Sqrt(dx * dx + dy * dy);
        if (length < 0.001) return;

        // Normalize
        double ux = dx / length;
        double uy = dy / length;

        // Perpendicular
        double px = -uy;
        double py = ux;

        // Arrow base points
        var baseCenter = new Point(tip.X - ux * ArrowLength, tip.Y - uy * ArrowLength);
        var left = new Point(baseCenter.X + px * ArrowWidth, baseCenter.Y + py * ArrowWidth);
        var right = new Point(baseCenter.X - px * ArrowWidth, baseCenter.Y - py * ArrowWidth);

        var geometry = new StreamGeometry();
        using (var gc = geometry.Open())
        {
            gc.BeginFigure(tip, true);
            gc.LineTo(left);
            gc.LineTo(right);
            gc.EndFigure(true);
        }

        ctx.DrawGeometry(brush, null, geometry);
    }

    /// <summary>
    /// Clips a line from center to target so it starts at the rectangle edge.
    /// </summary>
    private static Point ClipToEntityEdge(Point center, Point target, Rect bounds)
    {
        double dx = target.X - center.X;
        double dy = target.Y - center.Y;

        if (Math.Abs(dx) < 0.001 && Math.Abs(dy) < 0.001)
            return center;

        double halfW = bounds.Width / 2;
        double halfH = bounds.Height / 2;

        // Find the parametric t where the line exits the rectangle
        double tX = Math.Abs(dx) > 0.001 ? halfW / Math.Abs(dx) : double.MaxValue;
        double tY = Math.Abs(dy) > 0.001 ? halfH / Math.Abs(dy) : double.MaxValue;
        double t = Math.Min(tX, tY);

        return new Point(center.X + dx * t, center.Y + dy * t);
    }
}

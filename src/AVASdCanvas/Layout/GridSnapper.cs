using Avalonia;

namespace AVASdCanvas.Layout;

/// <summary>
/// Snaps a point to the nearest grid intersection.
/// </summary>
public static class GridSnapper
{
    public static Point Snap(Point point, double gridSpacing)
    {
        if (gridSpacing <= 0) return point;
        return new Point(
            Math.Round(point.X / gridSpacing) * gridSpacing,
            Math.Round(point.Y / gridSpacing) * gridSpacing);
    }

    public static double Snap(double value, double gridSpacing)
    {
        if (gridSpacing <= 0) return value;
        return Math.Round(value / gridSpacing) * gridSpacing;
    }
}

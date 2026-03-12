using Avalonia;
using AVASdCanvas.Models;

namespace AVASdCanvas.Layout;

/// <summary>
/// Assigns positions to entities that don't have explicit coordinates.
/// Places entities left-to-right, top-to-bottom in a grid layout.
/// </summary>
public class AutoPlacementStrategy
{
    private const double Padding = 20;

    public void PlaceEntities(IReadOnlyList<GraphicEntity> entities, Size canvasBounds, double gridSpacing, bool snapToGrid)
    {
        double currentX = Padding;
        double currentY = Padding;
        double rowHeight = 0;

        foreach (var entity in entities)
        {
            if (!double.IsNaN(entity.X) && !double.IsNaN(entity.Y))
            {
                // Already placed — just track row height in case we share a row
                continue;
            }

            // Check if we need to wrap to the next row
            if (currentX + entity.Width + Padding > canvasBounds.Width && currentX > Padding)
            {
                currentX = Padding;
                currentY += rowHeight + Padding;
                rowHeight = 0;
            }

            double x = currentX;
            double y = currentY;

            if (snapToGrid && gridSpacing > 0)
            {
                x = GridSnapper.Snap(x, gridSpacing);
                y = GridSnapper.Snap(y, gridSpacing);
            }

            entity.X = x;
            entity.Y = y;

            currentX += entity.Width + Padding;
            rowHeight = Math.Max(rowHeight, entity.Height);
        }
    }
}

using Avalonia;
using AVASdCanvas.Models;

namespace AVASdCanvas.HitTesting;

/// <summary>
/// Performs hit testing against canvas elements.
/// </summary>
public class HitTestService
{
    /// <summary>
    /// Returns the topmost entity under the given point, or null if none.
    /// Iterates in reverse order (last drawn = topmost).
    /// </summary>
    public GraphicEntity? HitTestEntity(Point point, IReadOnlyList<GraphicEntity> entities)
    {
        for (int i = entities.Count - 1; i >= 0; i--)
        {
            var entity = entities[i];
            if (double.IsNaN(entity.X) || double.IsNaN(entity.Y))
                continue;

            if (entity.Bounds.Contains(point))
                return entity;
        }
        return null;
    }

    /// <summary>
    /// Returns the connector nearest to the point within the given tolerance, or null.
    /// </summary>
    public Connector? HitTestConnector(Point point, IReadOnlyList<Connector> connectors,
        IReadOnlyList<GraphicEntity> entities, double tolerance = 4.0)
    {
        var entityLookup = new Dictionary<string, GraphicEntity>(entities.Count);
        foreach (var e in entities)
            entityLookup[e.Id] = e;

        Connector? closest = null;
        double closestDist = tolerance;

        foreach (var connector in connectors)
        {
            if (!entityLookup.TryGetValue(connector.SourceEntityId, out var source) ||
                !entityLookup.TryGetValue(connector.TargetEntityId, out var target))
                continue;

            double dist = PointToSegmentDistance(point, source.Center, target.Center);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = connector;
            }
        }

        return closest;
    }

    private static double PointToSegmentDistance(Point p, Point a, Point b)
    {
        double dx = b.X - a.X;
        double dy = b.Y - a.Y;
        double lengthSq = dx * dx + dy * dy;

        if (lengthSq < 0.001)
            return Math.Sqrt((p.X - a.X) * (p.X - a.X) + (p.Y - a.Y) * (p.Y - a.Y));

        double t = Math.Clamp(((p.X - a.X) * dx + (p.Y - a.Y) * dy) / lengthSq, 0, 1);
        double projX = a.X + t * dx;
        double projY = a.Y + t * dy;

        return Math.Sqrt((p.X - projX) * (p.X - projX) + (p.Y - projY) * (p.Y - projY));
    }
}

using Avalonia;
using Avalonia.Interactivity;
using AVASdCanvas.Models;

namespace AVASdCanvas.Events;

/// <summary>
/// Event arguments for canvas element interaction events.
/// </summary>
public class CanvasElementEventArgs : RoutedEventArgs
{
    public CanvasElementEventArgs(RoutedEvent routedEvent, GraphicEntity? entity, Connector? connector, Point canvasPosition)
        : base(routedEvent)
    {
        Entity = entity;
        Connector = connector;
        CanvasPosition = canvasPosition;
    }

    /// <summary>
    /// The entity that was interacted with, or null if the interaction was on a connector or empty space.
    /// </summary>
    public GraphicEntity? Entity { get; }

    /// <summary>
    /// The connector that was interacted with, or null.
    /// </summary>
    public Connector? Connector { get; }

    /// <summary>
    /// The position on the canvas where the interaction occurred.
    /// </summary>
    public Point CanvasPosition { get; }
}

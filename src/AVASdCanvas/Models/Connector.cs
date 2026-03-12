using Avalonia;
using Avalonia.Media;

namespace AVASdCanvas.Models;

/// <summary>
/// Represents a line connecting two GraphicEntity elements on the canvas.
/// </summary>
public class Connector : AvaloniaObject
{
    public static readonly StyledProperty<string> SourceEntityIdProperty =
        AvaloniaProperty.Register<Connector, string>(nameof(SourceEntityId), string.Empty);

    public static readonly StyledProperty<string> TargetEntityIdProperty =
        AvaloniaProperty.Register<Connector, string>(nameof(TargetEntityId), string.Empty);

    public static readonly StyledProperty<IBrush?> LineBrushProperty =
        AvaloniaProperty.Register<Connector, IBrush?>(nameof(LineBrush), Brushes.DarkSlateGray);

    public static readonly StyledProperty<double> LineThicknessProperty =
        AvaloniaProperty.Register<Connector, double>(nameof(LineThickness), 2.0);

    public static readonly StyledProperty<EndpointStyle> SourceEndpointProperty =
        AvaloniaProperty.Register<Connector, EndpointStyle>(nameof(SourceEndpoint), EndpointStyle.None);

    public static readonly StyledProperty<EndpointStyle> TargetEndpointProperty =
        AvaloniaProperty.Register<Connector, EndpointStyle>(nameof(TargetEndpoint), EndpointStyle.PointedArrow);

    public string Id { get; } = Guid.NewGuid().ToString("N");

    public string SourceEntityId
    {
        get => GetValue(SourceEntityIdProperty);
        set => SetValue(SourceEntityIdProperty, value);
    }

    public string TargetEntityId
    {
        get => GetValue(TargetEntityIdProperty);
        set => SetValue(TargetEntityIdProperty, value);
    }

    public IBrush? LineBrush
    {
        get => GetValue(LineBrushProperty);
        set => SetValue(LineBrushProperty, value);
    }

    public double LineThickness
    {
        get => GetValue(LineThicknessProperty);
        set => SetValue(LineThicknessProperty, value);
    }

    public EndpointStyle SourceEndpoint
    {
        get => GetValue(SourceEndpointProperty);
        set => SetValue(SourceEndpointProperty, value);
    }

    public EndpointStyle TargetEndpoint
    {
        get => GetValue(TargetEndpointProperty);
        set => SetValue(TargetEndpointProperty, value);
    }

    /// <summary>
    /// Arbitrary user-defined object attached to this connector.
    /// </summary>
    public object? Metadata { get; set; }
}

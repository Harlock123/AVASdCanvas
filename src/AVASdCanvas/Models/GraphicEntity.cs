using Avalonia;
using Avalonia.Media;

namespace AVASdCanvas.Models;

/// <summary>
/// Represents a labeled graphic element on the structured diagram canvas.
/// Set X/Y to double.NaN for automatic placement.
/// </summary>
public class GraphicEntity : AvaloniaObject
{
    public static readonly StyledProperty<double> XProperty =
        AvaloniaProperty.Register<GraphicEntity, double>(nameof(X), double.NaN);

    public static readonly StyledProperty<double> YProperty =
        AvaloniaProperty.Register<GraphicEntity, double>(nameof(Y), double.NaN);

    public static readonly StyledProperty<double> WidthProperty =
        AvaloniaProperty.Register<GraphicEntity, double>(nameof(Width), 120);

    public static readonly StyledProperty<double> HeightProperty =
        AvaloniaProperty.Register<GraphicEntity, double>(nameof(Height), 60);

    public static readonly StyledProperty<string> LabelProperty =
        AvaloniaProperty.Register<GraphicEntity, string>(nameof(Label), string.Empty);

    public static readonly StyledProperty<IBrush?> FillProperty =
        AvaloniaProperty.Register<GraphicEntity, IBrush?>(nameof(Fill), Brushes.LightSteelBlue);

    public static readonly StyledProperty<IBrush?> StrokeProperty =
        AvaloniaProperty.Register<GraphicEntity, IBrush?>(nameof(Stroke), Brushes.DarkSlateBlue);

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<GraphicEntity, double>(nameof(StrokeThickness), 2.0);

    public static readonly StyledProperty<IBrush?> LabelBrushProperty =
        AvaloniaProperty.Register<GraphicEntity, IBrush?>(nameof(LabelBrush), Brushes.Black);

    public static readonly StyledProperty<double> CornerRadiusProperty =
        AvaloniaProperty.Register<GraphicEntity, double>(nameof(CornerRadius), 6.0);

    public string Id { get; } = Guid.NewGuid().ToString("N");

    public double X
    {
        get => GetValue(XProperty);
        set => SetValue(XProperty, value);
    }

    public double Y
    {
        get => GetValue(YProperty);
        set => SetValue(YProperty, value);
    }

    public double Width
    {
        get => GetValue(WidthProperty);
        set => SetValue(WidthProperty, value);
    }

    public double Height
    {
        get => GetValue(HeightProperty);
        set => SetValue(HeightProperty, value);
    }

    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public IBrush? Fill
    {
        get => GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    public IBrush? Stroke
    {
        get => GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public IBrush? LabelBrush
    {
        get => GetValue(LabelBrushProperty);
        set => SetValue(LabelBrushProperty, value);
    }

    public double CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>
    /// Gets the bounding rectangle of this entity.
    /// </summary>
    public Rect Bounds => new(X, Y, Width, Height);

    /// <summary>
    /// Gets the center point of this entity.
    /// </summary>
    public Point Center => new(X + Width / 2, Y + Height / 2);

    /// <summary>
    /// Arbitrary user-defined object attached to this entity.
    /// Use this to associate domain data (e.g. a model object, DTO, or view-model)
    /// with the visual element. Retrieve it from any entity reference, including
    /// those returned by mouse interaction events or by iterating Canvas.Entities.
    /// </summary>
    public object? Metadata { get; set; }
}

using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using AVASdCanvas.Events;
using AVASdCanvas.HitTesting;
using AVASdCanvas.Layout;
using AVASdCanvas.Models;
using AVASdCanvas.Rendering;

namespace AVASdCanvas.Controls;

/// <summary>
/// A canvas control for rendering structured diagrams with colorized labeled entities,
/// connectors, and a configurable graph-paper grid background.
/// </summary>
public class SdCanvas : Control
{
    #region Styled Properties

    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<SdCanvas, IBrush?>(nameof(Background), Brushes.White);

    public static readonly StyledProperty<IBrush> GridLineBrushProperty =
        AvaloniaProperty.Register<SdCanvas, IBrush>(nameof(GridLineBrush),
            new SolidColorBrush(Color.Parse("#1B1464")));

    public static readonly StyledProperty<double> GridSpacingProperty =
        AvaloniaProperty.Register<SdCanvas, double>(nameof(GridSpacing), 20.0);

    public static readonly StyledProperty<bool> IsGridVisibleProperty =
        AvaloniaProperty.Register<SdCanvas, bool>(nameof(IsGridVisible), true);

    public static readonly StyledProperty<bool> SnapToGridProperty =
        AvaloniaProperty.Register<SdCanvas, bool>(nameof(SnapToGrid), true);

    #endregion

    #region Routed Events

    public static readonly RoutedEvent<CanvasElementEventArgs> ElementClickEvent =
        RoutedEvent.Register<SdCanvas, CanvasElementEventArgs>(nameof(ElementClick), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<CanvasElementEventArgs> ElementDoubleClickEvent =
        RoutedEvent.Register<SdCanvas, CanvasElementEventArgs>(nameof(ElementDoubleClick), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<CanvasElementEventArgs> ElementRightClickEvent =
        RoutedEvent.Register<SdCanvas, CanvasElementEventArgs>(nameof(ElementRightClick), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<CanvasElementEventArgs> ElementHoverEvent =
        RoutedEvent.Register<SdCanvas, CanvasElementEventArgs>(nameof(ElementHover), RoutingStrategies.Bubble);

    public event EventHandler<CanvasElementEventArgs> ElementClick
    {
        add => AddHandler(ElementClickEvent, value);
        remove => RemoveHandler(ElementClickEvent, value);
    }

    public event EventHandler<CanvasElementEventArgs> ElementDoubleClick
    {
        add => AddHandler(ElementDoubleClickEvent, value);
        remove => RemoveHandler(ElementDoubleClickEvent, value);
    }

    public event EventHandler<CanvasElementEventArgs> ElementRightClick
    {
        add => AddHandler(ElementRightClickEvent, value);
        remove => RemoveHandler(ElementRightClickEvent, value);
    }

    public event EventHandler<CanvasElementEventArgs> ElementHover
    {
        add => AddHandler(ElementHoverEvent, value);
        remove => RemoveHandler(ElementHoverEvent, value);
    }

    #endregion

    private readonly AvaloniaList<GraphicEntity> _entities = new();
    private readonly AvaloniaList<Connector> _connectors = new();
    private readonly HitTestService _hitTestService = new();
    private readonly AutoPlacementStrategy _autoPlacement = new();

    private GraphicEntity? _hoveredEntity;
    private Dictionary<string, GraphicEntity>? _entityLookup;

    // Drag state
    private GraphicEntity? _draggedEntity;
    private Point _dragOffset;
    private Point _dragStartPos;
    private bool _isDragging;
    private const double DragThreshold = 4.0;

    public SdCanvas()
    {
        ClipToBounds = true;

        _entities.CollectionChanged += (_, _) =>
        {
            _entityLookup = null;
            InvalidateVisual();
        };
        _connectors.CollectionChanged += (_, _) => InvalidateVisual();
    }

    #region Properties

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public IBrush GridLineBrush
    {
        get => GetValue(GridLineBrushProperty);
        set => SetValue(GridLineBrushProperty, value);
    }

    public double GridSpacing
    {
        get => GetValue(GridSpacingProperty);
        set => SetValue(GridSpacingProperty, value);
    }

    public bool IsGridVisible
    {
        get => GetValue(IsGridVisibleProperty);
        set => SetValue(IsGridVisibleProperty, value);
    }

    public bool SnapToGrid
    {
        get => GetValue(SnapToGridProperty);
        set => SetValue(SnapToGridProperty, value);
    }

    /// <summary>
    /// The collection of graphic entities displayed on the canvas.
    /// </summary>
    public AvaloniaList<GraphicEntity> Entities => _entities;

    /// <summary>
    /// The collection of connectors between entities.
    /// </summary>
    public AvaloniaList<Connector> Connectors => _connectors;

    #endregion

    #region Public API

    /// <summary>
    /// Adds an entity at the specified coordinates, optionally snapping to grid.
    /// </summary>
    public GraphicEntity AddEntity(string label, double x, double y,
        double width = 120, double height = 60, IBrush? fill = null, IBrush? stroke = null,
        object? metadata = null)
    {
        if (SnapToGrid && GridSpacing > 0)
        {
            x = GridSnapper.Snap(x, GridSpacing);
            y = GridSnapper.Snap(y, GridSpacing);
        }

        var entity = new GraphicEntity
        {
            Label = label,
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Metadata = metadata
        };

        if (fill != null) entity.Fill = fill;
        if (stroke != null) entity.Stroke = stroke;

        _entities.Add(entity);
        return entity;
    }

    /// <summary>
    /// Adds an entity with automatic placement at the next available position.
    /// </summary>
    public GraphicEntity AddEntity(string label, double width = 120, double height = 60,
        IBrush? fill = null, IBrush? stroke = null, object? metadata = null)
    {
        var entity = new GraphicEntity
        {
            Label = label,
            Width = width,
            Height = height,
            Metadata = metadata
        };

        if (fill != null) entity.Fill = fill;
        if (stroke != null) entity.Stroke = stroke;

        _entities.Add(entity);
        return entity;
    }

    /// <summary>
    /// Adds a connector between two entities.
    /// </summary>
    public Connector AddConnector(GraphicEntity source, GraphicEntity target,
        EndpointStyle sourceEndpoint = EndpointStyle.None,
        EndpointStyle targetEndpoint = EndpointStyle.PointedArrow,
        IBrush? lineBrush = null, object? metadata = null)
    {
        var connector = new Connector
        {
            SourceEntityId = source.Id,
            TargetEntityId = target.Id,
            SourceEndpoint = sourceEndpoint,
            TargetEndpoint = targetEndpoint,
            Metadata = metadata
        };

        if (lineBrush != null) connector.LineBrush = lineBrush;

        _connectors.Add(connector);
        return connector;
    }

    /// <summary>
    /// Adds a connector between two entities by ID.
    /// </summary>
    public Connector AddConnector(string sourceId, string targetId,
        EndpointStyle sourceEndpoint = EndpointStyle.None,
        EndpointStyle targetEndpoint = EndpointStyle.PointedArrow,
        IBrush? lineBrush = null, object? metadata = null)
    {
        var connector = new Connector
        {
            SourceEntityId = sourceId,
            TargetEntityId = targetId,
            SourceEndpoint = sourceEndpoint,
            TargetEndpoint = targetEndpoint,
            Metadata = metadata
        };

        if (lineBrush != null) connector.LineBrush = lineBrush;

        _connectors.Add(connector);
        return connector;
    }

    /// <summary>
    /// Removes an entity and all its connected connectors.
    /// </summary>
    public void RemoveEntity(GraphicEntity entity)
    {
        var toRemove = _connectors
            .Where(c => c.SourceEntityId == entity.Id || c.TargetEntityId == entity.Id)
            .ToList();

        foreach (var c in toRemove)
            _connectors.Remove(c);

        _entities.Remove(entity);
    }

    /// <summary>
    /// Removes a connector.
    /// </summary>
    public void RemoveConnector(Connector connector)
    {
        _connectors.Remove(connector);
    }

    /// <summary>
    /// Clears all entities and connectors.
    /// </summary>
    public void Clear()
    {
        _connectors.Clear();
        _entities.Clear();
    }

    /// <summary>
    /// Finds an entity by its ID.
    /// </summary>
    public GraphicEntity? FindEntity(string id)
    {
        return GetEntityLookup().GetValueOrDefault(id);
    }

    #endregion

    #region Rendering

    public override void Render(DrawingContext context)
    {
        var bounds = Bounds.Size;

        // 1. Background
        if (Background is { } bg)
        {
            context.DrawRectangle(bg, null, new Rect(bounds));
        }

        // 2. Grid
        if (IsGridVisible)
        {
            GridRenderer.Render(context, bounds, GridLineBrush, GridSpacing);
        }

        // 3. Auto-placement pass
        _autoPlacement.PlaceEntities(_entities, bounds, GridSpacing, SnapToGrid);

        // Build lookup for connector rendering
        var lookup = GetEntityLookup();

        // 4. Connectors (drawn first so entities appear on top)
        foreach (var connector in _connectors)
        {
            if (lookup.TryGetValue(connector.SourceEntityId, out var source) &&
                lookup.TryGetValue(connector.TargetEntityId, out var target))
            {
                ConnectorRenderer.Render(context, connector, source, target);
            }
        }

        // 5. Entities
        foreach (var entity in _entities)
        {
            if (double.IsNaN(entity.X) || double.IsNaN(entity.Y))
                continue;

            bool isHovered = ReferenceEquals(entity, _hoveredEntity);
            EntityRenderer.Render(context, entity, isHovered);
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return availableSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        return finalSize;
    }

    #endregion

    #region Pointer Events

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        var pos = e.GetPosition(this);

        // Handle dragging
        if (_draggedEntity != null)
        {
            // Check if we've moved past the drag threshold to start actual dragging
            if (!_isDragging)
            {
                var delta = pos - _dragStartPos;
                if (Math.Abs(delta.X) >= DragThreshold || Math.Abs(delta.Y) >= DragThreshold)
                {
                    _isDragging = true;
                    Cursor = new Cursor(StandardCursorType.DragMove);
                }
                else
                {
                    return;
                }
            }

            // Move the entity: new position = mouse position minus the grab offset
            _draggedEntity.X = pos.X - _dragOffset.X;
            _draggedEntity.Y = pos.Y - _dragOffset.Y;

            // Invalidate to redraw entity + all connected connectors
            InvalidateVisual();
            return;
        }

        // Normal hover tracking when not dragging
        var hitEntity = _hitTestService.HitTestEntity(pos, _entities);

        if (!ReferenceEquals(hitEntity, _hoveredEntity))
        {
            _hoveredEntity = hitEntity;
            InvalidateVisual();

            Cursor = hitEntity != null ? new Cursor(StandardCursorType.Hand) : Cursor.Default;

            RaiseEvent(new CanvasElementEventArgs(ElementHoverEvent, hitEntity, null, pos));
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var pos = e.GetPosition(this);
        var properties = e.GetCurrentPoint(this).Properties;

        var hitEntity = _hitTestService.HitTestEntity(pos, _entities);
        var hitConnector = hitEntity == null
            ? _hitTestService.HitTestConnector(pos, _connectors, _entities)
            : null;

        if (properties.IsRightButtonPressed)
        {
            RaiseEvent(new CanvasElementEventArgs(ElementRightClickEvent, hitEntity, hitConnector, pos));
        }
        else if (properties.IsLeftButtonPressed)
        {
            if (e.ClickCount == 2)
            {
                RaiseEvent(new CanvasElementEventArgs(ElementDoubleClickEvent, hitEntity, hitConnector, pos));
            }
            else if (hitEntity != null)
            {
                // Begin potential drag — capture the pointer and record the offset
                _draggedEntity = hitEntity;
                _dragOffset = new Point(pos.X - hitEntity.X, pos.Y - hitEntity.Y);
                _dragStartPos = pos;
                _isDragging = false;
                e.Pointer.Capture(this);
            }
            else
            {
                RaiseEvent(new CanvasElementEventArgs(ElementClickEvent, null, hitConnector, pos));
            }
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_draggedEntity != null)
        {
            var pos = e.GetPosition(this);

            if (_isDragging)
            {
                // Snap to grid on drop if enabled
                if (SnapToGrid && GridSpacing > 0)
                {
                    _draggedEntity.X = GridSnapper.Snap(_draggedEntity.X, GridSpacing);
                    _draggedEntity.Y = GridSnapper.Snap(_draggedEntity.Y, GridSpacing);
                }

                InvalidateVisual();
            }
            else
            {
                // Never moved past threshold — treat as a click
                var hitConnector = _hitTestService.HitTestConnector(pos, _connectors, _entities);
                RaiseEvent(new CanvasElementEventArgs(ElementClickEvent, _draggedEntity, hitConnector, pos));
            }

            // Reset drag state
            _draggedEntity = null;
            _isDragging = false;
            e.Pointer.Capture(null);
            Cursor = _hoveredEntity != null ? new Cursor(StandardCursorType.Hand) : Cursor.Default;
        }
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);

        // Don't clear hover during an active drag (pointer is captured)
        if (_draggedEntity != null)
            return;

        if (_hoveredEntity != null)
        {
            _hoveredEntity = null;
            Cursor = Cursor.Default;
            InvalidateVisual();
        }
    }

    #endregion

    #region Property Changed

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == BackgroundProperty ||
            change.Property == GridLineBrushProperty ||
            change.Property == GridSpacingProperty ||
            change.Property == IsGridVisibleProperty ||
            change.Property == SnapToGridProperty)
        {
            InvalidateVisual();
        }
    }

    #endregion

    private Dictionary<string, GraphicEntity> GetEntityLookup()
    {
        if (_entityLookup != null) return _entityLookup;

        _entityLookup = new Dictionary<string, GraphicEntity>(_entities.Count);
        foreach (var e in _entities)
            _entityLookup[e.Id] = e;

        return _entityLookup;
    }
}

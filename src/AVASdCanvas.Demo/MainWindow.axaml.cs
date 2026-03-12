using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using AVASdCanvas.Controls;
using AVASdCanvas.Events;
using AVASdCanvas.Models;

namespace AVASdCanvas.Demo;

/// <summary>
/// Sample metadata representing a step in a workflow diagram.
/// </summary>
public record WorkflowStep(
    string StepType,
    string Description,
    string Owner,
    int EstimatedMinutes,
    bool IsAutomated
);

/// <summary>
/// Sample metadata representing a connection between workflow steps.
/// </summary>
public record WorkflowTransition(
    string TransitionType,
    string Condition,
    int Priority
);

public partial class MainWindow : Window
{
    private int _entityCount;
    private readonly Random _random = new();

    private static readonly IBrush[] EntityColors =
    [
        new SolidColorBrush(Color.Parse("#6C9BCF")),
        new SolidColorBrush(Color.Parse("#A8D5BA")),
        new SolidColorBrush(Color.Parse("#F5C87A")),
        new SolidColorBrush(Color.Parse("#E8A0BF")),
        new SolidColorBrush(Color.Parse("#C4A5DE")),
        new SolidColorBrush(Color.Parse("#87CEEB")),
        new SolidColorBrush(Color.Parse("#FFB347")),
    ];

    public MainWindow()
    {
        InitializeComponent();
        WireUpEvents();
        CreateSampleDiagram();
    }

    private void WireUpEvents()
    {
        BtnAddEntity.Click += OnAddEntity;
        BtnAddConnector.Click += OnAddConnector;
        BtnClear.Click += OnClear;

        BtnToggleGrid.IsCheckedChanged += (_, _) =>
            Canvas.IsGridVisible = BtnToggleGrid.IsChecked == true;

        BtnToggleSnap.IsCheckedChanged += (_, _) =>
            Canvas.SnapToGrid = BtnToggleSnap.IsChecked == true;

        SpacingInput.ValueChanged += (_, _) =>
        {
            if (SpacingInput.Value.HasValue)
                Canvas.GridSpacing = (double)SpacingInput.Value.Value;
        };

        Canvas.ElementClick += OnElementClick;
        Canvas.ElementDoubleClick += OnElementDoubleClick;
        Canvas.ElementRightClick += OnElementRightClick;
        Canvas.ElementHover += OnElementHover;
    }

    private void CreateSampleDiagram()
    {
        var start = Canvas.AddEntity("Start", 60, 40, 120, 60, new SolidColorBrush(Color.Parse("#A8D5BA")),
            metadata: new WorkflowStep("Trigger", "Workflow initiated by incoming request", "System", 0, true));
        var process = Canvas.AddEntity("Process Data", 240, 40, 120, 60, new SolidColorBrush(Color.Parse("#6C9BCF")),
            metadata: new WorkflowStep("Transform", "Parse and normalize input payload", "DataTeam", 5, true));
        var decision = Canvas.AddEntity("Valid?", 440, 40, 100, 60, new SolidColorBrush(Color.Parse("#F5C87A")),
            metadata: new WorkflowStep("Decision", "Validate against business rules", "RulesEngine", 2, true));
        var output = Canvas.AddEntity("Output", 440, 160, 120, 60, new SolidColorBrush(Color.Parse("#C4A5DE")),
            metadata: new WorkflowStep("Sink", "Write results to output database", "DataTeam", 3, true));
        var error = Canvas.AddEntity("Error", 240, 160, 120, 60, new SolidColorBrush(Color.Parse("#E8A0BF")),
            metadata: new WorkflowStep("ErrorHandler", "Log failure and notify ops team", "OpsTeam", 1, false));
        var end = Canvas.AddEntity("End", 440, 280, 120, 60, new SolidColorBrush(Color.Parse("#A8D5BA")),
            metadata: new WorkflowStep("Terminal", "Workflow complete, emit metrics", "System", 0, true));

        Canvas.AddConnector(start, process, targetEndpoint: EndpointStyle.PointedArrow,
            metadata: new WorkflowTransition("Sequential", "Always", 1));
        Canvas.AddConnector(process, decision, targetEndpoint: EndpointStyle.PointedArrow,
            metadata: new WorkflowTransition("Sequential", "Always", 1));
        Canvas.AddConnector(decision, output, targetEndpoint: EndpointStyle.PointedArrow,
            metadata: new WorkflowTransition("Conditional", "Validation passed", 1));
        Canvas.AddConnector(decision, error, targetEndpoint: EndpointStyle.PointedArrow,
            lineBrush: new SolidColorBrush(Color.Parse("#CC4444")),
            metadata: new WorkflowTransition("Conditional", "Validation failed", 2));
        Canvas.AddConnector(error, process, sourceEndpoint: EndpointStyle.RoundDot,
            targetEndpoint: EndpointStyle.PointedArrow,
            lineBrush: new SolidColorBrush(Color.Parse("#CC4444")),
            metadata: new WorkflowTransition("Retry", "Max 3 attempts", 1));
        Canvas.AddConnector(output, end, targetEndpoint: EndpointStyle.PointedArrow,
            metadata: new WorkflowTransition("Sequential", "Always", 1));

        _entityCount = 6;
    }

    private void OnAddEntity(object? sender, RoutedEventArgs e)
    {
        _entityCount++;
        var fill = EntityColors[_random.Next(EntityColors.Length)];
        Canvas.AddEntity($"Entity {_entityCount}", fill: fill,
            metadata: new WorkflowStep("Task", $"Dynamically added step #{_entityCount}",
                "User", _random.Next(1, 30), _random.Next(2) == 0));
        StatusText.Text = $"Added Entity {_entityCount}";
    }

    private void OnAddConnector(object? sender, RoutedEventArgs e)
    {
        var entities = Canvas.Entities;
        if (entities.Count < 2)
        {
            StatusText.Text = "Need at least 2 entities to create a connector";
            return;
        }

        var source = entities[_random.Next(entities.Count)];
        GraphicEntity target;
        do { target = entities[_random.Next(entities.Count)]; }
        while (ReferenceEquals(source, target));

        Canvas.AddConnector(source, target, targetEndpoint: EndpointStyle.PointedArrow);
        StatusText.Text = $"Connected '{source.Label}' → '{target.Label}'";
    }

    private void OnClear(object? sender, RoutedEventArgs e)
    {
        // Demonstrate iterating entities and reading metadata before clearing
        int automated = 0, manual = 0;
        foreach (var entity in Canvas.Entities)
        {
            if (entity.Metadata is WorkflowStep step)
            {
                if (step.IsAutomated) automated++;
                else manual++;
            }
        }

        Canvas.Clear();
        _entityCount = 0;
        StatusText.Text = $"Cleared {automated + manual} entities ({automated} automated, {manual} manual)";
    }

    private static string FormatMetadata(GraphicEntity entity)
    {
        if (entity.Metadata is WorkflowStep step)
            return $"[{step.StepType}] {step.Description} (Owner: {step.Owner}, ~{step.EstimatedMinutes}min, Automated: {step.IsAutomated})";
        return "(no metadata)";
    }

    private void OnElementClick(object? sender, CanvasElementEventArgs e)
    {
        if (e.Entity != null)
            StatusText.Text = $"Clicked: {e.Entity.Label} — {FormatMetadata(e.Entity)}";
        else if (e.Connector != null)
        {
            var meta = e.Connector.Metadata is WorkflowTransition t
                ? $"[{t.TransitionType}] Condition: {t.Condition}, Priority: {t.Priority}"
                : "(no metadata)";
            StatusText.Text = $"Clicked connector — {meta}";
        }
        else
            StatusText.Text = $"Clicked canvas at ({e.CanvasPosition.X:F0}, {e.CanvasPosition.Y:F0})";
    }

    private void OnElementDoubleClick(object? sender, CanvasElementEventArgs e)
    {
        if (e.Entity != null)
            StatusText.Text = $"Double-clicked: {e.Entity.Label} — {FormatMetadata(e.Entity)}";
    }

    private void OnElementRightClick(object? sender, CanvasElementEventArgs e)
    {
        if (e.Entity != null)
            StatusText.Text = $"Right-clicked: {e.Entity.Label} — {FormatMetadata(e.Entity)}";
        else
            StatusText.Text = $"Right-clicked canvas at ({e.CanvasPosition.X:F0}, {e.CanvasPosition.Y:F0})";
    }

    private void OnElementHover(object? sender, CanvasElementEventArgs e)
    {
        if (e.Entity != null)
            StatusText.Text = $"Hover: {e.Entity.Label} — {FormatMetadata(e.Entity)}";
    }
}

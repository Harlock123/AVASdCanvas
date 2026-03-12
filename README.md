# AVASdCanvas

An [Avalonia UI](https://avaloniaui.net/) canvas control for rendering interactive **structured diagrams**. Place colorized, labeled entities on a configurable graph-paper grid, connect them with lines sporting round-dot or pointed-arrow endpoints, drag them around, and attach arbitrary metadata to every element.

Built on .NET 9 and Avalonia 11.

![Demo screenshot](/Images/demo-overview.png)

---

## Features

| Feature | Details |
|---|---|
| **Graph-paper grid** | Configurable line color (dark blue default), spacing (20 px default), visibility toggle |
| **Paper-white background** | Adjustable via `Background` brush |
| **Entities** | Rounded rectangles with label, fill, stroke, corner radius — all configurable |
| **Connectors** | Lines between entities with `RoundDot`, `PointedArrow`, or `None` endpoints at each end |
| **Snap to grid** | Entities snap to nearest grid intersection on placement and after drag |
| **Auto-placement** | Add entities without coordinates — they flow left-to-right, top-to-bottom |
| **Drag and drop** | Left-click and drag entities; connectors follow in real time |
| **Hit testing** | Point-in-rectangle for entities, point-to-segment for connectors |
| **Mouse events** | `ElementClick`, `ElementDoubleClick`, `ElementRightClick`, `ElementHover` — all bubbling routed events |
| **Metadata** | Attach any `object` to entities and connectors; retrieve via events or iteration |
| **Resizable** | Fills its container and redraws on resize |

---

## Screenshots

| Overview | Drag in progress | Hover with metadata |
|---|---|---|
| ![Overview](https://raw.githubusercontent.com/harlock123/AVASdCanvas/main/Images/demo-overview.png) | ![Dragging](https://raw.githubusercontent.com/harlock123/AVASdCanvas/main/Images/demo-drag.png) | ![Hover](https://raw.githubusercontent.com/harlock123/AVASdCanvas/main/Images/demo-hover.png) |

> **Note:** To populate these screenshots, run the demo app, capture the images, and save them to the `Images/` folder.

---

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Build

```bash
dotnet build
```

### Run the demo

```bash
dotnet run --project src/AVASdCanvas.Demo
```

### Install from NuGet

```bash
dotnet add package AVASdCanvas
```

Or via the Package Manager Console:

```powershell
Install-Package AVASdCanvas
```

---

## Usage

### XAML

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:sd="clr-namespace:AVASdCanvas.Controls;assembly=AVASdCanvas">

    <sd:SdCanvas x:Name="Canvas"
                 Background="White"
                 GridLineBrush="#1B1464"
                 GridSpacing="20"
                 IsGridVisible="True"
                 SnapToGrid="True" />
</Window>
```

### Adding entities

```csharp
// At explicit coordinates
var server = canvas.AddEntity("Web Server", 100, 60, 140, 60,
    fill: Brushes.LightSkyBlue,
    metadata: new ServerInfo("web-01", 443));

// Auto-placed (next available grid position)
var db = canvas.AddEntity("Database",
    fill: Brushes.LightGoldenrodYellow,
    metadata: new ServerInfo("db-01", 5432));
```

### Connecting entities

```csharp
canvas.AddConnector(server, db,
    sourceEndpoint: EndpointStyle.RoundDot,
    targetEndpoint: EndpointStyle.PointedArrow,
    lineBrush: Brushes.DarkSlateGray,
    metadata: new ConnectionInfo("HTTPS", 443));
```

### Handling mouse events

```csharp
canvas.ElementClick += (sender, e) =>
{
    if (e.Entity is { } entity)
    {
        Console.WriteLine($"Clicked: {entity.Label}");

        if (entity.Metadata is ServerInfo info)
            Console.WriteLine($"  Host: {info.Host}, Port: {info.Port}");
    }
};

canvas.ElementDoubleClick += (sender, e) => { /* ... */ };
canvas.ElementRightClick  += (sender, e) => { /* ... */ };
canvas.ElementHover       += (sender, e) => { /* ... */ };
```

### Iterating entities

```csharp
foreach (var entity in canvas.Entities)
{
    Console.WriteLine($"{entity.Label} at ({entity.X}, {entity.Y})");

    if (entity.Metadata is ServerInfo info)
        Console.WriteLine($"  -> {info.Host}:{info.Port}");
}
```

### Removing elements

```csharp
canvas.RemoveEntity(server);    // also removes connected connectors
canvas.RemoveConnector(conn);
canvas.Clear();                 // removes everything
```

---

## API Reference

### SdCanvas Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Background` | `IBrush?` | `Brushes.White` | Canvas background color |
| `GridLineBrush` | `IBrush` | `#1B1464` (dark blue) | Grid line color |
| `GridSpacing` | `double` | `20.0` | Grid line spacing in pixels |
| `IsGridVisible` | `bool` | `true` | Show/hide the grid |
| `SnapToGrid` | `bool` | `true` | Snap entity placement to grid |
| `Entities` | `AvaloniaList<GraphicEntity>` | empty | Observable entity collection |
| `Connectors` | `AvaloniaList<Connector>` | empty | Observable connector collection |

### SdCanvas Methods

| Method | Description |
|---|---|
| `AddEntity(label, x, y, ...)` | Add at specific coordinates |
| `AddEntity(label, ...)` | Add with auto-placement |
| `AddConnector(source, target, ...)` | Connect two entities |
| `RemoveEntity(entity)` | Remove entity and its connectors |
| `RemoveConnector(connector)` | Remove a connector |
| `FindEntity(id)` | Look up entity by ID |
| `Clear()` | Remove all entities and connectors |

### SdCanvas Events

| Event | Fires when |
|---|---|
| `ElementClick` | Left mouse button clicked on entity, connector, or canvas |
| `ElementDoubleClick` | Left mouse button double-clicked |
| `ElementRightClick` | Right mouse button clicked |
| `ElementHover` | Mouse moves over a different element |

All events provide `CanvasElementEventArgs` with `Entity`, `Connector`, and `CanvasPosition`.

### GraphicEntity Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Id` | `string` | auto-generated | Unique identifier |
| `X`, `Y` | `double` | `NaN` (auto-place) | Position on canvas |
| `Width`, `Height` | `double` | `120`, `60` | Size in pixels |
| `Label` | `string` | `""` | Display text (centered) |
| `Fill` | `IBrush?` | `LightSteelBlue` | Background color |
| `Stroke` | `IBrush?` | `DarkSlateBlue` | Border color |
| `StrokeThickness` | `double` | `2.0` | Border width |
| `LabelBrush` | `IBrush?` | `Black` | Text color |
| `CornerRadius` | `double` | `6.0` | Rounded corner radius |
| `Metadata` | `object?` | `null` | Arbitrary user data |

### Connector Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Id` | `string` | auto-generated | Unique identifier |
| `SourceEntityId` | `string` | `""` | Source entity ID |
| `TargetEntityId` | `string` | `""` | Target entity ID |
| `LineBrush` | `IBrush?` | `DarkSlateGray` | Line color |
| `LineThickness` | `double` | `2.0` | Line width |
| `SourceEndpoint` | `EndpointStyle` | `None` | Start decorator |
| `TargetEndpoint` | `EndpointStyle` | `PointedArrow` | End decorator |
| `Metadata` | `object?` | `null` | Arbitrary user data |

### EndpointStyle Enum

| Value | Renders as |
|---|---|
| `None` | Plain line end |
| `RoundDot` | Filled circle |
| `PointedArrow` | Filled triangle arrowhead |

---

## Project Structure

```
AVASdCanvas.sln
Directory.Build.props
src/
  AVASdCanvas/                          Class library (the NuGet package)
    Controls/
      SdCanvas.cs                       Main canvas control
    Models/
      GraphicEntity.cs                  Diagram node
      Connector.cs                      Line between nodes
      EndpointStyle.cs                  Endpoint decorator enum
    Rendering/
      GridRenderer.cs                   Graph-paper grid
      EntityRenderer.cs                 Rounded rectangle + label
      ConnectorRenderer.cs              Lines + arrows/dots
    HitTesting/
      HitTestService.cs                 Point-in-rect, point-to-segment
    Layout/
      GridSnapper.cs                    Snap to grid math
      AutoPlacementStrategy.cs          Auto-place unpositioned entities
    Events/
      CanvasElementEventArgs.cs         Routed event args
    Properties/
      AssemblyInfo.cs                   XAML namespace registration

  AVASdCanvas.Demo/                     Desktop demo app
    MainWindow.axaml/.cs                Sample diagram with metadata
    App.axaml/.cs                       Avalonia app bootstrap
    Program.cs                          Entry point
```

---

## Publishing to NuGet

The library project is pre-configured with NuGet packaging metadata. To create and publish a package:

```bash
# Pack
dotnet pack src/AVASdCanvas/AVASdCanvas.csproj -c Release

# Publish (replace YOUR_API_KEY)
dotnet nuget push src/AVASdCanvas/bin/Release/AVASdCanvas.1.0.0.nupkg \
    --api-key YOUR_API_KEY \
    --source https://api.nuget.org/v3/index.json
```

To bump the version, edit the `<Version>` in `src/AVASdCanvas/AVASdCanvas.csproj`.

Optionally, place an `icon.png` (128x128 recommended) in the `Images/` folder — it will be included in the NuGet package automatically.

---

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

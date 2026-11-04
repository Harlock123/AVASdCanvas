# AGENTS.md

Notes for anyone, human or AI agent, changing this code. `README.md` covers the features, usage and API reference;
this file covers how the code is put together and the rules that keep it working.

## What this is

AVASdCanvas, an Avalonia 11 control for structured diagrams, in C# on .NET 10. It draws labelled entities on a
graph-paper grid and joins them with connectors. Entities can be dragged, and every element can carry metadata and
raises mouse events. It is packaged for NuGet as `AVASdCanvas`; a small demo app shows it off.

## Commands

```bash
dotnet build AVASdCanvas.sln                      # no global.json: any .NET 10 or later SDK
dotnet run --project src/AVASdCanvas.Demo         # the demo window
dotnet pack src/AVASdCanvas/AVASdCanvas.csproj -c Release   # the NuGet package
```

There are no tests yet. `Directory.Build.props` sets nullable reference types and implicit usings for both
projects.

## Layout

- `src/AVASdCanvas/`: the library.
  - `Controls/SdCanvas.cs`: the control. It holds the styled properties, the routed events, the `Entities` and
    `Connectors` lists, `AddEntity`/`AddConnector`/`Remove*`/`Clear`/`FindEntity`, rendering and pointer handling
    (click, double-click, right-click, hover, drag).
  - `Models/`: `GraphicEntity` and `Connector` (both `AvaloniaObject`s with styled properties and a `Metadata`
    object), and `EndpointStyle`.
  - `Rendering/`: `GridRenderer`, `EntityRenderer` and `ConnectorRenderer` draw onto the `DrawingContext`.
  - `HitTesting/HitTestService.cs`: point-in-rectangle for entities, point-to-segment for connectors.
  - `Layout/`: `GridSnapper` and `AutoPlacementStrategy` (left-to-right, top-to-bottom placement).
  - `Events/CanvasElementEventArgs.cs`: the event arguments.
  - `Properties/AssemblyInfo.cs`: maps the XAML namespace `https://github.com/harlock123/avasdcanvas` to
    `AVASdCanvas.Controls` and `AVASdCanvas.Models`.
- `src/AVASdCanvas.Demo/`: an Avalonia app that fills a canvas with sample workflow entities and shows the
  clicked or hovered element in a status line.
- `Images/`: the README screenshot and `icon.png`, which goes into the NuGet package.

## Rules that keep it working

- **Redraws.** The canvas redraws when its own grid properties or the `Entities`/`Connectors` collections change,
  and during drags. It does not watch the properties of each entity or connector, so code that changes one in
  place must call `InvalidateVisual()` on the canvas.
- **Auto-placement.** An entity whose `X` or `Y` is `double.NaN` is placed by `AutoPlacementStrategy` when the canvas
  next renders, and snapped to the grid when `SnapToGrid` is on. Keep `NaN` as the "place it for me" signal.
- **Public API is a NuGet contract.** Renaming or removing public members breaks users of the package. Add rather
  than change, and bump `<Version>` in `src/AVASdCanvas/AVASdCanvas.csproj` for each release.
- **XML docs.** The library generates a documentation file for IntelliSense. Give new public members a
  `<summary>`.

## Making a change

- Build, run the demo, and try the change by hand: drag, click, hover and resize.
- Update the README's usage and API reference tables to match.

## CI and releases

`.github/workflows/build.yml` builds every push and packs the NuGet package, then publishes the demo as
self-contained single files for win-x64/x86/arm64, linux-x64/arm64/arm and osx-x64/arm64. Pushes to `master` refresh
the rolling `latest` pre-release; a `v*` tag makes a proper release, with the package version taken from the tag.
The `.nupkg` is attached to the release; pushing it to nuget.org is still done by hand (see the README's
"Publishing to NuGet").

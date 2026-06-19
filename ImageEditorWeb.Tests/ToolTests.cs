using ImageEditorWeb.Core.Services;
using ImageEditorWeb.Core.Tools;
using ImageEditorWeb.Shared.Models;
using ImageEditorWeb.Shared.ToolType;

namespace ImageEditorWeb.Tests;

/// <summary>
/// Юнит-тесты для инструментов BrushTool, EraserTool и менеджера ToolManager.
/// Покрывают создание штрихов, реакцию на серию событий мыши,
/// различия между кистью и ластиком, регистрацию и выбор инструментов.
/// </summary>
public class ToolTests
{
    private static (LayerService layerService, ToolSettings settings) CreateSetup(int brushSize = 8, string color = "#ff0000")
    {
        var service = new LayerService();
        var settings = new ToolSettings
        {
            BrushSize = brushSize,
            ForegroundColorHex = color,
            BackgroundColorHex = "#ffffff"
        };
        return (service, settings);
    }

    // ----------------------------- BrushTool -----------------------------

    [Fact]
    public async Task BrushTool_OnMouseDown_AddsInitialStrokeAndSavesCheckpoint()
    {
        var (service, settings) = CreateSetup();
        var tool = new BrushTool();

        await tool.OnMouseDown(new EditorPoint(10, 10), settings, service);

        Assert.Single(service.GetActiveLayer()!.Strokes);
        Assert.True(service.CanUndo());
    }

    [Fact]
    public async Task BrushTool_OnMouseMove_WithoutMouseDown_AddsNothing()
    {
        var (service, settings) = CreateSetup();
        var tool = new BrushTool();

        await tool.OnMouseMove(new EditorPoint(5, 5), settings, service);

        Assert.Empty(service.GetActiveLayer()!.Strokes);
    }

    [Fact]
    public async Task BrushTool_DragSequence_AddsConnectedStrokes()
    {
        var (service, settings) = CreateSetup();
        var tool = new BrushTool();

        await tool.OnMouseDown(new EditorPoint(0, 0), settings, service);
        await tool.OnMouseMove(new EditorPoint(10, 0), settings, service);
        await tool.OnMouseMove(new EditorPoint(10, 10), settings, service);
        await tool.OnMouseUp(new EditorPoint(20, 20), settings, service);

        var strokes = service.GetActiveLayer()!.Strokes;
        Assert.Equal(4, strokes.Count);
        Assert.All(strokes, s => Assert.False(s.IsEraser));
        Assert.All(strokes, s => Assert.Equal(settings.ForegroundColorHex, s.ColorHex));
        Assert.All(strokes, s => Assert.Equal(settings.BrushSize, s.Size));
    }

    [Fact]
    public async Task BrushTool_OnMouseUp_SamePoint_DoesNotAddExtraStroke()
    {
        var (service, settings) = CreateSetup();
        var tool = new BrushTool();
        await tool.OnMouseDown(new EditorPoint(7, 7), settings, service);
        var beforeUp = service.GetActiveLayer()!.Strokes.Count;

        await tool.OnMouseUp(new EditorPoint(7, 7), settings, service);

        Assert.Equal(beforeUp, service.GetActiveLayer()!.Strokes.Count);
    }

    // ----------------------------- EraserTool -----------------------------

    [Fact]
    public async Task EraserTool_OnMouseDown_AddsEraserStrokeWithBackgroundColor()
    {
        var (service, settings) = CreateSetup();
        var tool = new EraserTool();

        await tool.OnMouseDown(new EditorPoint(2, 2), settings, service);

        var stroke = service.GetActiveLayer()!.Strokes.Single();
        Assert.True(stroke.IsEraser);
        Assert.Equal(settings.BackgroundColorHex, stroke.ColorHex);
    }

    [Fact]
    public async Task EraserTool_DragSequence_AllStrokesAreEraser()
    {
        var (service, settings) = CreateSetup();
        var tool = new EraserTool();

        await tool.OnMouseDown(new EditorPoint(0, 0), settings, service);
        await tool.OnMouseMove(new EditorPoint(15, 0), settings, service);
        await tool.OnMouseUp(new EditorPoint(15, 15), settings, service);

        Assert.All(service.GetActiveLayer()!.Strokes, s => Assert.True(s.IsEraser));
    }

    // ----------------------------- Identity -----------------------------

    [Fact]
    public void BrushTool_HasBrushType()
    {
        var tool = new BrushTool();

        Assert.Equal(ToolType.Brush, tool.Type);
        Assert.False(string.IsNullOrWhiteSpace(tool.Name));
    }

    [Fact]
    public void EraserTool_HasEraserType()
    {
        var tool = new EraserTool();

        Assert.Equal(ToolType.Eraser, tool.Type);
    }

    // ----------------------------- ToolManager -----------------------------

    [Fact]
    public void ToolManager_DefaultRegistersBrushAndEraser()
    {
        var manager = new ToolManager();

        var names = manager.GetAllTools().Select(tool => tool.Name).ToHashSet();
        Assert.Contains("Кисть", names);
        Assert.Contains("Ластик", names);
        Assert.NotNull(manager.GetCurrentTool());
    }

    [Fact]
    public void ToolManager_SetCurrentTool_KnownName_UpdatesCurrent()
    {
        var manager = new ToolManager();

        manager.SetCurrentTool("Ластик");

        Assert.Equal("Ластик", manager.GetCurrentTool()!.Name);
    }

    [Fact]
    public void ToolManager_SetCurrentTool_UnknownName_KeepsPrevious()
    {
        var manager = new ToolManager();
        var before = manager.GetCurrentTool();

        manager.SetCurrentTool("non-existent-tool");

        Assert.Same(before, manager.GetCurrentTool());
    }

    [Fact]
    public void ToolManager_RegisterTool_AddsCustomTool()
    {
        var manager = new ToolManager();
        var custom = new BrushTool();

        manager.RegisterTool(custom);

        Assert.Contains(manager.GetAllTools(), tool => ReferenceEquals(tool, custom));
    }
}

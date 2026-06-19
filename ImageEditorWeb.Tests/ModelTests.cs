using ImageEditorWeb.Shared.Models;

namespace ImageEditorWeb.Tests;

/// <summary>
/// Юнит-тесты для shared-моделей: CanvasLayer, StrokeLine, ToolSettings,
/// LayerFilterSettings, EditorProject, EditorViewState, EditorSettings, HotkeySettings.
/// Покрывают значения по умолчанию, глубокое клонирование и независимость копий.
/// </summary>
public class ModelTests
{
    // ----------------------------- CanvasLayer -----------------------------

    [Fact]
    public void CanvasLayer_DefaultState_IsVisibleWithFullOpacity()
    {
        var layer = new CanvasLayer();

        Assert.True(layer.IsVisible);
        Assert.Equal(1.0, layer.Opacity);
        Assert.Empty(layer.Strokes);
        Assert.Null(layer.ImageDataUrl);
        Assert.False(layer.HasContent);
    }

    [Fact]
    public void CanvasLayer_HasContent_TrueWhenStrokesExist()
    {
        var layer = new CanvasLayer();
        layer.Strokes.Add(new StrokeLine());

        Assert.True(layer.HasContent);
    }

    [Fact]
    public void CanvasLayer_HasContent_TrueWhenImageDataUrlPresent()
    {
        var layer = new CanvasLayer
        {
            ImageDataUrl = "data:image/png;base64,abc"
        };

        Assert.True(layer.HasContent);
    }

    [Fact]
    public void CanvasLayer_Clone_ProducesIndependentDeepCopy()
    {
        var original = new CanvasLayer
        {
            Id = Guid.NewGuid(),
            Name = "Original",
            IsVisible = false,
            Opacity = 0.42,
            ZIndex = 7,
            ImageDataUrl = "data:image/png;base64,abc",
            Filters = new LayerFilterSettings { Brightness = 150, Contrast = 90, BlurRadius = 3 }
        };
        original.Strokes.Add(new StrokeLine { X1 = 1, Y1 = 2, X2 = 3, Y2 = 4 });

        var clone = original.Clone();
        clone.Strokes[0].X1 = 999;
        clone.Filters.Brightness = 50;
        clone.Name = "Mutated";

        Assert.Equal(1, original.Strokes[0].X1);
        Assert.Equal(150, original.Filters.Brightness);
        Assert.Equal("Original", original.Name);
        Assert.Equal(original.Id, clone.Id);
    }

    // ----------------------------- StrokeLine -----------------------------

    [Fact]
    public void StrokeLine_DefaultSizeAndColor()
    {
        var stroke = new StrokeLine();

        Assert.Equal(5, stroke.Size);
        Assert.Equal("#000000", stroke.ColorHex);
        Assert.False(stroke.IsEraser);
    }

    [Fact]
    public void StrokeLine_Clone_ReturnsSeparateInstance()
    {
        var original = new StrokeLine { X1 = 1, X2 = 2, Y1 = 3, Y2 = 4, Size = 12, ColorHex = "#abcdef", IsEraser = true };

        var clone = original.Clone();
        clone.Size = 99;

        Assert.Equal(12, original.Size);
        Assert.Equal(99, clone.Size);
    }

    // ----------------------------- LayerFilterSettings -----------------------------

    [Fact]
    public void LayerFilterSettings_Defaults_Are100PercentNoBlur()
    {
        var filters = new LayerFilterSettings();

        Assert.Equal(100, filters.Brightness);
        Assert.Equal(100, filters.Contrast);
        Assert.Equal(0, filters.BlurRadius);
    }

    // ----------------------------- ToolSettings -----------------------------

    [Fact]
    public void ToolSettings_Defaults_AreBlackOnWhiteRoundBrush()
    {
        var settings = new ToolSettings();

        Assert.Equal("#000000", settings.ForegroundColorHex);
        Assert.Equal("#FFFFFF", settings.BackgroundColorHex);
        Assert.Equal(5, settings.BrushSize);
        Assert.Equal("Round", settings.BrushType);
    }

    [Fact]
    public void ToolSettings_Clone_IsIndependent()
    {
        var settings = new ToolSettings { BrushSize = 14 };
        var clone = settings.Clone();
        clone.BrushSize = 99;

        Assert.Equal(14, settings.BrushSize);
    }

    // ----------------------------- EditorViewState -----------------------------

    [Fact]
    public void EditorViewState_DefaultZoomIsOneAndNoPan()
    {
        var state = new EditorViewState();

        Assert.Equal(1.0, state.Zoom);
        Assert.Equal(0, state.PanX);
        Assert.Equal(0, state.PanY);
    }

    [Fact]
    public void EditorViewState_Clone_ProducesIndependentCopy()
    {
        var state = new EditorViewState { Zoom = 1.5, PanX = 10, PanY = 20 };
        var clone = state.Clone();
        clone.Zoom = 4.0;

        Assert.Equal(1.5, state.Zoom);
        Assert.Equal(4.0, clone.Zoom);
    }

    // ----------------------------- EditorSettings / Hotkeys -----------------------------

    [Fact]
    public void EditorSettings_DefaultsAreReasonable()
    {
        var settings = new EditorSettings();

        Assert.True(settings.AutoSaveProjects);
        Assert.True(settings.ShowCheckerboardBackground);
        Assert.Equal("ru-RU", settings.Language);
        Assert.Equal(0.1, settings.ZoomStep);
        Assert.Equal("png", settings.DefaultExportFormat);
        Assert.NotNull(settings.Hotkeys);
    }

    [Fact]
    public void EditorSettings_Clone_IsDeepCopy()
    {
        var settings = new EditorSettings { Language = "ru-RU" };
        settings.Hotkeys.Save = "Ctrl+S";

        var clone = settings.Clone();
        clone.Language = "en-US";
        clone.Hotkeys.Save = "Ctrl+Shift+Q";

        Assert.Equal("ru-RU", settings.Language);
        Assert.Equal("Ctrl+S", settings.Hotkeys.Save);
    }

    [Fact]
    public void HotkeySettings_CreateDefault_ReturnsKnownBindings()
    {
        var hotkeys = HotkeySettings.CreateDefault();

        Assert.Equal("Ctrl+S", hotkeys.Save);
        Assert.Equal("Ctrl+Shift+S", hotkeys.SaveAs);
        Assert.Equal("Ctrl+Z", hotkeys.Undo);
        Assert.Equal("ArrowLeft", hotkeys.PanLeft);
    }

    [Fact]
    public void HotkeySettings_Clone_IsIndependent()
    {
        var hotkeys = new HotkeySettings();
        var clone = hotkeys.Clone();
        clone.Save = "Ctrl+Alt+S";

        Assert.Equal("Ctrl+S", hotkeys.Save);
    }

    // ----------------------------- EditorProject -----------------------------

    [Fact]
    public void EditorProject_HasReasonableDefaults()
    {
        var project = new EditorProject();

        Assert.NotEqual(Guid.Empty, project.Id);
        Assert.Equal("Без названия", project.Name);
        Assert.Equal(800, project.CanvasWidth);
        Assert.Equal(600, project.CanvasHeight);
        Assert.Equal("Кисть", project.CurrentTool);
    }

    [Fact]
    public void EditorProject_Clone_PerformsDeepCopyOfLayers()
    {
        var project = new EditorProject
        {
            Name = "P",
            Layers = new List<CanvasLayer>
            {
                new() { Id = Guid.NewGuid(), Name = "L1" }
            }
        };

        var clone = project.Clone();
        clone.Layers[0].Name = "Mutated";

        Assert.Equal("L1", project.Layers[0].Name);
        Assert.Equal("Mutated", clone.Layers[0].Name);
    }

    [Fact]
    public void EditorState_DefaultCurrentToolIsBrush()
    {
        var state = new EditorState();

        Assert.Equal("Кисть", state.CurrentTool);
        Assert.Equal(800, state.CanvasWidth);
        Assert.Equal(600, state.CanvasHeight);
        Assert.Empty(state.Layers);
    }
}

using ImageEditorWeb.Core.Services;
using ImageEditorWeb.Shared.Models;

namespace ImageEditorWeb.Tests;

/// <summary>
/// Дополнительные юнит-тесты для LayerService.
/// Покрывают: clamp размеров холста, перемещение слоёв по Z-индексу,
/// видимость, прозрачность, переименование, замену изображения,
/// загрузку проекта и историю undo/redo.
/// </summary>
public class LayerServiceTests
{
    // ----------------------------- Canvas size -----------------------------

    [Theory]
    [InlineData(0, 0, 1, 1)]
    [InlineData(-100, -100, 1, 1)]
    [InlineData(5000, 5000, 4096, 4096)]
    public void SetCanvasSize_ClampsValuesToAllowedRange(int width, int height, int expectedWidth, int expectedHeight)
    {
        var service = new LayerService();

        service.SetCanvasSize(width, height);

        Assert.Equal(expectedWidth, service.CanvasWidth);
        Assert.Equal(expectedHeight, service.CanvasHeight);
    }

    [Fact]
    public void SetCanvasSize_SameDimensions_DoesNotSaveHistory()
    {
        var service = new LayerService();

        service.SetCanvasSize(service.CanvasWidth, service.CanvasHeight);

        Assert.False(service.CanUndo());
    }

    [Fact]
    public void SetCanvasSize_NewDimensions_RegistersInHistory()
    {
        var service = new LayerService();

        service.SetCanvasSize(1024, 768);

        Assert.True(service.CanUndo());
    }

    // ----------------------------- Layer creation/removal -----------------------------

    [Fact]
    public void CreateEmptyLayer_UsesAutoNameWhenNotProvided()
    {
        var service = new LayerService();

        var layer = service.CreateEmptyLayer();

        Assert.False(string.IsNullOrWhiteSpace(layer.Name));
        Assert.StartsWith("Слой", layer.Name);
    }

    [Fact]
    public void CreateEmptyLayer_UsesProvidedName()
    {
        var service = new LayerService();

        var layer = service.CreateEmptyLayer("Фон");

        Assert.Equal("Фон", layer.Name);
    }

    [Fact]
    public void AddLayer_AssignsIncreasingZIndex()
    {
        var service = new LayerService();
        var first = service.CreateEmptyLayer("A");
        var second = service.CreateEmptyLayer("B");

        service.AddLayer(first);
        service.AddLayer(second);

        var ordered = service.Layers.OrderBy(layer => layer.ZIndex).ToList();
        Assert.True(ordered[^1].ZIndex > ordered[0].ZIndex);
    }

    [Fact]
    public void AddLayer_WithSetActiveFalse_KeepsPreviousActiveLayer()
    {
        var service = new LayerService();
        var previousActiveId = service.ActiveLayerId;

        service.AddLayer(service.CreateEmptyLayer("Second"), setActive: false);

        Assert.Equal(previousActiveId, service.ActiveLayerId);
    }

    [Fact]
    public void RemoveLayer_UnknownId_NoOp()
    {
        var service = new LayerService();
        var initialCount = service.Layers.Count;

        service.RemoveLayer(Guid.NewGuid());

        Assert.Equal(initialCount, service.Layers.Count);
        Assert.False(service.CanUndo());
    }

    [Fact]
    public void RemoveLayer_NonActiveLayer_KeepsActiveLayerUntouched()
    {
        var service = new LayerService();
        var second = service.CreateEmptyLayer("Second");
        service.AddLayer(second);
        var activeBefore = service.ActiveLayerId;

        service.RemoveLayer(service.Layers.First(layer => layer.Id != activeBefore).Id);

        Assert.Equal(activeBefore, service.ActiveLayerId);
    }

    // ----------------------------- Active layer -----------------------------

    [Fact]
    public void SetActiveLayer_UnknownId_KeepsCurrentActiveLayer()
    {
        var service = new LayerService();
        var originalActive = service.ActiveLayerId;

        service.SetActiveLayer(Guid.NewGuid());

        Assert.Equal(originalActive, service.ActiveLayerId);
    }

    [Fact]
    public void SetActiveLayer_KnownId_UpdatesActiveLayer()
    {
        var service = new LayerService();
        var newLayer = service.CreateEmptyLayer("Second");
        service.AddLayer(newLayer, setActive: false);

        service.SetActiveLayer(newLayer.Id);

        Assert.Equal(newLayer.Id, service.ActiveLayerId);
    }

    // ----------------------------- Visibility / opacity / rename -----------------------------

    [Fact]
    public void ToggleLayerVisibility_TogglesIsVisible()
    {
        var service = new LayerService();
        var layerId = service.ActiveLayerId!.Value;

        service.ToggleLayerVisibility(layerId, false);

        Assert.False(service.GetActiveLayer()!.IsVisible);

        service.ToggleLayerVisibility(layerId, true);

        Assert.True(service.GetActiveLayer()!.IsVisible);
    }

    [Theory]
    [InlineData(-0.5, 0)]
    [InlineData(0.5, 0.5)]
    [InlineData(1.5, 1.0)]
    public void SetLayerOpacity_ClampsBetweenZeroAndOne(double input, double expected)
    {
        var service = new LayerService();
        var layerId = service.ActiveLayerId!.Value;

        service.SetLayerOpacity(layerId, input);

        Assert.Equal(expected, service.GetActiveLayer()!.Opacity);
    }

    [Fact]
    public void RenameLayer_EmptyName_DoesNotOverwriteOriginal()
    {
        var service = new LayerService();
        var layerId = service.ActiveLayerId!.Value;
        var originalName = service.GetActiveLayer()!.Name;

        service.RenameLayer(layerId, "   ");

        Assert.Equal(originalName, service.GetActiveLayer()!.Name);
    }

    [Fact]
    public void RenameLayer_TrimsNewName()
    {
        var service = new LayerService();
        var layerId = service.ActiveLayerId!.Value;

        service.RenameLayer(layerId, "  Background  ");

        Assert.Equal("Background", service.GetActiveLayer()!.Name);
    }

    // ----------------------------- Z-order -----------------------------

    [Fact]
    public void MoveLayerUp_OnTopLayer_DoesNotChangeOrder()
    {
        var service = new LayerService();
        var second = service.CreateEmptyLayer("Second");
        service.AddLayer(second);
        var orderBefore = service.Layers.Select(layer => layer.Id).ToList();

        service.MoveLayerUp(second.Id);

        var orderAfter = service.Layers.Select(layer => layer.Id).ToList();
        Assert.Equal(orderBefore, orderAfter);
    }

    [Fact]
    public void MoveLayerDown_OnBottomLayer_DoesNotChangeOrder()
    {
        var service = new LayerService();
        var second = service.CreateEmptyLayer("Second");
        service.AddLayer(second);
        var bottomId = service.Layers.OrderBy(layer => layer.ZIndex).First().Id;
        var orderBefore = service.Layers.Select(layer => layer.Id).ToList();

        service.MoveLayerDown(bottomId);

        var orderAfter = service.Layers.Select(layer => layer.Id).ToList();
        Assert.Equal(orderBefore, orderAfter);
    }

    [Fact]
    public void MoveLayerDown_SwapsZIndexWithLowerLayer()
    {
        var service = new LayerService();
        var second = service.CreateEmptyLayer("Second");
        service.AddLayer(second);
        var topId = second.Id;

        service.MoveLayerDown(topId);

        var ordered = service.Layers.OrderBy(layer => layer.ZIndex).Select(layer => layer.Id).ToList();
        Assert.Equal(topId, ordered[0]);
    }

    // ----------------------------- Filters -----------------------------

    [Theory]
    [InlineData(-10, -10, -1, 0, 0, 0)]
    [InlineData(500, 500, 50, 300, 300, 20)]
    public void SetLayerFilters_ClampsAllValues(int b, int c, int blur, int expectedB, int expectedC, int expectedBlur)
    {
        var service = new LayerService();
        var layerId = service.ActiveLayerId!.Value;

        service.SetLayerFilters(layerId, b, c, blur);

        var filters = service.GetActiveLayer()!.Filters;
        Assert.Equal(expectedB, filters.Brightness);
        Assert.Equal(expectedC, filters.Contrast);
        Assert.Equal(expectedBlur, filters.BlurRadius);
    }

    [Fact]
    public void SetLayerFilters_UnknownLayerId_DoesNothing()
    {
        var service = new LayerService();

        service.SetLayerFilters(Guid.NewGuid(), 50, 50, 5);

        Assert.False(service.CanUndo());
    }

    // ----------------------------- Image data -----------------------------

    [Fact]
    public void SetLayerImage_AssignsDataUrlAndClearsStrokes()
    {
        var service = new LayerService();
        var layerId = service.ActiveLayerId!.Value;
        service.AddStrokeToActiveLayer(new StrokeLine { X1 = 0, Y1 = 0, X2 = 1, Y2 = 1 });

        service.SetLayerImage(layerId, "data:image/png;base64,abc");

        var layer = service.GetActiveLayer()!;
        Assert.Equal("data:image/png;base64,abc", layer.ImageDataUrl);
        Assert.Empty(layer.Strokes);
    }

    [Fact]
    public void AddStrokeToActiveLayer_WhenNoActiveLayerExists_DoesNotThrow()
    {
        var service = new LayerService();

        var exception = Record.Exception(() => service.AddStrokeToActiveLayer(new StrokeLine()));

        Assert.Null(exception);
    }

    // ----------------------------- Project snapshot / load -----------------------------

    [Fact]
    public void CreateProjectSnapshot_CopiesCurrentLayersAndCanvasSize()
    {
        var service = new LayerService();
        service.SetCanvasSize(640, 480);
        service.AddLayer(service.CreateEmptyLayer("Test"));

        var snapshot = service.CreateProjectSnapshot("Demo", new ToolSettings(), "Кисть", new EditorViewState());

        Assert.Equal("Demo", snapshot.Name);
        Assert.Equal(640, snapshot.CanvasWidth);
        Assert.Equal(480, snapshot.CanvasHeight);
        Assert.Equal(service.Layers.Count, snapshot.Layers.Count);
    }

    [Fact]
    public void LoadProject_ReplacesLayersAndClearsHistory()
    {
        var service = new LayerService();
        service.AddLayer(service.CreateEmptyLayer("A"));
        service.AddLayer(service.CreateEmptyLayer("B"));
        Assert.True(service.CanUndo());

        var project = new EditorProject
        {
            CanvasWidth = 320,
            CanvasHeight = 240,
            Layers = new List<CanvasLayer>
            {
                new() { Id = Guid.NewGuid(), Name = "Restored", ZIndex = 0 }
            }
        };
        project.ActiveLayerId = project.Layers[0].Id;

        service.LoadProject(project);

        Assert.Single(service.Layers);
        Assert.Equal("Restored", service.Layers[0].Name);
        Assert.Equal(320, service.CanvasWidth);
        Assert.False(service.CanUndo());
        Assert.False(service.CanRedo());
    }

    [Fact]
    public void LoadProject_WithEmptyLayerList_RecreatesBaseLayer()
    {
        var service = new LayerService();

        service.LoadProject(new EditorProject
        {
            Layers = new List<CanvasLayer>()
        });

        Assert.Single(service.Layers);
        Assert.NotNull(service.ActiveLayerId);
    }

    // ----------------------------- Undo / redo -----------------------------

    [Fact]
    public void Undo_WhenHistoryEmpty_DoesNothing()
    {
        var service = new LayerService();

        var exception = Record.Exception(() => service.Undo());

        Assert.Null(exception);
        Assert.False(service.CanUndo());
    }

    [Fact]
    public void Redo_WhenStackEmpty_DoesNothing()
    {
        var service = new LayerService();

        var exception = Record.Exception(() => service.Redo());

        Assert.Null(exception);
        Assert.False(service.CanRedo());
    }

    [Fact]
    public void NewAction_AfterUndo_ClearsRedoStack()
    {
        var service = new LayerService();
        service.AddLayer(service.CreateEmptyLayer("A"));
        service.Undo();
        Assert.True(service.CanRedo());

        service.AddLayer(service.CreateEmptyLayer("B"));

        Assert.False(service.CanRedo());
    }

    [Fact]
    public void ClearHistory_RemovesUndoAndRedoEntries()
    {
        var service = new LayerService();
        service.AddLayer(service.CreateEmptyLayer("A"));
        service.Undo();

        service.ClearHistory();

        Assert.False(service.CanUndo());
        Assert.False(service.CanRedo());
    }
}

using ImageEditorWeb.Core.Services;
using ImageEditorWeb.Shared.Models;

namespace ImageEditorWeb.Tests;

public class UnitTest1
{
    [Fact]
    public void Constructor_CreatesDefaultLayerAndSetsItActive()
    {
        var service = new LayerService();

        Assert.Single(service.Layers);
        Assert.Equal(service.Layers[0].Id, service.ActiveLayerId);
    }

    [Fact]
    public void AddLayer_SetsNewLayerAsActive()
    {
        var service = new LayerService();
        var layer = service.CreateEmptyLayer("Тестовый слой");

        service.AddLayer(layer);

        Assert.Equal(2, service.Layers.Count);
        Assert.Equal(layer.Id, service.ActiveLayerId);
    }

    [Fact]
    public void RemoveLayer_LastLayerIsRecreatedAutomatically()
    {
        var service = new LayerService();
        var layerId = service.ActiveLayerId!.Value;

        service.RemoveLayer(layerId);

        Assert.Single(service.Layers);
        Assert.NotNull(service.ActiveLayerId);
    }

    [Fact]
    public void Undo_RevertsLastAddedLayer()
    {
        var service = new LayerService();
        service.AddLayer(service.CreateEmptyLayer("Второй слой"));

        service.Undo();

        Assert.Single(service.Layers);
    }

    [Fact]
    public void Redo_RestoresLayerAfterUndo()
    {
        var service = new LayerService();
        service.AddLayer(service.CreateEmptyLayer("Второй слой"));
        service.Undo();

        service.Redo();

        Assert.Equal(2, service.Layers.Count);
    }

    [Fact]
    public void SaveCheckpointAndStroke_AddsStrokeToActiveLayer()
    {
        var service = new LayerService();
        service.SaveCheckpoint();
        service.AddStrokeToActiveLayer(new StrokeLine
        {
            X1 = 0,
            Y1 = 0,
            X2 = 10,
            Y2 = 10,
            Size = 5,
            ColorHex = "#000000"
        });

        Assert.Single(service.GetActiveLayer()!.Strokes);

        service.Undo();

        Assert.Empty(service.GetActiveLayer()!.Strokes);
    }
}

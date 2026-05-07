using ImageEditorWeb.Core.Services;
using ImageEditorWeb.Shared.Models;
using ImageEditorWeb.Shared.ToolType;

namespace ImageEditorWeb.Core.Tools;

public readonly record struct EditorPoint(int X, int Y);

public abstract class BaseTool
{
    public abstract string Name { get; }
    public abstract string Icon { get; }
    public abstract ToolType Type { get; }

    public virtual Task OnMouseDown(EditorPoint position, ToolSettings settings, LayerService layerService)
        => Task.CompletedTask;

    public virtual Task OnMouseMove(EditorPoint position, ToolSettings settings, LayerService layerService)
        => Task.CompletedTask;

    public virtual Task OnMouseUp(EditorPoint position, ToolSettings settings, LayerService layerService)
        => Task.CompletedTask;
}

public class BrushTool : BaseTool
{
    private EditorPoint _lastPoint;
    private bool _isDrawing;

    public override string Name => "Кисть";
    public override string Icon => "🖌️";
    public override ToolType Type => ToolType.Brush;

    public override Task OnMouseDown(EditorPoint position, ToolSettings settings, LayerService layerService)
    {
        _isDrawing = true;
        _lastPoint = position;
        layerService.SaveCheckpoint();
        layerService.AddStrokeToActiveLayer(CreateStroke(position, position, settings, false));
        return Task.CompletedTask;
    }

    public override Task OnMouseMove(EditorPoint position, ToolSettings settings, LayerService layerService)
    {
        if (!_isDrawing)
        {
            return Task.CompletedTask;
        }

        layerService.AddStrokeToActiveLayer(CreateStroke(_lastPoint, position, settings, false));
        _lastPoint = position;
        return Task.CompletedTask;
    }

    public override Task OnMouseUp(EditorPoint position, ToolSettings settings, LayerService layerService)
    {
        if (_isDrawing && position != _lastPoint)
        {
            layerService.AddStrokeToActiveLayer(CreateStroke(_lastPoint, position, settings, false));
        }

        _isDrawing = false;
        return Task.CompletedTask;
    }

    private static StrokeLine CreateStroke(EditorPoint start, EditorPoint end, ToolSettings settings, bool isEraser)
    {
        return new StrokeLine
        {
            X1 = start.X,
            Y1 = start.Y,
            X2 = end.X,
            Y2 = end.Y,
            Size = settings.BrushSize,
            ColorHex = settings.ForegroundColorHex,
            IsEraser = isEraser
        };
    }
}

public class EraserTool : BaseTool
{
    private EditorPoint _lastPoint;
    private bool _isDrawing;

    public override string Name => "Ластик";
    public override string Icon => "🧽";
    public override ToolType Type => ToolType.Eraser;

    public override Task OnMouseDown(EditorPoint position, ToolSettings settings, LayerService layerService)
    {
        _isDrawing = true;
        _lastPoint = position;
        layerService.SaveCheckpoint();
        layerService.AddStrokeToActiveLayer(CreateStroke(position, position, settings));
        return Task.CompletedTask;
    }

    public override Task OnMouseMove(EditorPoint position, ToolSettings settings, LayerService layerService)
    {
        if (!_isDrawing)
        {
            return Task.CompletedTask;
        }

        layerService.AddStrokeToActiveLayer(CreateStroke(_lastPoint, position, settings));
        _lastPoint = position;
        return Task.CompletedTask;
    }

    public override Task OnMouseUp(EditorPoint position, ToolSettings settings, LayerService layerService)
    {
        if (_isDrawing && position != _lastPoint)
        {
            layerService.AddStrokeToActiveLayer(CreateStroke(_lastPoint, position, settings));
        }

        _isDrawing = false;
        return Task.CompletedTask;
    }

    private static StrokeLine CreateStroke(EditorPoint start, EditorPoint end, ToolSettings settings)
    {
        return new StrokeLine
        {
            X1 = start.X,
            Y1 = start.Y,
            X2 = end.X,
            Y2 = end.Y,
            Size = settings.BrushSize,
            ColorHex = settings.BackgroundColorHex,
            IsEraser = true
        };
    }
}

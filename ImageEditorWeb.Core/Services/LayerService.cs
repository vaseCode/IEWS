using ImageEditorWeb.Shared.Models;

namespace ImageEditorWeb.Core.Services;

public class LayerService
{
    private List<CanvasLayer> _layers = new();
    private readonly Stack<EditorStateSnapshot> _history = new();
    private readonly Stack<EditorStateSnapshot> _redoStack = new();

    public Guid? ActiveLayerId { get; private set; }
    public int CanvasWidth { get; private set; } = 800;
    public int CanvasHeight { get; private set; } = 600;
    public IReadOnlyList<CanvasLayer> Layers => _layers.OrderBy(layer => layer.ZIndex).ToList().AsReadOnly();

    public LayerService()
    {
        EnsureBaseLayer();
    }

    public CanvasLayer CreateEmptyLayer(string? name = null)
    {
        return new CanvasLayer
        {
            Id = Guid.NewGuid(),
            Name = string.IsNullOrWhiteSpace(name) ? $"Слой {_layers.Count + 1}" : name,
            ZIndex = _layers.Count == 0 ? 0 : _layers.Max(layer => layer.ZIndex) + 1
        };
    }

    public CanvasLayer? GetActiveLayer()
    {
        return _layers.FirstOrDefault(layer => layer.Id == ActiveLayerId);
    }

    public void SetCanvasSize(int width, int height)
    {
        width = Math.Clamp(width, 1, 4096);
        height = Math.Clamp(height, 1, 4096);

        if (CanvasWidth == width && CanvasHeight == height)
        {
            return;
        }

        SaveState();
        CanvasWidth = width;
        CanvasHeight = height;
    }

    public void AddLayer(CanvasLayer layer, bool setActive = true)
    {
        SaveState();

        layer.ZIndex = _layers.Count == 0 ? 0 : _layers.Max(existing => existing.ZIndex) + 1;
        _layers.Add(layer.Clone());
        NormalizeZIndexes();

        if (setActive)
        {
            ActiveLayerId = layer.Id;
        }
    }

    public void RemoveLayer(Guid layerId)
    {
        if (_layers.All(layer => layer.Id != layerId))
        {
            return;
        }

        SaveState();
        _layers.RemoveAll(layer => layer.Id == layerId);
        NormalizeZIndexes();

        if (ActiveLayerId == layerId)
        {
            ActiveLayerId = _layers.OrderByDescending(layer => layer.ZIndex).FirstOrDefault()?.Id;
        }

        EnsureBaseLayer();
    }

    public void SetActiveLayer(Guid layerId)
    {
        if (_layers.Any(layer => layer.Id == layerId))
        {
            ActiveLayerId = layerId;
        }
    }

    public void RenameLayer(Guid layerId, string name)
    {
        var layer = _layers.FirstOrDefault(item => item.Id == layerId);
        if (layer == null)
        {
            return;
        }

        SaveState();
        layer.Name = string.IsNullOrWhiteSpace(name) ? layer.Name : name.Trim();
    }

    public void ToggleLayerVisibility(Guid layerId, bool isVisible)
    {
        var layer = _layers.FirstOrDefault(item => item.Id == layerId);
        if (layer == null)
        {
            return;
        }

        SaveState();
        layer.IsVisible = isVisible;
    }

    public void SetLayerOpacity(Guid layerId, double opacity)
    {
        var layer = _layers.FirstOrDefault(item => item.Id == layerId);
        if (layer == null)
        {
            return;
        }

        SaveState();
        layer.Opacity = Math.Clamp(opacity, 0d, 1d);
    }

    public void SetLayerFilters(Guid layerId, int brightness, int contrast, int blurRadius)
    {
        var layer = _layers.FirstOrDefault(item => item.Id == layerId);
        if (layer == null)
        {
            return;
        }

        SaveState();
        layer.Filters.Brightness = Math.Clamp(brightness, 0, 300);
        layer.Filters.Contrast = Math.Clamp(contrast, 0, 300);
        layer.Filters.BlurRadius = Math.Clamp(blurRadius, 0, 20);
    }

    public void ResetLayerFilters(Guid layerId)
    {
        var layer = _layers.FirstOrDefault(item => item.Id == layerId);
        if (layer == null)
        {
            return;
        }

        SaveState();
        layer.Filters = new LayerFilterSettings();
    }

    public void MoveLayerUp(Guid layerId)
    {
        var ordered = _layers.OrderBy(layer => layer.ZIndex).ToList();
        var index = ordered.FindIndex(layer => layer.Id == layerId);
        if (index < 0 || index >= ordered.Count - 1)
        {
            return;
        }

        SaveState();
        (ordered[index].ZIndex, ordered[index + 1].ZIndex) = (ordered[index + 1].ZIndex, ordered[index].ZIndex);
        NormalizeZIndexes();
    }

    public void MoveLayerDown(Guid layerId)
    {
        var ordered = _layers.OrderBy(layer => layer.ZIndex).ToList();
        var index = ordered.FindIndex(layer => layer.Id == layerId);
        if (index <= 0)
        {
            return;
        }

        SaveState();
        (ordered[index].ZIndex, ordered[index - 1].ZIndex) = (ordered[index - 1].ZIndex, ordered[index].ZIndex);
        NormalizeZIndexes();
    }

    public void SetLayerImage(Guid layerId, string dataUrl)
    {
        var layer = _layers.FirstOrDefault(item => item.Id == layerId);
        if (layer == null)
        {
            return;
        }

        SaveState();
        layer.ImageDataUrl = dataUrl;
        layer.Strokes.Clear();
    }

    public void AddStrokeToActiveLayer(StrokeLine stroke)
    {
        var activeLayer = GetActiveLayer();
        if (activeLayer == null)
        {
            return;
        }

        activeLayer.Strokes.Add(stroke);
    }

    public void SaveCheckpoint()
    {
        SaveState();
    }

    public EditorProject CreateProjectSnapshot(string projectName, ToolSettings toolSettings, string currentTool, EditorViewState viewState, Guid? projectId = null, DateTime? createdAtUtc = null)
    {
        return new EditorProject
        {
            Id = projectId ?? Guid.NewGuid(),
            Name = projectName,
            CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            Layers = _layers.Select(layer => layer.Clone()).ToList(),
            ActiveLayerId = ActiveLayerId,
            CanvasWidth = CanvasWidth,
            CanvasHeight = CanvasHeight,
            CurrentTool = currentTool,
            ToolSettings = toolSettings.Clone(),
            ViewState = viewState.Clone()
        };
    }

    public void LoadProject(EditorProject project)
    {
        _layers = project.Layers.Select(layer => layer.Clone()).ToList();
        ActiveLayerId = project.ActiveLayerId;
        CanvasWidth = Math.Clamp(project.CanvasWidth, 1, 4096);
        CanvasHeight = Math.Clamp(project.CanvasHeight, 1, 4096);
        EnsureBaseLayer();
        NormalizeZIndexes();
        ClearHistory();
    }

    public void ClearHistory()
    {
        _history.Clear();
        _redoStack.Clear();
    }

    public bool CanUndo() => _history.Count > 0;
    public bool CanRedo() => _redoStack.Count > 0;

    public void Undo()
    {
        if (_history.Count == 0)
        {
            return;
        }

        _redoStack.Push(CreateSnapshot());
        RestoreSnapshot(_history.Pop());
    }

    public void Redo()
    {
        if (_redoStack.Count == 0)
        {
            return;
        }

        _history.Push(CreateSnapshot());
        RestoreSnapshot(_redoStack.Pop());
    }

    private void EnsureBaseLayer()
    {
        if (_layers.Count > 0)
        {
            return;
        }

        var layer = CreateEmptyLayer("Слой 1");
        layer.ZIndex = 0;
        _layers.Add(layer);
        ActiveLayerId = layer.Id;
    }

    private void NormalizeZIndexes()
    {
        _layers = _layers.OrderBy(layer => layer.ZIndex).ToList();
        for (var i = 0; i < _layers.Count; i++)
        {
            _layers[i].ZIndex = i;
        }
    }

    private void SaveState()
    {
        _history.Push(CreateSnapshot());
        _redoStack.Clear();
    }

    private EditorStateSnapshot CreateSnapshot()
    {
        return new EditorStateSnapshot
        {
            ActiveLayerId = ActiveLayerId,
            CanvasWidth = CanvasWidth,
            CanvasHeight = CanvasHeight,
            Layers = _layers.Select(layer => layer.Clone()).ToList()
        };
    }

    private void RestoreSnapshot(EditorStateSnapshot snapshot)
    {
        _layers = snapshot.Layers.Select(layer => layer.Clone()).ToList();
        ActiveLayerId = snapshot.ActiveLayerId;
        CanvasWidth = snapshot.CanvasWidth;
        CanvasHeight = snapshot.CanvasHeight;
        EnsureBaseLayer();
        NormalizeZIndexes();
    }

    private sealed class EditorStateSnapshot
    {
        public List<CanvasLayer> Layers { get; init; } = new();
        public Guid? ActiveLayerId { get; init; }
        public int CanvasWidth { get; init; }
        public int CanvasHeight { get; init; }
    }
}

namespace ImageEditorWeb.Shared.Models;

public class EditorProject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Без названия";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<CanvasLayer> Layers { get; set; } = new();
    public Guid? ActiveLayerId { get; set; }
    public int CanvasWidth { get; set; } = 800;
    public int CanvasHeight { get; set; } = 600;
    public string CurrentTool { get; set; } = "Кисть";
    public ToolSettings ToolSettings { get; set; } = new();
    public EditorViewState ViewState { get; set; } = new();

    public EditorProject Clone()
    {
        return new EditorProject
        {
            Id = Id,
            Name = Name,
            CreatedAtUtc = CreatedAtUtc,
            UpdatedAtUtc = UpdatedAtUtc,
            Layers = Layers.Select(layer => layer.Clone()).ToList(),
            ActiveLayerId = ActiveLayerId,
            CanvasWidth = CanvasWidth,
            CanvasHeight = CanvasHeight,
            CurrentTool = CurrentTool,
            ToolSettings = ToolSettings.Clone(),
            ViewState = ViewState.Clone()
        };
    }
}

public class EditorViewState
{
    public double Zoom { get; set; } = 1.0;
    public double PanX { get; set; }
    public double PanY { get; set; }

    public EditorViewState Clone()
    {
        return new EditorViewState
        {
            Zoom = Zoom,
            PanX = PanX,
            PanY = PanY
        };
    }
}

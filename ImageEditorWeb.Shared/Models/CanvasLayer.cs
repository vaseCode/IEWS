namespace ImageEditorWeb.Shared.Models;

public class CanvasLayer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsVisible { get; set; } = true;
    public double Opacity { get; set; } = 1.0;
    public int ZIndex { get; set; }
    public string? ImageDataUrl { get; set; }
    public LayerFilterSettings Filters { get; set; } = new();
    public List<StrokeLine> Strokes { get; set; } = new();

    public bool HasContent => !string.IsNullOrWhiteSpace(ImageDataUrl) || Strokes.Count > 0;

    public CanvasLayer Clone()
    {
        return new CanvasLayer
        {
            Id = Id,
            Name = Name,
            IsVisible = IsVisible,
            Opacity = Opacity,
            ZIndex = ZIndex,
            ImageDataUrl = ImageDataUrl,
            Filters = Filters.Clone(),
            Strokes = Strokes.Select(stroke => stroke.Clone()).ToList()
        };
    }
}

public class LayerFilterSettings
{
    public int Brightness { get; set; } = 100;
    public int Contrast { get; set; } = 100;
    public int BlurRadius { get; set; }

    public LayerFilterSettings Clone()
    {
        return new LayerFilterSettings
        {
            Brightness = Brightness,
            Contrast = Contrast,
            BlurRadius = BlurRadius
        };
    }
}

public class StrokeLine
{
    public int X1 { get; set; }
    public int Y1 { get; set; }
    public int X2 { get; set; }
    public int Y2 { get; set; }
    public int Size { get; set; } = 5;
    public string ColorHex { get; set; } = "#000000";
    public bool IsEraser { get; set; }

    public StrokeLine Clone()
    {
        return new StrokeLine
        {
            X1 = X1,
            Y1 = Y1,
            X2 = X2,
            Y2 = Y2,
            Size = Size,
            ColorHex = ColorHex,
            IsEraser = IsEraser
        };
    }
}

public class EditorState
{
    public List<CanvasLayer> Layers { get; set; } = new();
    public Guid? ActiveLayerId { get; set; }
    public int CanvasWidth { get; set; } = 800;
    public int CanvasHeight { get; set; } = 600;
    public string CurrentTool { get; set; } = "Кисть";
}

public class ToolSettings
{
    public string ForegroundColorHex { get; set; } = "#000000";
    public string BackgroundColorHex { get; set; } = "#FFFFFF";
    public int BrushSize { get; set; } = 5;
    public string BrushType { get; set; } = "Round";

    public ToolSettings Clone()
    {
        return new ToolSettings
        {
            ForegroundColorHex = ForegroundColorHex,
            BackgroundColorHex = BackgroundColorHex,
            BrushSize = BrushSize,
            BrushType = BrushType
        };
    }
}

public class ImportedImageData
{
    public string DataUrl { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "image/png";
}

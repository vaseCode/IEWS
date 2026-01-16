using System;
/*using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;*/
using System.Drawing;

namespace ImageEditorWeb.Shared.Models
{
    public class CanvasLayer
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsVisible { get; set; } = true;
        public float Opacity { get; set; } = 1.0f;
        public int ZIndex { get; set; }
        public byte[] ImageData { get; set; }
    }

    public class EditorState
    {
        public List<CanvasLayer> Layers { get; set; } = new();
        public int ActiveLayerIndex { get; set; }
        public Size CanvasSize { get; set; }
        public string CurrentTool { get; set; }
    }

    public class ToolSettings
    {
        public Color ForegroundColor { get; set; } = Color.White;
        public Color BackgroundColor { get; set; } = Color.Black;
        public int BrushSize { get; set; }
        public string BrushType { get; set; } = "Round";
    }
}

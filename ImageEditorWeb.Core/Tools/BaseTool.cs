using ImageEditorWeb.Shared.Models;
using ImageEditorWeb.Shared.ToolType;
using System.Drawing;
using System.Threading.Tasks;

namespace ImageEditorWeb.Core.Tools
{
    public abstract class BaseTool
    {
        public abstract string Name { get; }
        public abstract string Icon { get; }
        public abstract ToolType Type { get; }

        public virtual Task OnMouseDown(Point position, ToolSettings settings)
            => Task.CompletedTask;

        public virtual Task OnMouseMove(Point position, ToolSettings settings)
            => Task.CompletedTask;

        public virtual Task OnMouseUp(Point position, ToolSettings settings)
            => Task.CompletedTask;
    }

    public class BrushTool : BaseTool
    {
        public override string Name => "Кисть";
        public override string Icon => "fas fa-paint-brush";
        public override ToolType Type => ToolType.Brush;

        private Point _lastPoint;
        private bool _isDrawing = false;

        public override Task OnMouseDown(Point position, ToolSettings settings)
        {
            _isDrawing = true;
            _lastPoint = position;
            return Task.CompletedTask;
        }

        public override Task OnMouseMove(Point position, ToolSettings settings)
        {
            if (!_isDrawing) return Task.CompletedTask;

            // Логика рисования линии
            _lastPoint = position;
            return Task.CompletedTask;
        }

        public override Task OnMouseUp(Point position, ToolSettings settings)
        {
            _isDrawing = false;
            return Task.CompletedTask;
        }
    }
}

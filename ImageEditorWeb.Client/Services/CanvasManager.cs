using Blazor.Extensions;
using Blazor.Extensions.Canvas;
using Blazor.Extensions.Canvas.Canvas2D;
using ImageEditorWeb.Shared.Models;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageEditorWeb.Client.Services
{
    public class CanvasManager
    {
        private readonly IJSRuntime _jsRuntime;
        private Canvas2DContext _context;
        private BECanvasComponent _canvas;

        public CanvasManager(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeAsync(BECanvasComponent canvas)
        {
            _canvas = canvas;
            _context = await _canvas.CreateCanvas2DAsync();

            // Инициализация
            await _context.SetFillStyleAsync("white");
            await _context.FillRectAsync(0, 0, 800, 600);
        }

        public async Task DrawLayer(CanvasLayer layer)
        {
            if (!layer.IsVisible || layer.ImageData == null)
                return;

            await _context.SaveAsync();
            await _context.SetGlobalAlphaAsync(layer.Opacity);

            // Здесь будет логика отрисовки изображения
            // Временная реализация
            await _context.RestoreAsync();
        }

        public async Task DrawAllLayers(IEnumerable<CanvasLayer> layers)
        {
            await ClearCanvas();
            foreach (var layer in layers.OrderBy(l => l.ZIndex))
            {
                await DrawLayer(layer);
            }
        }

        private async Task ClearCanvas()
        {
            await _context.SetFillStyleAsync("white");
            await _context.FillRectAsync(0, 0, 800, 600);
        }

        // Метод для отрисовки точки
        public async Task DrawPoint(int x, int y, int size, string color)
        {
            await _context.BeginPathAsync();
            await _context.ArcAsync(x, y, size / 2, 0, Math.PI * 2);
            await _context.SetFillStyleAsync(color);
            await _context.FillAsync();
        }

        // Метод для отрисовки линии
        public async Task DrawLine(int x1, int y1, int x2, int y2, int size, string color)
        {
            await _context.BeginPathAsync();
            await _context.MoveToAsync(x1, y1);
            await _context.LineToAsync(x2, y2);
            await _context.SetStrokeStyleAsync(color);
            await _context.SetLineWidthAsync(size);
            await _context.SetLineCapAsync(LineCap.Round);
            await _context.StrokeAsync();
        }
    }
}
/*using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using ImageEditorWeb.Shared.Models;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace ImageEditorWeb.Client.Services
{
    public class CanvasManager : IAsyncDisposable
    {
        private readonly IJSRuntime _jsRuntime;
        private Canvas2DContext _context;
        private BECanvasComponent _canvas;

        public CanvasManager(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeAsync(string canvasId, BECanvasComponent canvas)
        {
            _canvas = canvas;
            _context = await _canvas.CreateCanvas2DAsync();

            // Инициализация
            await _context.SetFillStyleAsync("white");
            await _context.FillRectAsync(0, 0, 800, 600);
        }

        public async Task DrawLayer(CanvasLayer layer)
        {
            if (!layer.IsVisible || layer.ImageData == null)
                return;

            await _context.SaveAsync();
            await _context.SetGlobalAlphaAsync(layer.Opacity);

            // Здесь будет логика отрисовки изображения
            // Временная реализация
            await _context.RestoreAsync();
        }

        public async Task DrawAllLayers(IEnumerable<CanvasLayer> layers)
        {
            await ClearCanvas();
            foreach (var layer in layers.OrderBy(l => l.ZIndex))
            {
                await DrawLayer(layer);
            }
        }

        private async Task ClearCanvas()
        {
            await _context.SetFillStyleAsync("white");
            await _context.FillRectAsync(0, 0, 800, 600);
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}*/
/*using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using ImageEditorWeb.Shared.Models;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace ImageEditorWeb.Client.Services
{
    public class CanvasManager : IAsyncDisposable
    {
        private readonly IJSRuntime _jsRuntime;
        private Canvas2DContext _context;
        private BECanvasComponent _canvas;
        public CanvasManager(IJSRuntime jSRuntime)
        {
            _jsRuntime =jSRuntime;
        }
        public async Task InitializeAsync(string canvasId, BECanvasComponent canvas)
        {
            _canvas = canvas;
            _context = await _canvas.CreateCanvas2DAsync();

            await _context.SetFillStyleAsync("white");
            await _context.FillRectAsync(0, 0, 800, 600);
        }
        public async Task DrawLayer(CanvasLayer layer)
        {
            if(!layer.IsVisible||layer.ImageData == null) 
            { 
                return; 
            }  
            await _context.SaveAsync();
            await _context.SetGlobalAlphaAsync(layer.Opacity);



            await _context.RestoreAsync();
        }
        public async Task DrawAllLayers(IEnumerable<CanvasLayer> layers)
        {
            await ClearCanvas();
            foreach (var layer in layers.OrderBy(I => I.ZIndex)) 
            {
                await DrawLayer(layer);
            }
        }
        private async Task ClearCanvas()
        {
            await _context.SetFillStyleAsync("white");
            await _context.FillRectAsync(0, 0, 800, 600);
        }
        public async ValueTask DisposeAsync()
        {
            if (_context != null)
                await _context.DisposeAsync();            
        }
    }
}*/

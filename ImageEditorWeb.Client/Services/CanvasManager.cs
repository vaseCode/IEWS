using Blazor.Extensions;
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
}

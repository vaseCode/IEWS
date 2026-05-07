using ImageEditorWeb.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ImageEditorWeb.Client.Services;

public class CanvasManager
{
    private readonly IJSRuntime _jsRuntime;
    private ElementReference _canvas;
    private bool _initialized;
    private int _width = 800;
    private int _height = 600;

    public CanvasManager(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync(ElementReference canvas, int width = 800, int height = 600)
    {
        _canvas = canvas;
        _width = width;
        _height = height;
        _initialized = true;

        await _jsRuntime.InvokeVoidAsync("iewCanvas.init", _canvas, _width, _height, "#ffffff");
    }

    public async Task DrawAllLayersAsync(IEnumerable<CanvasLayer> layers)
    {
        if (!_initialized)
        {
            return;
        }

        await ClearCanvasAsync();

        foreach (var layer in layers.OrderBy(layer => layer.ZIndex))
        {
            if (!layer.IsVisible)
            {
                continue;
            }

            var payload = new
            {
                width = _width,
                height = _height,
                opacity = layer.Opacity,
                imageDataUrl = layer.ImageDataUrl,
                strokes = layer.Strokes.Select(stroke => new
                {
                    x1 = stroke.X1,
                    y1 = stroke.Y1,
                    x2 = stroke.X2,
                    y2 = stroke.Y2,
                    size = stroke.Size,
                    colorHex = stroke.ColorHex,
                    isEraser = stroke.IsEraser
                }).ToList()
            };

            await _jsRuntime.InvokeVoidAsync("iewCanvas.drawLayer", _canvas, payload);
        }
    }

    public async Task ClearCanvasAsync()
    {
        if (!_initialized)
        {
            return;
        }

        await _jsRuntime.InvokeVoidAsync("iewCanvas.clear", _canvas, _width, _height, "#ffffff");
    }

    public async Task DownloadPngAsync(string fileName)
    {
        if (!_initialized)
        {
            return;
        }

        var dataUrl = await _jsRuntime.InvokeAsync<string>("iewCanvas.getDataUrl", _canvas, "image/png", 1.0);
        await _jsRuntime.InvokeVoidAsync("iewCanvas.download", dataUrl, fileName);
    }
}

using ImageEditorWeb.Shared.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace ImageEditorWeb.Client.Services;

public class ImageImportService
{
    private readonly IJSRuntime _jsRuntime;

    public ImageImportService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<ImportedImageData> ReadImageAsync(IBrowserFile file, long maxBytes = 15 * 1024 * 1024)
    {
        await using var inputStream = file.OpenReadStream(maxBytes);
        using var memoryStream = new MemoryStream();
        await inputStream.CopyToAsync(memoryStream);

        var contentType = string.IsNullOrWhiteSpace(file.ContentType)
            ? "image/png"
            : file.ContentType;

        var base64 = Convert.ToBase64String(memoryStream.ToArray());
        var dataUrl = $"data:{contentType};base64,{base64}";
        var size = await _jsRuntime.InvokeAsync<ImageSizeDto>("iewCanvas.getImageSize", dataUrl);

        return new ImportedImageData
        {
            DataUrl = dataUrl,
            Width = size.Width,
            Height = size.Height,
            FileName = file.Name,
            ContentType = contentType
        };
    }

    private sealed class ImageSizeDto
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
}

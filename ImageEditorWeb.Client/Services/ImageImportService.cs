using Microsoft.AspNetCore.Components.Forms;

namespace ImageEditorWeb.Client.Services;

public class ImageImportService
{
    public async Task<string> ReadAsDataUrlAsync(IBrowserFile file, long maxBytes = 15 * 1024 * 1024)
    {
        await using var inputStream = file.OpenReadStream(maxBytes);
        using var memoryStream = new MemoryStream();
        await inputStream.CopyToAsync(memoryStream);

        var contentType = string.IsNullOrWhiteSpace(file.ContentType)
            ? "image/png"
            : file.ContentType;

        var base64 = Convert.ToBase64String(memoryStream.ToArray());
        return $"data:{contentType};base64,{base64}";
    }
}

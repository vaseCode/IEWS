using System.Text.Json;
using ImageEditorWeb.Shared.Models;
using Microsoft.JSInterop;

namespace ImageEditorWeb.Client.Services;

public class SettingsService
{
    private const string StorageKey = "iew-settings-v1";
    private readonly IJSRuntime _jsRuntime;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public SettingsService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<EditorSettings> LoadAsync()
    {
        var json = await _jsRuntime.InvokeAsync<string?>("iewApp.storageGet", StorageKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new EditorSettings();
        }

        return JsonSerializer.Deserialize<EditorSettings>(json, _jsonOptions) ?? new EditorSettings();
    }

    public async Task SaveAsync(EditorSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, _jsonOptions);
        await _jsRuntime.InvokeVoidAsync("iewApp.storageSet", StorageKey, json);
        await ApplyAsync(settings);
    }

    public async Task ApplyAsync(EditorSettings settings)
    {
        await _jsRuntime.InvokeVoidAsync("iewApp.setDocumentLanguage", settings.Language);
    }
}

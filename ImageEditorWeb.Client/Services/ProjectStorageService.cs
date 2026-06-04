using System.Text.Json;
using ImageEditorWeb.Shared.Models;
using Microsoft.JSInterop;

namespace ImageEditorWeb.Client.Services;

public class ProjectStorageService
{
    private const string StorageKey = "iew-projects-v1";
    private readonly IJSRuntime _jsRuntime;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public ProjectStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<List<EditorProject>> GetProjectsAsync()
    {
        var json = await _jsRuntime.InvokeAsync<string?>("iewApp.storageGet", StorageKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<EditorProject>();
        }

        var projects = JsonSerializer.Deserialize<List<EditorProject>>(json, _jsonOptions) ?? new List<EditorProject>();
        return projects.OrderByDescending(project => project.UpdatedAtUtc).ToList();
    }

    public async Task<EditorProject?> GetProjectAsync(Guid projectId)
    {
        var projects = await GetProjectsAsync();
        return projects.FirstOrDefault(project => project.Id == projectId)?.Clone();
    }

    public async Task SaveProjectAsync(EditorProject project)
    {
        var projects = await GetProjectsAsync();
        var existingIndex = projects.FindIndex(item => item.Id == project.Id);
        project.UpdatedAtUtc = DateTime.UtcNow;

        if (existingIndex >= 0)
        {
            projects[existingIndex] = project.Clone();
        }
        else
        {
            if (project.CreatedAtUtc == default)
            {
                project.CreatedAtUtc = DateTime.UtcNow;
            }

            projects.Add(project.Clone());
        }

        var json = JsonSerializer.Serialize(projects, _jsonOptions);
        await _jsRuntime.InvokeVoidAsync("iewApp.storageSet", StorageKey, json);
    }

    public async Task DeleteProjectAsync(Guid projectId)
    {
        var projects = await GetProjectsAsync();
        projects.RemoveAll(project => project.Id == projectId);
        var json = JsonSerializer.Serialize(projects, _jsonOptions);
        await _jsRuntime.InvokeVoidAsync("iewApp.storageSet", StorageKey, json);
    }
}

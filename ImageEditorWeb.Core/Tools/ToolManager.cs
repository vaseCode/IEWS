namespace ImageEditorWeb.Core.Tools;

public class ToolManager
{
    private readonly Dictionary<string, BaseTool> _tools = new();
    private BaseTool? _currentTool;

    public ToolManager()
    {
        RegisterTool(new BrushTool());
        RegisterTool(new EraserTool());
        _currentTool = _tools.Values.FirstOrDefault();
    }

    public void RegisterTool(BaseTool tool)
    {
        _tools[tool.Name] = tool;
    }

    public void SetCurrentTool(string toolName)
    {
        if (_tools.TryGetValue(toolName, out var tool))
        {
            _currentTool = tool;
        }
    }

    public BaseTool? GetCurrentTool() => _currentTool;

    public IEnumerable<BaseTool> GetAllTools() => _tools.Values;
}

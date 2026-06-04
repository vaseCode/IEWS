namespace ImageEditorWeb.Shared.Models;

public class EditorSettings
{
    public bool AutoSaveProjects { get; set; } = true;
    public bool ShowCheckerboardBackground { get; set; } = true;
    public string Language { get; set; } = "ru-RU";
    public double ZoomStep { get; set; } = 0.1;
    public string DefaultExportFormat { get; set; } = "png";
    public HotkeySettings Hotkeys { get; set; } = new();

    public EditorSettings Clone()
    {
        return new EditorSettings
        {
            AutoSaveProjects = AutoSaveProjects,
            ShowCheckerboardBackground = ShowCheckerboardBackground,
            Language = Language,
            ZoomStep = ZoomStep,
            DefaultExportFormat = DefaultExportFormat,
            Hotkeys = Hotkeys.Clone()
        };
    }
}

public class HotkeySettings
{
    public string OpenProjects { get; set; } = "Ctrl+O";
    public string Save { get; set; } = "Ctrl+S";
    public string SaveAs { get; set; } = "Ctrl+Shift+S";
    public string SaveCopy { get; set; } = "Ctrl+Alt+S";
    public string Undo { get; set; } = "Ctrl+Z";
    public string Redo { get; set; } = "Ctrl+Y";
    public string Settings { get; set; } = "Ctrl+,";
    public string ZoomIn { get; set; } = "Ctrl+=";
    public string ZoomOut { get; set; } = "Ctrl+-";
    public string PanLeft { get; set; } = "ArrowLeft";
    public string PanRight { get; set; } = "ArrowRight";
    public string PanUp { get; set; } = "ArrowUp";
    public string PanDown { get; set; } = "ArrowDown";

    public HotkeySettings Clone()
    {
        return new HotkeySettings
        {
            OpenProjects = OpenProjects,
            Save = Save,
            SaveAs = SaveAs,
            SaveCopy = SaveCopy,
            Undo = Undo,
            Redo = Redo,
            Settings = Settings,
            ZoomIn = ZoomIn,
            ZoomOut = ZoomOut,
            PanLeft = PanLeft,
            PanRight = PanRight,
            PanUp = PanUp,
            PanDown = PanDown
        };
    }

    public static HotkeySettings CreateDefault() => new();
}

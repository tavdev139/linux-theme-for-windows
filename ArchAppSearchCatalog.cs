using System;
using System.Collections.Generic;
using System.Linq;

public sealed class AppSearchEntry {
    public string Name { get; private set; }
    public string Target { get; private set; }
    public string Arguments { get; private set; }
    public string AccentColor { get; private set; }
    public bool IsPinned { get; private set; }

    public AppSearchEntry(string name, string target, string arguments, string accentColor, bool isPinned) {
        Name = name ?? String.Empty;
        Target = target ?? String.Empty;
        Arguments = arguments ?? String.Empty;
        AccentColor = accentColor ?? String.Empty;
        IsPinned = isPinned;
    }
}

public static class ArchAppSearchCatalog {
    public static AppSearchEntry[] All() {
        return new[] {
            new AppSearchEntry("Files", "explorer.exe", "", "#72C7F3", true),
            new AppSearchEntry("Browser", "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe", "", "#1793D1", true),
            new AppSearchEntry("Terminal", "wt.exe", "", "#8AD5F5", true),
            new AppSearchEntry("Code", "C:\\Users\\labta\\AppData\\Local\\Programs\\Microsoft VS Code\\Code.exe", "", "#4EA1FF", true),
            new AppSearchEntry("Arduino IDE", "C:\\Users\\labta\\AppData\\Local\\Programs\\Arduino IDE\\Arduino IDE.exe", "", "#00979D", true),
            new AppSearchEntry("Antigravity", "C:\\Users\\labta\\AppData\\Local\\Programs\\antigravity\\Antigravity.exe", "", "#9B8BFF", true),
            new AppSearchEntry("Network Settings", "ms-settings:network", "", "#7CC5F2", true),
            new AppSearchEntry("Volume", "sndvol.exe", "", "#95A8B8", true),
            new AppSearchEntry("Settings", "ms-settings:", "", "#B8C0CA", true),
            new AppSearchEntry("Command Prompt", "cmd.exe", "", "#00A4EF", false),
            new AppSearchEntry("PowerShell", "powershell.exe", "", "#5391FE", false),
            new AppSearchEntry("Task Manager", "taskmgr.exe", "", "#0078D7", false),
            new AppSearchEntry("Control Panel", "control.exe", "", "#0078D7", false),
            new AppSearchEntry("Registry Editor", "regedit.exe", "", "#0078D7", false),
            new AppSearchEntry("Paint", "mspaint.exe", "", "#0078D7", false),
            new AppSearchEntry("Notepad", "notepad.exe", "", "#0078D7", false),
            new AppSearchEntry("Snipping Tool", "snippingtool.exe", "", "#0078D7", false),
            new AppSearchEntry("Calculator", "calc.exe", "", "#0078D7", false)
        };
    }

    public static AppSearchEntry[] Filter(AppSearchEntry[] entries, string query) {
        if (entries == null) return Array.Empty<AppSearchEntry>();
        string q = (query ?? String.Empty).Trim();
        if (String.IsNullOrEmpty(q)) return entries.Where(e => e.IsPinned).ToArray();

        string lower = q.ToLowerInvariant();
        return entries
            .Select(e => new { Entry = e, Score = Score(e.Name, lower) })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Entry.Name, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.Entry)
            .ToArray();
    }

    private static int Score(string name, string lower) {
        if (String.IsNullOrEmpty(name)) return 0;
        string lowerName = name.ToLowerInvariant();
        if (lowerName.StartsWith(lower, StringComparison.Ordinal)) return 2;
        if (lowerName.IndexOf(lower, StringComparison.Ordinal) >= 0) return 1;
        return 0;
    }
}

using System;

public sealed class DockLauncher {
    public string Label { get; private set; }
    public string Glyph { get; private set; }
    public string Target { get; private set; }
    public string Arguments { get; private set; }
    public string AccentColor { get; private set; }

    public DockLauncher(string label, string glyph, string target, string arguments, string accentColor) {
        Label = label;
        Glyph = glyph;
        Target = target;
        Arguments = arguments ?? String.Empty;
        AccentColor = accentColor;
    }
}

public static class ArchDockCatalog {
    public static DockLauncher[] GetItems() {
        return new[] {
            new DockLauncher("Files", "FILE", "explorer.exe", "", "#72C7F3"),
            new DockLauncher("Browser", "WEB", "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe", "", "#1793D1"),
            new DockLauncher("Terminal", ">_", "wt.exe", "", "#8AD5F5"),
            new DockLauncher("Code", "</>", "C:\\Users\\labta\\AppData\\Local\\Programs\\Microsoft VS Code\\Code.exe", "", "#4EA1FF"),
            new DockLauncher("Arduino IDE", "A", "C:\\Users\\labta\\AppData\\Local\\Programs\\Arduino IDE\\Arduino IDE.exe", "", "#00979D"),
            new DockLauncher("Antigravity", "AI", "C:\\Users\\labta\\AppData\\Local\\Programs\\antigravity\\Antigravity.exe", "", "#9B8BFF"),
            new DockLauncher("Network", "NET", "ms-settings:network", "", "#7CC5F2"),
            new DockLauncher("Volume", "VOL", "sndvol.exe", "", "#95A8B8"),
            new DockLauncher("Settings", "SET", "ms-settings:", "", "#B8C0CA")
        };
    }
}

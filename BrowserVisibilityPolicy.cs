using System;
using System.Collections.Generic;

public static class BrowserVisibilityPolicy {
    private static readonly HashSet<string> BrowserProcesses = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
        "arc", "brave", "chrome", "chromium", "firefox", "msedge", "opera", "opera_gx", "vivaldi"
    };

    public static bool ShouldHide(bool enabled, string foregroundProcessName) {
        if (!enabled || String.IsNullOrWhiteSpace(foregroundProcessName)) return false;

        string name = foregroundProcessName.Trim();
        if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) {
            name = name.Substring(0, name.Length - 4);
        }
        return BrowserProcesses.Contains(name);
    }
}

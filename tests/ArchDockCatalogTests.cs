using System;
using System.Linq;

internal static class ArchDockCatalogTests {
    private static int failures;

    private static void Assert(bool condition, string message) {
        if (!condition) {
            Console.Error.WriteLine("FAIL: " + message);
            failures++;
        } else {
            Console.WriteLine("PASS: " + message);
        }
    }

    public static int Main() {
        DockLauncher[] items = ArchDockCatalog.GetItems();
        string[] labels = items.Select(item => item.Label).ToArray();
        Assert(labels.Distinct(StringComparer.OrdinalIgnoreCase).Count() == labels.Length, "dock launcher labels are unique");
        Assert(labels.Contains("Files") && labels.Contains("Browser") && labels.Contains("Terminal"), "core desktop launchers exist");
        Assert(labels.Contains("Code") && labels.Contains("Arduino IDE") && labels.Contains("Antigravity") && labels.Contains("Settings"), "installed apps and settings are pinned");
        Assert(labels.Contains("Volume") && labels.Contains("Network") && items.Length == 9, "system tray shortcuts are included");
        Assert(items.All(item => !String.IsNullOrWhiteSpace(item.Target)), "every launcher has a target");
        return failures == 0 ? 0 : 1;
    }
}

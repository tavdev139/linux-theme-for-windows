using System;

public static class ArchDockVisibilityPolicy {
    public static bool ShouldShow(bool autoHide, string foregroundProcess, string foregroundTitle, bool pointerAtRevealEdge, bool dockHovered) {
        if (!autoHide || pointerAtRevealEdge || dockHovered) return true;

        string process = (foregroundProcess ?? String.Empty).Trim();
        string title = (foregroundTitle ?? String.Empty).Trim();
        if (String.Equals(process, "ArchPanel", StringComparison.OrdinalIgnoreCase) ||
            String.Equals(process, "ArchDock", StringComparison.OrdinalIgnoreCase)) return true;

        return String.Equals(process, "explorer", StringComparison.OrdinalIgnoreCase) &&
               String.Equals(title, "Program Manager", StringComparison.OrdinalIgnoreCase);
    }
}

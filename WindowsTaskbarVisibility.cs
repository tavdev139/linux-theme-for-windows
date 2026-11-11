using System;
using System.Runtime.InteropServices;

public static class WindowsTaskbarVisibility {
    private const int SW_HIDE = 0;
    private const int SW_SHOWNA = 8;
    private static bool s_originallyVisible;
    private static bool s_hiddenByUs;

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindow(string className, string windowName);
    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr window);
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr window, int command);

    public static void HideNativeTaskbar() {
        try {
            IntPtr taskbar = FindWindow("Shell_TrayWnd", null);
            if (taskbar == IntPtr.Zero) return;
            if (!s_hiddenByUs) s_originallyVisible = IsWindowVisible(taskbar);
            s_hiddenByUs = true;
            if (IsWindowVisible(taskbar)) ShowWindow(taskbar, SW_HIDE);
        } catch { }
    }

    public static void RestoreNativeTaskbar() {
        try {
            if (!s_hiddenByUs) return;
            IntPtr taskbar = FindWindow("Shell_TrayWnd", null);
            if (taskbar != IntPtr.Zero && s_originallyVisible) ShowWindow(taskbar, SW_SHOWNA);
            s_hiddenByUs = false;
        } catch { }
    }
}

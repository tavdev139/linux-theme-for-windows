using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

// Compatibility launcher for older builds. The preview itself lives in
// linux_lockscreen.ps1 and deliberately collects no credentials.
public static class LockScreenProgram {
    [STAThread]
    public static void Main() {
        string dir = AppDomain.CurrentDomain.BaseDirectory;
        string preview = Path.Combine(dir, "linux_lockscreen.ps1");
        if (!File.Exists(preview)) {
            MessageBox.Show(
                "File pratinjau linux_lockscreen.ps1 tidak ditemukan.",
                "Pratinjau lock screen",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        var start = new ProcessStartInfo {
            FileName = "powershell.exe",
            Arguments = "-NoProfile -STA -WindowStyle Hidden -ExecutionPolicy Bypass -File \"" + preview + "\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = dir
        };
        Process.Start(start);
    }
}

$source = @"
using System;
using System.Runtime.InteropServices;
public static class NativeTaskbarRestore {
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr FindWindow(string className, string windowName);
    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr window, int command);
}
"@
Add-Type -TypeDefinition $source -ErrorAction SilentlyContinue
$taskbar = [NativeTaskbarRestore]::FindWindow("Shell_TrayWnd", $null)
if ($taskbar -ne [IntPtr]::Zero) {
    [void][NativeTaskbarRestore]::ShowWindow($taskbar, 8) # SW_SHOWNA
    Write-Host "Windows taskbar restored. Its native auto-hide setting is unchanged."
} else {
    Write-Host "Windows taskbar window was not found."
}

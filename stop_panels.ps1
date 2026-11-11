# Stop compiled islands first so the work area can be reset cleanly.
Get-Process -Name "LinuxDynamicIsland","ArchPanel" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
if (Test-Path "$PSScriptRoot\restore_windows_taskbar.ps1") { & "$PSScriptRoot\restore_windows_taskbar.ps1" }

# Stop any running script instances.
$procs = Get-CimInstance Win32_Process | Where-Object {
    $_.CommandLine -like "*gnome_panel*" -or
    $_.CommandLine -like "*dynamic_island*" -or
    $_.CommandLine -like "*Linux Dynamic Island*"
}

foreach ($p in $procs) {
    if ($p.ProcessId -ne $PID) {
        Write-Host "Stopping process: $($p.ProcessId) - $($p.Name)"
        Stop-Process -Id $p.ProcessId -Force -ErrorAction SilentlyContinue
    }
}

# Also ensure workarea is reset
Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
public class ScreenResetter {
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref RECT pvParam, uint fWinIni);
    public static void Reset() {
        RECT r = new RECT();
        SystemParametersInfo(0x0030, 0, ref r, 0); // SPI_GETWORKAREA
        r.Top = 0;
        SystemParametersInfo(0x002F, 0, ref r, 2); // SPI_SETWORKAREA, SPIF_SENDCHANGE
    }
}
"@ -ErrorAction SilentlyContinue

try { [ScreenResetter]::Reset() } catch {}
Write-Host "Cleanup completed."

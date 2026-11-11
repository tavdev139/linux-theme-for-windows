# ==============================================================
# Skrip untuk menghentikan Dynamic Island dan mencopot startup
# ==============================================================

# 1. Hentikan semua proses island dan lockscreen
Get-Process -Name "LinuxDynamicIsland","ArchPanel" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Get-Process -Name "LinuxLockScreen" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
if (Test-Path "$PSScriptRoot\restore_windows_taskbar.ps1") { & "$PSScriptRoot\restore_windows_taskbar.ps1" }

$procs = Get-CimInstance Win32_Process | Where-Object {
    $_.CommandLine -like "*gnome_panel*" -or
    $_.CommandLine -like "*dynamic_island*" -or
    $_.CommandLine -like "*linux_lockscreen*"
}
foreach ($p in $procs) {
    if ($p.ProcessId -ne $PID) {
        Stop-Process -Id $p.ProcessId -Force -ErrorAction SilentlyContinue
    }
}

# 2. Kembalikan area kerja layar ke normal (Reset WorkArea)
Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
public class ScreenWorkAreaRestorer {
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

try { [ScreenWorkAreaRestorer]::Reset() } catch {}

# 3. Hapus runner dari folder Startup Windows
$startupFolder = [Environment]::GetFolderPath("Startup")
$lnk = "$startupFolder\LinuxDynamicIsland.lnk"
if (Test-Path $lnk) { Remove-Item -Path $lnk -Force -ErrorAction SilentlyContinue }
$vbs = "$startupFolder\LinuxDynamicIsland.vbs"
if (Test-Path $vbs) { Remove-Item -Path $vbs -Force -ErrorAction SilentlyContinue }

# 4. Hapus dari Registry Run Key
$runKey = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run"
Remove-ItemProperty -Path $runKey -Name "LinuxDynamicIsland" -ErrorAction SilentlyContinue

# 5. Tampilkan pesan popup Windows
Add-Type -AssemblyName PresentationFramework
[System.Windows.MessageBox]::Show(
    "Dynamic Island & Tema Linux telah berhasil DIHENTIKAN dan DICOPOT dari startup Windows.`n`nArea layar telah dikembalikan ke ukuran normal.",
    "Dynamic Island Dicopot",
    [System.Windows.MessageBoxButton]::OK,
    [System.Windows.MessageBoxImage]::Information
) | Out-Null

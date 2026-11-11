# ==============================================================
# Installer Permanen Tema Linux, Lock Screen & Dynamic Island
# 100% Native Windows Executable (.exe) - Bebas dari Terminal!
# ==============================================================

$themeDir = "c:\Users\labta\Downloads\theme"
$islandExe = "$themeDir\ArchPanel.exe"

# 1. Hentikan semua proses lama (skrip powershell maupun exe)
Get-Process -Name "LinuxDynamicIsland","ArchPanel" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
if (Test-Path "$PSScriptRoot\restore_windows_taskbar.ps1") { & "$PSScriptRoot\restore_windows_taskbar.ps1" }

$oldProcs = Get-CimInstance Win32_Process | Where-Object {
    $_.CommandLine -like "*gnome_panel*" -or
    $_.CommandLine -like "*dynamic_island*"
}
foreach ($p in $oldProcs) {
    if ($p.ProcessId -ne $PID) {
        Stop-Process -Id $p.ProcessId -Force -ErrorAction SilentlyContinue
    }
}

# 2. Terapkan Wallpaper Desktop Ubuntu HD
$wpPath = "$themeDir\wallpapers\ubuntu_desktop.png"
if (Test-Path $wpPath) {
    Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
public class WallpaperSetter {
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
}
"@ -ErrorAction SilentlyContinue
    try {
        [WallpaperSetter]::SystemParametersInfo(0x0014, 0, $wpPath, 0x01 -bor 0x02) | Out-Null
    } catch {}
}

# 3. Terapkan Wallpaper Lock Screen Resmi Windows via WinRT API
if (Test-Path $wpPath) {
    try {
        Add-Type -AssemblyName System.Runtime.WindowsRuntime -ErrorAction SilentlyContinue
        $null = [Windows.Storage.StorageFile, Windows.Storage, ContentType = WindowsRuntime]
        $null = [Windows.System.UserProfile.LockScreen, Windows.System.UserProfile, ContentType = WindowsRuntime]

        $asTaskGeneric = [System.WindowsRuntimeSystemExtensions].GetMethods() | Where-Object { 
            $_.Name -eq 'AsTask' -and $_.GetParameters().Count -eq 1 -and $_.IsGenericMethod 
        } | Select-Object -First 1

        $op = [Windows.Storage.StorageFile]::GetFileFromPathAsync($wpPath)
        $asTask = $asTaskGeneric.MakeGenericMethod([Windows.Storage.StorageFile])
        $task = $asTask.Invoke($null, @($op))
        $file = $task.GetAwaiter().GetResult()

        $setOp = [Windows.System.UserProfile.LockScreen]::SetImageFileAsync($file)
        $asTaskVoid = [System.WindowsRuntimeSystemExtensions].GetMethods() | Where-Object { 
            $_.Name -eq 'AsTask' -and $_.GetParameters().Count -eq 1 -and -not $_.IsGenericMethod 
        } | Select-Object -First 1
        $setTask = $asTaskVoid.Invoke($null, @($setOp))
        $setTask.GetAwaiter().GetResult() | Out-Null
    } catch {}
}

# 4. Terapkan Windows Dark Mode & Aksen Warna Oranye Ubuntu di Registry
$regPersonalize = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"
if (Test-Path $regPersonalize) {
    Set-ItemProperty -Path $regPersonalize -Name "AppsUseLightTheme" -Value 0 -ErrorAction SilentlyContinue
    Set-ItemProperty -Path $regPersonalize -Name "SystemUsesLightTheme" -Value 0 -ErrorAction SilentlyContinue
    Set-ItemProperty -Path $regPersonalize -Name "ColorPrevalence" -Value 1 -ErrorAction SilentlyContinue
}

$regDwm = "HKCU:\Software\Microsoft\Windows\DWM"
if (Test-Path $regDwm) {
    Set-ItemProperty -Path $regDwm -Name "AccentColor" -Value 0x2054E9 -ErrorAction SilentlyContinue
    Set-ItemProperty -Path $regDwm -Name "ColorizationColor" -Value 0xC4E95420 -ErrorAction SilentlyContinue
}

# 5. Pasang Shortcut .lnk langsung ke folder Startup Windows
$startupFolder = [Environment]::GetFolderPath("Startup")
$wsh = New-Object -ComObject WScript.Shell
$shortcut = $wsh.CreateShortcut("$startupFolder\LinuxDynamicIsland.lnk")
$shortcut.TargetPath = $islandExe
$shortcut.WorkingDirectory = $themeDir
$shortcut.Description = "Linux Dynamic Island"
$shortcut.Save()

# Hapus file VBS lama di startup jika ada
if (Test-Path "$startupFolder\LinuxDynamicIsland.vbs") {
    Remove-Item "$startupFolder\LinuxDynamicIsland.vbs" -Force -ErrorAction SilentlyContinue
}

# 6. Pasang juga ke Registry Run Key sebagai proteksi ganda (100% Permanen)
$runKey = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run"
Set-ItemProperty -Path $runKey -Name "LinuxDynamicIsland" -Value "`"$islandExe`"" -ErrorAction SilentlyContinue

# 7. Jalankan LinuxDynamicIsland.exe sekarang juga
Start-Process -FilePath $islandExe -WorkingDirectory $themeDir

# 8. Tampilkan notifikasi popup Windows (native MsgBox)
Add-Type -AssemblyName PresentationFramework
[System.Windows.MessageBox]::Show(
    "TEMA LINUX, LOCK SCREEN & DYNAMIC ISLAND BERHASIL DIPASANG PERMANEN!`n`n" +
    "Kini Dynamic Island berupa aplikasi Windows murni (.exe) yang TERPISAH dari terminal:`n" +
    "1. Menutup tab terminal TIDAK AKAN PERNAH menutup Dynamic Island lagi.`n" +
    "2. Dynamic Island otomatis aktif saat Windows dinyalakan (Startup & Registry).`n" +
    "3. No-Cover Mode: Tab browser & jendela penuh tidak tertutup.`n" +
    "4. Lock Screen Linux telah aktif (klik Power > Lock Screen atau klik 2x KUNCI_LAYAR_LINUX.vbs).`n`n" +
    "Untuk mencopot/menghentikan: klik 2x file 'HAPUS_DAN_HENTIKAN_TEMA.vbs'.",
    "Pemasangan Sukses - Linux Theme Windows",
    [System.Windows.MessageBoxButton]::OK,
    [System.Windows.MessageBoxImage]::Information
) | Out-Null

param(
    [string]$ThemeName = "ubuntu"
)

$baseDir = Split-Path -Parent $MyInvocation.MyCommand.Path

$themeConfig = @{
    "ubuntu" = @{
        Name = "Ubuntu Linux Dark"
        Wallpaper = "$baseDir\wallpapers\ubuntu_desktop.png"
        Color = 0xE95420
        Rgb = "233, 84, 32"
    }
    "matrix" = @{
        Name = "Matrix Cyber Linux"
        Wallpaper = "$baseDir\wallpapers\matrix_desktop.png"
        Color = 0x00FF66
        Rgb = "0, 255, 102"
    }
    "arch" = @{
        Name = "Arch Linux Dark"
        Wallpaper = "$baseDir\wallpapers\arch_desktop.png"
        Color = 0x1793D1
        Rgb = "23, 147, 209"
    }
}

$selected = $themeConfig[$ThemeName.ToLower()]
if (-not $selected) {
    $selected = $themeConfig["ubuntu"]
}

Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "  Menerapkan $($selected.Name) di Windows..." -ForegroundColor Green
Write-Host "========================================================" -ForegroundColor Cyan

# 1. Atur Wallpaper via Win32 API
Add-Type @"
using System;
using System.Runtime.InteropServices;
public class WinWallpaper {
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
}
"@ -ErrorAction SilentlyContinue

$wpPath = $selected.Wallpaper
if (Test-Path $wpPath) {
    # Set registry wallpaper path
    Set-ItemProperty -Path "HKCU:\Control Panel\Desktop" -Name Wallpaper -Value $wpPath
    Set-ItemProperty -Path "HKCU:\Control Panel\Desktop" -Name TileWallpaper -Value "0"
    Set-ItemProperty -Path "HKCU:\Control Panel\Desktop" -Name WallpaperStyle -Value "10"
    
    # Broadcast change
    $res = [WinWallpaper]::SystemParametersInfo(20, 0, $wpPath, 3)
    Write-Host "[1/4] Wallpaper berhasil diubah: $(Split-Path $wpPath -Leaf)" -ForegroundColor Green
} else {
    Write-Host "[!] File wallpaper tidak ditemukan di $wpPath" -ForegroundColor Red
}

# 2. Gunakan wallpaper Arch yang sama pada lockscreen Windows asli
if (Test-Path -LiteralPath $wpPath) {
    try {
        Add-Type -AssemblyName System.Runtime.WindowsRuntime -ErrorAction Stop
        $null = [Windows.Storage.StorageFile, Windows.Storage, ContentType = WindowsRuntime]
        $null = [Windows.System.UserProfile.LockScreen, Windows.System.UserProfile, ContentType = WindowsRuntime]

        $asTaskGeneric = [System.WindowsRuntimeSystemExtensions].GetMethods() | Where-Object {
            $_.Name -eq 'AsTask' -and $_.GetParameters().Count -eq 1 -and $_.IsGenericMethod
        } | Select-Object -First 1
        $fileOperation = [Windows.Storage.StorageFile]::GetFileFromPathAsync($wpPath)
        $fileTask = $asTaskGeneric.MakeGenericMethod([Windows.Storage.StorageFile]).Invoke($null, @($fileOperation))
        $storageFile = $fileTask.GetAwaiter().GetResult()

        $setOperation = [Windows.System.UserProfile.LockScreen]::SetImageFileAsync($storageFile)
        $asTaskAction = [System.WindowsRuntimeSystemExtensions].GetMethods() | Where-Object {
            $_.Name -eq 'AsTask' -and $_.GetParameters().Count -eq 1 -and -not $_.IsGenericMethod
        } | Select-Object -First 1
        $setTask = $asTaskAction.Invoke($null, @($setOperation))
        $setTask.GetAwaiter().GetResult() | Out-Null
        Write-Host "[2/4] Wallpaper lockscreen Windows berhasil diatur ke wallpaper Arch." -ForegroundColor Green
    } catch {
        Write-Host "[!] Wallpaper desktop aktif; lockscreen tidak berubah: $_" -ForegroundColor Yellow
    }
}

# 3. Atur Dark Mode & Warna Aksen di Registry Windows
try {
    $personalizeKey = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"
    if (-not (Test-Path $personalizeKey)) {
        New-Item -Path $personalizeKey -Force | Out-Null
    }
    Set-ItemProperty -Path $personalizeKey -Name AppsUseLightTheme -Value 0 -Type DWord
    Set-ItemProperty -Path $personalizeKey -Name SystemUsesLightTheme -Value 0 -Type DWord
    Set-ItemProperty -Path $personalizeKey -Name ColorPrevalence -Value 1 -Type DWord

    $dwmKey = "HKCU:\Software\Microsoft\Windows\DWM"
    if (-not (Test-Path $dwmKey)) {
        New-Item -Path $dwmKey -Force | Out-Null
    }
    Set-ItemProperty -Path $dwmKey -Name AccentColor -Value $selected.Color -Type DWord
    Set-ItemProperty -Path $dwmKey -Name ColorizationColor -Value $selected.Color -Type DWord
    Set-ItemProperty -Path $dwmKey -Name ColorPrevalence -Value 1 -Type DWord

    Write-Host "[3/4] Windows Dark Mode & Warna Aksen Linux berhasil diaktifkan!" -ForegroundColor Green
} catch {
    Write-Host "[!] Gagal memperbarui registry warna: $_" -ForegroundColor Yellow
}

# 3. Salin & Daftarkan ke folder tema Windows pengguna
$themesFolder = "$env:LOCALAPPDATA\Microsoft\Windows\Themes"
if (-not (Test-Path $themesFolder)) {
    New-Item -Path $themesFolder -ItemType Directory -Force | Out-Null
}

$themeFileName = switch ($ThemeName.ToLower()) {
    "matrix" { "Matrix_Cyber_Linux.theme" }
    "arch"   { "Arch_Linux_Dark.theme" }
    default  { "Ubuntu_Linux_Dark.theme" }
}

$sourceTheme = "$baseDir\$themeFileName"
if (Test-Path $sourceTheme) {
    Copy-Item $sourceTheme "$themesFolder\$themeFileName" -Force
    # Jalankan theme file agar Windows settings mengaplikasikannya secara native
    Start-Process -FilePath "$themesFolder\$themeFileName"
    Write-Host "[4/4] File tema Windows resmi berhasil didaftarkan!" -ForegroundColor Green
}

Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "  SELESAI! Tema Linux sekarang aktif pada sistem Windows." -ForegroundColor Green
Write-Host "========================================================" -ForegroundColor Cyan

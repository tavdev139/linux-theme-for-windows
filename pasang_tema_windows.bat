@echo off
title Pasang Tema Linux di Windows
color 0B
cd /d "%~dp0"

echo ========================================================
echo   PEMASANG TEMA SISTEM LINUX UNTUK WINDOWS
echo ========================================================
echo.
echo Pilih varian tema yang ingin diterapkan pada sistem Windows Anda:
echo.
echo  [1] Ubuntu Linux Dark   (Wallpaper Ubuntu + Dark Mode + Aksen Oranye)
echo  [2] Matrix Cyber Linux  (Wallpaper Matrix + Dark Mode + Aksen Hijau Neon)
echo  [3] Arch Linux Dark     (Wallpaper Arch + Dark Mode + Aksen Biru)
echo  [0] Batal
echo.

set /p pilihan="Masukkan angka pilihan (1-3): "

if "%pilihan%"=="1" (
    powershell -ExecutionPolicy Bypass -File "%~dp0apply_theme.ps1" -ThemeName ubuntu
    goto done
)
if "%pilihan%"=="2" (
    powershell -ExecutionPolicy Bypass -File "%~dp0apply_theme.ps1" -ThemeName matrix
    goto done
)
if "%pilihan%"=="3" (
    powershell -ExecutionPolicy Bypass -File "%~dp0apply_theme.ps1" -ThemeName arch
    goto done
)

echo Dibatalkan.
pause
exit

:done
echo.
echo ========================================================
echo Perubahan telah diterapkan pada sistem Windows Anda!
echo ========================================================
pause

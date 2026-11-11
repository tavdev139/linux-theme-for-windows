@echo off
chcp 65001 >nul 2>&1
title Ubah Windows Jadi Linux
color 0A
cls
echo.
echo  ========================================================
echo     UBAH TAMPILAN WINDOWS JADI LINUX
echo     Ubuntu / Matrix / Arch + Dynamic Island
echo  ========================================================
echo.
echo  [1] Pasang Permanen Tema Ubuntu + Dynamic Island (Rekomendasi)
echo  [2] Dynamic Island saja (tanpa ganti wallpaper)
echo  [3] Install TranslucentTB (taskbar bening/transparan)
echo  [4] Hentikan / Copot Dynamic Island & Kembalikan Layar
echo  [5] Tema Matrix Cyber
echo  [6] Tema Arch Linux Dark
echo  [7] Keluar
echo.
set /p pilih="Pilih [1-7]: "

if "%pilih%"=="1" goto full_permanent
if "%pilih%"=="2" goto dynamic_island
if "%pilih%"=="3" goto translucent
if "%pilih%"=="4" goto uninstall
if "%pilih%"=="5" goto matrix
if "%pilih%"=="6" goto arch
if "%pilih%"=="7" goto keluar
goto keluar

:full_permanent
echo.
echo [*] Memasang Tema Ubuntu + Dynamic Island secara permanen...
powershell.exe -ExecutionPolicy Bypass -File "%~dp0install_permanent.ps1"
goto keluar

:dynamic_island
echo.
echo [*] Menghentikan proses lama...
powershell.exe -ExecutionPolicy Bypass -File "%~dp0stop_panels.ps1"
echo [*] Menjalankan Dynamic Island...
start /B powershell.exe -WindowStyle Hidden -ExecutionPolicy Bypass -File "%~dp0dynamic_island.ps1"
echo [OK] Dynamic Island aktif!
pause
goto keluar

:translucent
echo.
echo [*] Menginstall TranslucentTB untuk taskbar transparan...
winget install --id CharlesMilette.TranslucentTB --accept-package-agreements --accept-source-agreements
echo [OK] TranslucentTB terinstall! Buka dari system tray untuk konfigurasi.
pause
goto keluar

:uninstall
echo.
echo [*] Menghentikan Dynamic Island dan mencopot dari startup...
powershell.exe -ExecutionPolicy Bypass -File "%~dp0uninstall.ps1"
goto keluar

:matrix
echo.
echo [*] Menerapkan tema Matrix Cyber Linux...
set "THEME_VARIANT=matrix"
powershell.exe -ExecutionPolicy Bypass -File "%~dp0apply_theme.ps1"
echo [OK] Tema Matrix aktif!
pause
goto keluar

:arch
echo.
echo [*] Menerapkan tema Arch Linux Dark...
set "THEME_VARIANT=arch"
powershell.exe -ExecutionPolicy Bypass -File "%~dp0apply_theme.ps1"
echo [OK] Tema Arch aktif!
pause
goto keluar

:keluar
exit

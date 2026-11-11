@echo off
title Arch Panel
echo =======================================================
echo   ARCH LINUX PANEL FOR WINDOWS
echo =======================================================
echo.
echo [*] Memulai Arch Panel...
powershell.exe -ExecutionPolicy Bypass -File "%~dp0stop_panels.ps1" >nul 2>&1
echo.
echo Fitur:
echo   - Terapung di tengah atas layar (tidak menutupi tab/tombol close)
echo   - Arahkan mouse (hover) untuk melihat CPU, RAM, & Quick Actions
echo   - Drag dengan klik kiri mouse untuk memindahkan posisi
echo   - Klik kanan untuk opsi (Center, Pin, Keluar)
echo   - Arch Dock di bawah: Files, Browser, Terminal, Code, Arduino, Volume, Network, Settings, Jam
echo   - Dock auto-hide saat aplikasi aktif; arahkan pointer ke tepi bawah untuk menampilkannya
echo   - Windows bar disembunyikan saat Arch aktif dan dipulihkan saat keluar
echo.
start "" /B "%~dp0ArchPanel.exe"
echo [OK] Arch Panel berjalan di background!
timeout /t 3 >nul

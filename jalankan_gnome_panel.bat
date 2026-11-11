@echo off
title Linux Top Bar & Dynamic Island
cd /d "%~dp0"
echo [*] Menghentikan panel lama...
powershell.exe -ExecutionPolicy Bypass -File "%~dp0stop_panels.ps1" >nul 2>&1
start "" powershell.exe -WindowStyle Hidden -ExecutionPolicy Bypass -File "%~dp0dynamic_island.ps1"
exit

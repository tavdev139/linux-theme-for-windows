@echo off
title Samsung Linux Theme Server
color 0A
cd /d "%~dp0"

echo ========================================================
echo   🐧 SAMSUNG LINUX THEME - LAUNCHER
echo ========================================================
echo.

where node >nul 2>nul
if %ERRORLEVEL% equ 0 (
    echo [INFO] Menjalankan server menggunakan Node.js...
    start http://localhost:8080
    node server.js
    goto end
)

where python >nul 2>nul
if %ERRORLEVEL% equ 0 (
    echo [INFO] Menjalankan server menggunakan Python...
    start http://localhost:8080
    python -m http.server 8080
    goto end
)

echo [INFO] Node.js atau Python tidak ditemukan, membuka file langsung...
start index.html

:end
pause

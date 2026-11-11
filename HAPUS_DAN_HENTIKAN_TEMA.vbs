' ==============================================================
' 1-KLIK HENTIKAN & COPOT DYNAMIC ISLAND DARI WINDOWS
' Cukup klik 2x file ini jika ingin mematikan tema.
' Berjalan 100% senyap tanpa jendela terminal atau CMD.
' ==============================================================

Set objFSO = CreateObject("Scripting.FileSystemObject")
strCurDir = objFSO.GetParentFolderName(WScript.ScriptFullName)
Set WshShell = CreateObject("WScript.Shell")

WshShell.Run "powershell.exe -WindowStyle Hidden -ExecutionPolicy Bypass -File """ & strCurDir & "\uninstall.ps1""", 0, False

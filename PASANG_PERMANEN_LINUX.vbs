' ==============================================================
' 1-KLIK PASANG PERMANEN TEMA LINUX & DYNAMIC ISLAND
' Cukup klik 2x file ini dari File Explorer!
' Berjalan 100% senyap tanpa jendela terminal atau CMD.
' ==============================================================

Set objFSO = CreateObject("Scripting.FileSystemObject")
strCurDir = objFSO.GetParentFolderName(WScript.ScriptFullName)
Set WshShell = CreateObject("WScript.Shell")

' Jalankan installer secara tersembunyi (0 = hidden)
WshShell.Run "powershell.exe -WindowStyle Hidden -ExecutionPolicy Bypass -File """ & strCurDir & "\install_permanent.ps1""", 0, False

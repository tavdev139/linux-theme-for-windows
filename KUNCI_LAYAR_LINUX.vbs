' ==============================================================
' Pratinjau visual Linux. Bukan pengunci layar Windows.
' Tombol di dalam pratinjau mengaktifkan lockscreen Windows asli.
' Esc menutup pratinjau visual.
' ==============================================================

Set objFSO = CreateObject("Scripting.FileSystemObject")
strCurDir = objFSO.GetParentFolderName(WScript.ScriptFullName)
Set WshShell = CreateObject("WScript.Shell")

strScript = strCurDir & "\linux_lockscreen.ps1"
strCommand = "powershell.exe -NoProfile -STA -WindowStyle Hidden -ExecutionPolicy Bypass -File """ & strScript & """"
WshShell.Run strCommand, 0, False

@echo off
setlocal
"C:\Program Files (x86)\WiX Toolset v3.14\bin\insignia.exe" -ib %1 -o engine.exe
call "%~dp0sign.bat" "engine.exe"
"C:\Program Files (x86)\WiX Toolset v3.14\bin\insignia.exe" -ab engine.exe %1 -o %1
call "%~dp0sign.bat" %1 %2
endlocal
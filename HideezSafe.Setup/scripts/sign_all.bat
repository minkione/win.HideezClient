:: %1 is output directory

@echo off
setlocal
@echo p %1
for %%G in (.exe, .dll) do forfiles /p %1 /s /m *%%G /c "cmd /c %~dp0sign.bat @path" 2>nul
endlocal
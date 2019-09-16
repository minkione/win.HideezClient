devenv ..\HideezClient.Setup.sln /Clean "Release|x86"
devenv ..\HideezClient.Setup.sln /Clean "Release|x64"
devenv ..\HideezClient.Setup.sln /Build "Release|x86" /Project WiXSetup
devenv ..\HideezClient.Setup.sln /Build "Release|x64" /Project WiXSetup
devenv ..\HideezClient.Setup.sln /Build "Release|x64" /Project WixBootstrapper

if not exist "..\Release\" mkdir "..\Release\"
if not exist "..\Release\x64" mkdir "..\Release\x64"
if not exist "..\Release\x86" mkdir "..\Release\x86"
xcopy /s/y "..\HideezClient.Setup\WiXSetup\bin\x64\Release\hideezclient.msi" "..\Release\x64"
xcopy /s/y "..\HideezClient.Setup\WiXSetup\bin\Release\hideezclient.msi" "..\Release\x86"
xcopy /s/y "..\HideezClient.Setup\WixBootstrapper\bin\x64\Release\hideezclient.exe" "..\Release\"
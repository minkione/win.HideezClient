:: %1 is Full path to file
:: %2 is description

:: Add :: before GOTO statement to enable .exe and .dll signing 
@echo off
call "%~dp0signing_variables.bat"
IF [%2]==[] (
	%TOOLPATH% sign /n %CERTNAME% /sha1 %CERTSHA1% /fd SHA256 /td SHA256 /tr "http://timestamp.comodoca.com" %1
) ELSE (
	%TOOLPATH% sign /n %CERTNAME% /sha1 %CERTSHA1% /fd SHA256 /td SHA256 /tr "http://timestamp.comodoca.com" /d %2 %1
)
:: Comodo: "Please add a delay of 15 seconds or more between signings so that you're not hammering our servers."
ping 127.0.0.1 -n 16 > nul
:SKIP
endlocal
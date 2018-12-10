@echo off
C:\Windows\Microsoft.NET\Framework\v4.0.30319\installutil.exe AlarmServer.exe

if ERRORLEVEL 1 goto error
exit
:error
echo There was a problem
pause
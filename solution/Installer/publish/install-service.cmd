@echo off
cd /d "%~dp0"
sc stop BalanzaService 2>nul
sc delete BalanzaService 2>nul
timeout /t 2 /nobreak >nul
sc create BalanzaService binPath= "\"%~dp0BalanzaService.exe\"" start= auto DisplayName= "Balanza Service - KFC"
sc description BalanzaService "Servicio de lectura de balanzas y envio a SAP"
sc failure BalanzaService reset= 86400 actions= restart/5000/restart/10000/restart/30000
netsh advfirewall firewall delete rule name="BalanzaService HTTP" 2>nul
netsh advfirewall firewall add rule name="BalanzaService HTTP" dir=in action=allow protocol=TCP localport=80
if exist "%~dp0..\appsettings.json" copy /Y "%~dp0..\appsettings.json" "%~dp0appsettings.json"
sc start BalanzaService

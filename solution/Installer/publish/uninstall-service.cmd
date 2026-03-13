@echo off
sc stop BalanzaService 2>nul
timeout /t 3 /nobreak >nul
sc delete BalanzaService 2>nul
netsh advfirewall firewall delete rule name="BalanzaService HTTP" 2>nul

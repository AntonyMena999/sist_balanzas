#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Desinstala BalanzaService

.DESCRIPTION
    Detiene y elimina el servicio de Windows BalanzaService

.PARAMETER ServiceName
    Nombre del servicio (default: BalanzaService)

.PARAMETER RemoveFiles
    Si se especifica, elimina tambien los archivos de instalacion

.PARAMETER InstallPath
    Ruta de instalacion (default: C:\BalanzaService)

.EXAMPLE
    .\Uninstall-Service.ps1

.EXAMPLE
    .\Uninstall-Service.ps1 -RemoveFiles -InstallPath "D:\Servicios\Balanza"
#>

param(
    [string]$ServiceName = "BalanzaService",
    [switch]$RemoveFiles,
    [string]$InstallPath = "C:\BalanzaService"
)

$ErrorActionPreference = "Stop"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Desinstalador de BalanzaService" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Verificar si el servicio existe
$existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if (-not $existingService) {
    Write-Host "El servicio '$ServiceName' no existe." -ForegroundColor Yellow
    exit 0
}

# Detener el servicio
Write-Host "Deteniendo servicio '$ServiceName'..." -ForegroundColor Yellow
Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

# Eliminar el servicio
Write-Host "Eliminando servicio..." -ForegroundColor Yellow
sc.exe delete $ServiceName

if ($LASTEXITCODE -eq 0) {
    Write-Host "Servicio eliminado correctamente." -ForegroundColor Green
} else {
    Write-Host "Error al eliminar el servicio. Codigo: $LASTEXITCODE" -ForegroundColor Red
}

# Eliminar archivos si se solicita
if ($RemoveFiles) {
    Write-Host "Eliminando archivos de instalacion..." -ForegroundColor Yellow
    if (Test-Path $InstallPath) {
        Remove-Item -Path $InstallPath -Recurse -Force
        Write-Host "Archivos eliminados: $InstallPath" -ForegroundColor Green
    } else {
        Write-Host "Directorio no encontrado: $InstallPath" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "  Desinstalacion completada!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green

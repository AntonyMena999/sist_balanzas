#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Instala BalanzaService como servicio de Windows

.DESCRIPTION
    Este script instala el ejecutable de BalanzaService como un servicio de Windows
    que se inicia automaticamente con el sistema.

.PARAMETER ServiceName
    Nombre del servicio (default: BalanzaService)

.PARAMETER InstallPath
    Ruta donde se instalara el servicio (default: C:\BalanzaService)

.PARAMETER ExePath
    Ruta del ejecutable publicado (default: .\publish\BalanzaService.exe)

.EXAMPLE
    .\Install-Service.ps1

.EXAMPLE
    .\Install-Service.ps1 -ServiceName "BalanzaKFC" -InstallPath "D:\Servicios\Balanza"
#>

param(
    [string]$ServiceName = "BalanzaService",
    [string]$InstallPath = "C:\BalanzaService",
    [string]$ExePath = ".\publish\BalanzaService.exe"
)

$ErrorActionPreference = "Stop"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Instalador de BalanzaService" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Verificar si el servicio ya existe
$existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($existingService) {
    Write-Host "El servicio '$ServiceName' ya existe." -ForegroundColor Yellow
    $response = Read-Host "Desea desinstalarlo primero? (S/N)"
    if ($response -eq 'S' -or $response -eq 's') {
        Write-Host "Deteniendo servicio..." -ForegroundColor Yellow
        Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
        Write-Host "Eliminando servicio..." -ForegroundColor Yellow
        sc.exe delete $ServiceName
        Start-Sleep -Seconds 2
    } else {
        Write-Host "Instalacion cancelada." -ForegroundColor Red
        exit 1
    }
}

# Verificar que existe el ejecutable
if (-not (Test-Path $ExePath)) {
    Write-Host "ERROR: No se encuentra el ejecutable en: $ExePath" -ForegroundColor Red
    Write-Host "Ejecute primero: dotnet publish -c Release" -ForegroundColor Yellow
    exit 1
}

# Crear directorio de instalacion
Write-Host "Creando directorio de instalacion: $InstallPath" -ForegroundColor Green
if (-not (Test-Path $InstallPath)) {
    New-Item -ItemType Directory -Path $InstallPath -Force | Out-Null
}

# Copiar archivos
Write-Host "Copiando archivos..." -ForegroundColor Green
$publishDir = Split-Path $ExePath -Parent
Copy-Item -Path "$publishDir\*" -Destination $InstallPath -Recurse -Force

# Copiar appsettings si existe en el directorio actual
if (Test-Path ".\appsettings.json") {
    Copy-Item -Path ".\appsettings.json" -Destination $InstallPath -Force
    Write-Host "  - appsettings.json copiado" -ForegroundColor Gray
}

$exeFullPath = Join-Path $InstallPath "BalanzaService.exe"

# Crear el servicio
Write-Host "Creando servicio de Windows..." -ForegroundColor Green
$params = @{
    Name = $ServiceName
    BinaryPathName = $exeFullPath
    DisplayName = "Balanza Service - KFC"
    Description = "Servicio de lectura de balanzas y envio a SAP"
    StartupType = "Automatic"
}

New-Service @params

# Configurar recuperacion del servicio (reiniciar en caso de fallo)
Write-Host "Configurando recuperacion automatica..." -ForegroundColor Green
sc.exe failure $ServiceName reset= 86400 actions= restart/5000/restart/10000/restart/30000

# Iniciar el servicio
Write-Host "Iniciando servicio..." -ForegroundColor Green
Start-Service -Name $ServiceName

# Verificar estado
$service = Get-Service -Name $ServiceName
Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "  Instalacion completada!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""
Write-Host "Servicio: $ServiceName" -ForegroundColor White
Write-Host "Estado: $($service.Status)" -ForegroundColor White
Write-Host "Ruta: $InstallPath" -ForegroundColor White
Write-Host ""
Write-Host "Endpoint: http://localhost/balanza" -ForegroundColor Cyan
Write-Host ""
Write-Host "Para ver logs: Get-EventLog -LogName Application -Source BalanzaService -Newest 20" -ForegroundColor Gray

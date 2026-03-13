<#
.SYNOPSIS
    Publica BalanzaService como ejecutable standalone

.DESCRIPTION
    Compila y publica el proyecto como un ejecutable self-contained
    para Windows x64

.PARAMETER Configuration
    Configuracion de build (default: Release)

.PARAMETER OutputPath
    Ruta de salida (default: .\publish)

.EXAMPLE
    .\Publish.ps1

.EXAMPLE
    .\Publish.ps1 -Configuration Debug -OutputPath .\output
#>

param(
    [string]$Configuration = "Release",
    [string]$OutputPath = ".\publish"
)

$ErrorActionPreference = "Stop"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Publicando BalanzaService" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

$projectPath = "..\BalanzaService\BalanzaService.csproj"

if (-not (Test-Path $projectPath)) {
    $projectPath = ".\BalanzaService\BalanzaService.csproj"
}

if (-not (Test-Path $projectPath)) {
    Write-Host "ERROR: No se encuentra el proyecto. Ejecute desde el directorio raiz." -ForegroundColor Red
    exit 1
}

Write-Host "Proyecto: $projectPath" -ForegroundColor Gray
Write-Host "Configuracion: $Configuration" -ForegroundColor Gray
Write-Host "Salida: $OutputPath" -ForegroundColor Gray
Write-Host ""

# Limpiar directorio de salida
if (Test-Path $OutputPath) {
    Write-Host "Limpiando directorio de salida..." -ForegroundColor Yellow
    Remove-Item -Path $OutputPath -Recurse -Force
}

# Publicar
Write-Host "Compilando y publicando..." -ForegroundColor Green
dotnet publish $projectPath `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -o $OutputPath

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Fallo la publicacion." -ForegroundColor Red
    exit 1
}

# Copiar appsettings
$appsettingsSource = "..\BalanzaService\appsettings.json"
if (-not (Test-Path $appsettingsSource)) {
    $appsettingsSource = ".\BalanzaService\appsettings.json"
}

if (Test-Path $appsettingsSource) {
    Copy-Item -Path $appsettingsSource -Destination $OutputPath -Force
    Write-Host "appsettings.json copiado" -ForegroundColor Gray
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "  Publicacion completada!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""
Write-Host "Archivos en: $OutputPath" -ForegroundColor Cyan
Write-Host ""

# Mostrar archivos generados
Get-ChildItem $OutputPath | Format-Table Name, Length -AutoSize

<#
.SYNOPSIS
    Prueba el servicio BalanzaService

.DESCRIPTION
    Verifica el estado del servicio y realiza una llamada al endpoint /balanza

.PARAMETER ServiceName
    Nombre del servicio (default: BalanzaService)

.PARAMETER Port
    Puerto del servicio (default: 80)

.EXAMPLE
    .\Test-Service.ps1
#>

param(
    [string]$ServiceName = "BalanzaService",
    [int]$Port = 80
)

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Test de BalanzaService" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Verificar estado del servicio
Write-Host "Estado del servicio:" -ForegroundColor Yellow
$service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($service) {
    Write-Host "  Nombre: $($service.Name)" -ForegroundColor White
    Write-Host "  Estado: $($service.Status)" -ForegroundColor $(if ($service.Status -eq 'Running') { 'Green' } else { 'Red' })
    Write-Host "  Tipo inicio: $($service.StartType)" -ForegroundColor White
} else {
    Write-Host "  Servicio no encontrado!" -ForegroundColor Red
}

Write-Host ""

# Probar endpoint
Write-Host "Probando endpoint http://localhost:$Port/balanza ..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "http://localhost:$Port/balanza" -Method Get -TimeoutSec 5
    Write-Host "  Respuesta: $($response | ConvertTo-Json -Compress)" -ForegroundColor Green
} catch {
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Probar health
Write-Host "Probando endpoint http://localhost:$Port/health ..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:$Port/health" -Method Get -TimeoutSec 5
    Write-Host "  Status: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "  Body: $($response.Content)" -ForegroundColor Green
} catch {
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Mostrar ultimos logs
Write-Host "Ultimos logs del servicio:" -ForegroundColor Yellow
try {
    $logs = Get-EventLog -LogName Application -Source $ServiceName -Newest 5 -ErrorAction SilentlyContinue
    if ($logs) {
        $logs | ForEach-Object {
            $color = if ($_.EntryType -eq 'Error') { 'Red' } elseif ($_.EntryType -eq 'Warning') { 'Yellow' } else { 'Gray' }
            Write-Host "  [$($_.TimeGenerated.ToString('HH:mm:ss'))] $($_.Message)" -ForegroundColor $color
        }
    } else {
        Write-Host "  No se encontraron logs." -ForegroundColor Gray
    }
} catch {
    Write-Host "  No se pudieron obtener los logs." -ForegroundColor Gray
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan

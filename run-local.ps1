# ==============================================================================
# VetClinic Pro - Script de Ejecución Monopuesto Local (RNF-07, RNF-08)
# Dres. Fabio y William - Clínica Veterinaria
# ==============================================================================

Write-Host "==========================================================" -ForegroundColor Green
Write-Host " Iniciando VetClinic Pro (Estación Monopuesto Localhost) " -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green

# Configurar SDK local si es requerido
if ($env:LOCALAPPDATA -and (Test-Path "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe")) {
    $env:DOTNET_ROOT = "$env:LOCALAPPDATA\Microsoft\dotnet"
    $env:PATH = "$env:DOTNET_ROOT;$env:PATH"
}

# 1. Compilar SPA React si no existe dist o se solicita rebuild
if (!(Test-Path "VetClinic.Web\dist\index.html")) {
    Write-Host "`n[1/3] Compilando frontend web (React + Vite)..." -ForegroundColor Cyan
    Push-Location "VetClinic.Web"
    npm run build
    Pop-Location
}

# 2. Sincronizar archivos estáticos con wwwroot de la API
Write-Host "`n[2/3] Sincronizando interfaz web en Kestrel (wwwroot)..." -ForegroundColor Cyan
if (!(Test-Path "VetClinic.Api\wwwroot")) {
    New-Item -ItemType Directory -Path "VetClinic.Api\wwwroot" -Force | Out-Null
}
Copy-Item -Path "VetClinic.Web\dist\*" -Destination "VetClinic.Api\wwwroot" -Recurse -Force

# 3. Iniciar servidor Kestrel local
Write-Host "`n[3/3] Iniciando servidor Kestrel en http://localhost:5000 ..." -ForegroundColor Cyan
Write-Host "-> Abre tu navegador en: http://localhost:5000" -ForegroundColor Yellow
Write-Host "-> Documentación API:   http://localhost:5000/swagger" -ForegroundColor Yellow
Write-Host "-> Presiona Ctrl+C para detener el servidor.`n" -ForegroundColor Gray

dotnet run --project VetClinic.Api

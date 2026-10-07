# ==============================================================================
# VetClinic Pro - Script de Ejecución Simultánea (Web + Escritorio)
# Dres. Fabio y William - Clínica Veterinaria
# ==============================================================================

Write-Host "==========================================================" -ForegroundColor Green
Write-Host " Iniciando VetClinic Pro (Web + Escritorio WPF)          " -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green

# Configurar SDK local si es requerido
if ($env:LOCALAPPDATA -and (Test-Path "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe")) {
    $env:DOTNET_ROOT = "$env:LOCALAPPDATA\Microsoft\dotnet"
    $env:PATH = "$env:DOTNET_ROOT;$env:PATH"
}

# 1. Sincronizar archivos estáticos con wwwroot de la API
if (Test-Path "VetClinic.Web\dist") {
    if (!(Test-Path "VetClinic.Api\wwwroot")) {
        New-Item -ItemType Directory -Path "VetClinic.Api\wwwroot" -Force | Out-Null
    }
    Copy-Item -Path "VetClinic.Web\dist\*" -Destination "VetClinic.Api\wwwroot" -Recurse -Force
}

# 2. Iniciar Servidor Web Kestrel en segundo plano
Write-Host "`n[1/2] Iniciando Servidor Web en http://localhost:5000 ..." -ForegroundColor Cyan
$webProcess = Start-Process -FilePath "dotnet" -ArgumentList "run --project VetClinic.Api --urls http://localhost:5000" -PassThru
Start-Sleep -Seconds 2
Start-Process "http://localhost:5000"

# 3. Iniciar Aplicación de Escritorio WPF
Write-Host "`n[2/2] Iniciando Aplicación de Escritorio WPF..." -ForegroundColor Cyan
Write-Host "-> Ambas interfaces comparten la misma base de datos SQLite local en tiempo real.`n" -ForegroundColor Yellow

try {
    dotnet run --project VetClinic.Presentation
}
finally {
    if ($webProcess -and !$webProcess.HasExited) {
        Write-Host "`nCerrando servidor web de respaldo..." -ForegroundColor Gray
        Stop-Process -Id $webProcess.Id -Force -ErrorAction SilentlyContinue
    }
}

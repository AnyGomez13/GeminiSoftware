# ==============================================================================
# VetClinic Pro - Script de Ejecución de Escritorio WPF (RNF-07, RNF-08)
# Dres. Fabio y William - Clínica Veterinaria
# ==============================================================================

Write-Host "==========================================================" -ForegroundColor Green
Write-Host " Iniciando VetClinic Pro (Aplicación Escritorio WPF)      " -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green

# Configurar SDK local si es requerido
if ($env:LOCALAPPDATA -and (Test-Path "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe")) {
    $env:DOTNET_ROOT = "$env:LOCALAPPDATA\Microsoft\dotnet"
    $env:PATH = "$env:DOTNET_ROOT;$env:PATH"
}

Write-Host "`nIniciando aplicación de escritorio WPF..." -ForegroundColor Cyan
dotnet run --project VetClinic.Presentation

Write-Host "============================================" -ForegroundColor Cyan
Write-Host " Executando Testes do Backend (xUnit + Moq)" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan

# O SDK 8.0.423 esta no dotnet do perfil do usuario
$dotnet = "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe"

if (-not (Test-Path $dotnet)) {
    Write-Host "ERRO: .NET SDK nao encontrado em $dotnet" -ForegroundColor Red
    Write-Host "Baixe em: https://aka.ms/dotnet/download" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "SDK encontrado:" -ForegroundColor Green
& $dotnet --list-sdks

# ⚠️ Correto: pasta do projeto de TESTES
$testProject = "$PSScriptRoot\Cinema.Api.Tests"

Write-Host ""
Write-Host "Restaurando pacotes..." -ForegroundColor Yellow
Push-Location $testProject
& $dotnet restore --verbosity quiet

Write-Host ""
Write-Host "Executando testes..." -ForegroundColor Yellow
& $dotnet test --no-restore --verbosity normal
Pop-Location

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host " Testes concluidos." -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan

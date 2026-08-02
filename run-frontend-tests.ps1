Write-Host "============================================" -ForegroundColor Cyan
Write-Host " Executando Testes do Frontend (Jasmine/Karma)" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan

$projectDir = "C:\Users\mathe\Desktop\desafio\cinema-app"
Push-Location $projectDir

# Instala dependencias se necessario
if (-not (Test-Path "node_modules")) {
    Write-Host ""
    Write-Host "Instalando dependencias (npm install)..." -ForegroundColor Yellow
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[ERRO] Falha no npm install." -ForegroundColor Red
        Pop-Location
        Read-Host "Pressione Enter para sair"
        exit 1
    }
}

# Encontra o Angular CLI local
$ngCmd = $null
if (Test-Path "node_modules\.bin\ng.cmd") {
    $ngCmd = ".\node_modules\.bin\ng.cmd"
} elseif (Test-Path "node_modules\.bin\ng") {
    $ngCmd = ".\node_modules\.bin\ng"
} else {
    Write-Host "[ERRO] Angular CLI nao encontrado em node_modules." -ForegroundColor Red
    Pop-Location
    Read-Host "Pressione Enter para sair"
    exit 1
}

Write-Host ""
Write-Host "Executando testes..." -ForegroundColor Yellow
& cmd.exe /c $ngCmd test --watch=false --browsers=ChromeHeadless

Pop-Location
Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host " Testes concluidos." -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Read-Host "Pressione Enter para sair"

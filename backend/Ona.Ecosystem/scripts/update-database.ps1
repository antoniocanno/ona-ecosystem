# Script para aplicar migrations no banco de dados
# Uso: .\scripts\update-database.ps1

$projectPath = "./src/Identity/Ona.Auth.Infrastructure/Ona.Auth.Infrastructure.csproj"
$startupProjectPath = "./src/Identity/Ona.Auth.API/Ona.Auth.API.csproj"

Write-Host "Aplicando migrations no banco de dados..." -ForegroundColor Cyan
Write-Host ""

# Verifica se os projetos existem
if (-not (Test-Path $projectPath)) {
    Write-Host "ERRO: Projeto nao encontrado em $projectPath" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $startupProjectPath)) {
    Write-Host "ERRO: Startup project nao encontrado em $startupProjectPath" -ForegroundColor Red
    exit 1
}

# Aplica as migrations
Write-Host "Executando: dotnet ef database update ..." -ForegroundColor Yellow
dotnet ef database update `
    --project $projectPath `
    --startup-project $startupProjectPath `
    --context AuthDbContext

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "SUCCESS: Migrations aplicadas com sucesso!" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "ERRO: Erro ao aplicar migrations. Verifique os erros acima." -ForegroundColor Red
    Write-Host "DICA: Certifique-se de que:" -ForegroundColor Yellow
    Write-Host "   - O PostgreSQL esta rodando" -ForegroundColor White
    Write-Host "   - A connection string esta configurada corretamente no appsettings.Development.json" -ForegroundColor White
    exit 1
}


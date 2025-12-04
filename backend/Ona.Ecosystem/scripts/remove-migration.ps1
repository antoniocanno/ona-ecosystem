# Script para remover a última migration criada
# Uso: .\scripts\remove-migration.ps1

$projectPath = "./src/Identity/Ona.Auth.Infrastructure/Ona.Auth.Infrastructure.csproj"
$startupProjectPath = "./src/Identity/Ona.Auth.API/Ona.Auth.API.csproj"

Write-Host "Removendo a ultima migration..." -ForegroundColor Yellow
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

# Remove a última migration
Write-Host "Executando: dotnet ef migrations remove ..." -ForegroundColor Yellow
dotnet ef migrations remove `
    --project $projectPath `
    --startup-project $startupProjectPath `
    --context AuthDbContext

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "SUCCESS: Migration removida com sucesso!" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "ERRO: Erro ao remover migration. Verifique os erros acima." -ForegroundColor Red
    exit 1
}


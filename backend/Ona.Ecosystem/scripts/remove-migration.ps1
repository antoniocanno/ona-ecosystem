# Script para remover a última migration criada
# Uso: .\scripts\remove-migration.ps1 -Project "auth|commit"

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("auth", "commit")]
    [string]$Project = "auth"
)

$projectPath = ""
$startupProjectPath = ""
$context = ""

if ($Project -eq "auth") {
    $projectPath = "./src/Identity/Ona.Auth.Infrastructure/Ona.Auth.Infrastructure.csproj"
    $startupProjectPath = "./src/Identity/Ona.Auth.API/Ona.Auth.API.csproj"
    $context = "AuthDbContext"
} else {
    $projectPath = "./src/Apps/Commit/Ona.Commit.Infrastructure/Ona.Commit.Infrastructure.csproj"
    $startupProjectPath = "./src/Apps/Commit/Ona.Commit.API/Ona.Commit.API.csproj"
    $context = "CommitDbContext"
}

Write-Host "Projeto selecionado: $Project" -ForegroundColor Cyan
Write-Host "Removendo a ultima migration do contexto $context..." -ForegroundColor Yellow
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
    --context $context

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "SUCCESS: Migration removida com sucesso!" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "ERRO: Erro ao remover migration. Verifique os erros acima." -ForegroundColor Red
    exit 1
}

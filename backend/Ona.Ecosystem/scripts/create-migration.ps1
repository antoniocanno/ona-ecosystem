# Script para criar uma nova migration do Entity Framework
# Uso: .\scripts\create-migration.ps1 -Name "NomeDaMigration" -Project "auth|commit"

param(
    [Parameter(Mandatory=$true)]
    [string]$Name,
    
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
Write-Host "Criando migration: $Name para o contexto $context" -ForegroundColor Cyan
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

# Cria a migration
Write-Host "Executando: dotnet ef migrations add $Name ..." -ForegroundColor Yellow
dotnet ef migrations add $Name `
    --project $projectPath `
    --startup-project $startupProjectPath `
    --context $context

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "SUCCESS: Migration criada com sucesso!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Proximos passos:" -ForegroundColor Cyan
    Write-Host "   1. A migration sera aplicada automaticamente quando voce iniciar a aplicacao em modo Development" -ForegroundColor White
    Write-Host "   2. Ou execute: .\scripts\update-database.ps1 -Project $Project" -ForegroundColor White
} else {
    Write-Host ""
    Write-Host "ERRO: Erro ao criar migration. Verifique os erros acima." -ForegroundColor Red
    exit 1
}

# Scripts de Migrations - Entity Framework

Este diretório contém scripts auxiliares para gerenciar migrations do Entity Framework Core.

## 🚀 Sistema Automatizado de Migrations

O projeto está configurado para **aplicar migrations automaticamente** quando a aplicação é iniciada em modo `Development`. Isso significa que você **não precisa** executar `dotnet ef database update` manualmente ao iniciar a aplicação.

### Como Funciona

1. **Ao iniciar a aplicação** (via AppHost ou diretamente):
   - A aplicação verifica se há migrations pendentes
   - Se houver, aplica automaticamente
   - Logs informativos são exibidos no console

2. **Para criar novas migrations**, você ainda precisa executar o comando manualmente (veja abaixo)

## 📝 Scripts Disponíveis

### 1. Criar Migration

Cria uma nova migration com o nome especificado.

```powershell
.\scripts\create-migration.ps1 -Name "NomeDaMigration"
```

**Exemplo:**
```powershell
.\scripts\create-migration.ps1 -Name "AddUserProfileTable"
```

### 2. Aplicar Migrations

Aplica todas as migrations pendentes no banco de dados.

```powershell
.\scripts\update-database.ps1
```

> **Nota:** Normalmente não é necessário executar este script, pois as migrations são aplicadas automaticamente ao iniciar a aplicação em modo Development.

### 3. Remover Última Migration

Remove a última migration criada (útil se você cometeu um erro).

```powershell
.\scripts\remove-migration.ps1
```

## 🔧 Fluxo de Trabalho Recomendado

### Desenvolvimento Local (com .NET Aspire)

1. **Criar uma nova migration:**
   ```powershell
   .\scripts\create-migration.ps1 -Name "SuaMigration"
   ```

2. **Iniciar o AppHost:**
   ```powershell
   dotnet run --project src/Orchestration/Ona.AppHost/Ona.AppHost.csproj
   ```
   
   As migrations serão aplicadas automaticamente quando a API iniciar.

### Desenvolvimento Local (sem Aspire)

1. **Criar uma nova migration:**
   ```powershell
   .\scripts\create-migration.ps1 -Name "SuaMigration"
   ```

2. **Aplicar migrations manualmente (se necessário):**
   ```powershell
   .\scripts\update-database.ps1
   ```

3. **Ou simplesmente iniciar a aplicação** - as migrations serão aplicadas automaticamente em modo Development.

## ⚙️ Configuração

### Connection String

A connection string deve estar configurada em:
- `src/Identity/Ona.Auth.API/appsettings.Development.json`

Exemplo:
```json
{
  "ConnectionStrings": {
    "auth-db": "Host=localhost;Port=5432;Database=auth-db;Username=postgres;Password=postgres"
  }
}
```

### Com .NET Aspire

Quando você executa o AppHost, a connection string é configurada automaticamente via service discovery. Você não precisa configurar manualmente.

## 📋 Comandos Manuais (Alternativa aos Scripts)

Se preferir usar os comandos diretamente:

### Criar Migration
```powershell
dotnet ef migrations add NomeDaMigration `
  --project ./src/Identity/Ona.Auth.Infrastructure/Ona.Auth.Infrastructure.csproj `
  --startup-project ./src/Identity/Ona.Auth.API/Ona.Auth.API.csproj
```

### Aplicar Migrations
```powershell
dotnet ef database update `
  --project ./src/Identity/Ona.Auth.Infrastructure/Ona.Auth.Infrastructure.csproj `
  --startup-project ./src/Identity/Ona.Auth.API/Ona.Auth.API.csproj
```

### Remover Migration
```powershell
dotnet ef migrations remove `
  --project ./src/Identity/Ona.Auth.Infrastructure/Ona.Auth.Infrastructure.csproj `
  --startup-project ./src/Identity/Ona.Auth.API/Ona.Auth.API.csproj
```

## 🐛 Troubleshooting

### Erro: "Host can't be null"

- Verifique se a connection string está configurada corretamente no `appsettings.Development.json`
- Certifique-se de que o PostgreSQL está rodando
- Se estiver usando Aspire, certifique-se de que o AppHost está configurado corretamente

### Migrations não são aplicadas automaticamente

- Verifique se está rodando em modo `Development` (variável `ASPNETCORE_ENVIRONMENT=Development`)
- Verifique os logs da aplicação para ver se há erros
- Certifique-se de que o banco de dados está acessível

## 📚 Mais Informações

- [Entity Framework Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)


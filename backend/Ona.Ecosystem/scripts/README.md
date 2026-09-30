# Scripts de Migrations - Entity Framework

Este diretório contém scripts auxiliares para gerenciar migrations do Entity Framework Core para os projetos do ecossistema Ona.

## 🚀 Sistema Automatizado de Migrations

O projeto aplica migrations automaticamente através do recurso **`migrations`** (`Ona.MigrationService`), registrado no AppHost. Ele roda **antes** das APIs e do worker Hangfire (`WaitForCompletion`) e aplica as migrations de `AuthDbContext` e `CommitDbContext`. Isso significa que você **não precisa** executar `dotnet ef database update` manualmente.

### Como Funciona

1. **Ao iniciar o AppHost:**
   - O Aspire sobe o PostgreSQL e espera ele ficar saudável
   - O recurso `migrations` conecta, verifica migrations pendentes e aplica (`AuthDbContext`, depois `CommitDbContext`)
   - Ao terminar, ele encerra; só então as APIs e o worker iniciam
   - Logs informativos são exibidos no console do recurso `migrations`

2. **Para criar novas migrations**, execute o comando manualmente usando os scripts abaixo.

## 📝 Scripts Disponíveis

Todos os scripts aceitam o parâmetro `-Project` para definir qual contexto de banco de dados você deseja manipular:
- `auth` (Padrão): Gerencia o Identity e Controle de Acesso.
- `commit`: Gerencia o App Commit (Agendamentos e Clientes).

### 1. Criar Migration

Cria uma nova migration com o nome especificado para o projeto selecionado.

```powershell
.\scripts\create-migration.ps1 -Name "NomeDaMigration" -Project "auth|commit"
```

**Exemplo para o Identity (Auth):**
```powershell
.\scripts\create-migration.ps1 -Name "AddUserProfileTable"
```

**Exemplo para o Commit:**
```powershell
.\scripts\create-migration.ps1 -Name "AddAppointmentTable" -Project commit
```

### 2. Aplicar Migrations

Aplica todas as migrations pendentes no banco de dados para o projeto selecionado.

```powershell
.\scripts\update-database.ps1 -Project "auth|commit"
```

> **Nota:** Normalmente não é necessário executar este script, pois o recurso `migrations` aplica tudo automaticamente ao subir o AppHost. Use-o apenas para aplicar migrations fora do Aspire (ex.: banco local).

### 3. Remover Última Migration

Remove a última migration criada (útil se você cometeu um erro).

```powershell
.\scripts\remove-migration.ps1 -Project "auth|commit"
```

## 🔧 Fluxo de Trabalho Recomendado

### Desenvolvimento Local

1. **Criar uma nova migration:**
   ```powershell
   .\scripts\create-migration.ps1 -Name "SuaMigration" -Project commit
   ```

2. **Aplicar migrations:**
   - Inicie o **AppHost** — o recurso `migrations` aplica automaticamente:
     ```powershell
     dotnet run --project src/Orchestration/Ona.AppHost/Ona.AppHost.csproj
     ```
   - Ou aplique manualmente:
     ```powershell
     .\scripts\update-database.ps1 -Project commit
     ```

## ⚙️ Configuração

### Connection Strings

Rodando via Aspire, as connection strings são injetadas automaticamente (`auth-db`, `commit-db`) — nenhum JSON precisa ser configurado.

Para rodar os comandos `dotnet ef` fora do Aspire, os projetos de Infrastructure possuem **design-time factories** com uma connection string local padrão:
- **Auth:** `AuthDbContextFactory` → `Host=localhost;Database=OnaAuth;...`
- **Commit:** `CommitDbContextFactory` → `Host=localhost;Database=OnaCommit;...`

Ajuste essas factories ou use `.\scripts\update-database.ps1` se precisar apontar para outro banco.

## 📋 Projetos Suportados

| Projeto | Nome do Contexto | Caminho da Infraestrutura |
| :--- | :--- | :--- |
| **Auth** | `AuthDbContext` | `src/Identity/Ona.Auth.Infrastructure` |
| **Commit** | `CommitDbContext` | `src/Apps/Commit/Ona.Commit.Infrastructure` |

## 🐛 Troubleshooting

### Erro: `dotnet ef` não encontrado

- Instale a ferramenta: `dotnet tool install --global dotnet-ef --version 8.0.22`

### Erro: "Host can't be null"

- Verifique a connection string da design-time factory do projeto de Infrastructure correspondente.
- Certifique-se de que o PostgreSQL está rodando.

### Migrations não são aplicadas automaticamente

- Verifique os logs do recurso `migrations` no dashboard do Aspire.
- Confirme que as APIs/worker têm `.WaitForCompletion(migrations)` no `AppHost.cs`.
- Confirme que os projetos de Infrastructure possuem migrations (`Migrations/`).

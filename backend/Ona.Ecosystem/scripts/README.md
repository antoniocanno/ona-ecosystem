# Scripts de Migrations - Entity Framework

Este diretório contém scripts auxiliares para gerenciar migrations do Entity Framework Core para os projetos do ecossistema Ona.

## 🚀 Sistema Automatizado de Migrations

O projeto está configurado para **aplicar migrations automaticamente** quando a aplicação é iniciada em modo `Development`. Isso significa que você **não precisa** executar `dotnet ef database update` manualmente ao iniciar a aplicação.

### Como Funciona

1. **Ao iniciar a aplicação** (via AppHost ou diretamente):
   - A aplicação verifica se há migrations pendentes
   - Se houver, aplica automaticamente
   - Logs informativos são exibidos no console

2. **Para criar novas migrations**, você ainda precisa executar o comando manualmente usando os scripts abaixo.

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

> **Nota:** Normalmente não é necessário executar este script no Identity, pois as migrations são aplicadas automaticamente ao iniciar a aplicação em modo Development.

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
   - Inicie o **AppHost** para aplicação automática:
     ```powershell
     dotnet run --project src/Orchestration/Ona.AppHost/Ona.AppHost.csproj
     ```
   - Ou aplique manualmente:
     ```powershell
     .\scripts\update-database.ps1 -Project commit
     ```

## ⚙️ Configuração

### Connection Strings

As connection strings devem estar configuradas nos respectivos projetos de API:
- **Auth:** `src/Identity/Ona.Auth.API/appsettings.Development.json`
- **Commit:** `src/Apps/Commit/Ona.Commit.API/appsettings.Development.json`

### Com .NET Aspire

Quando você executa o AppHost, as connection strings são injetadas automaticamente. Você não precisa configurar manualmente os arquivos JSON se estiver rodando via Aspire.

## 📋 Projetos Suportados

| Projeto | Nome do Contexto | Caminho da Infraestrutura |
| :--- | :--- | :--- |
| **Auth** | `AuthDbContext` | `src/Identity/Ona.Auth.Infrastructure` |
| **Commit** | `CommitDbContext` | `src/Apps/Commit/Ona.Commit.Infrastructure` |

## 🐛 Troubleshooting

### Erro: "Host can't be null"

- Verifique se a connection string está configurada corretamente no JSON do projeto de API correspondente.
- Certifique-se de que o PostgreSQL está rodando.

### Migrations não são aplicadas automaticamente

- Verifique se está rodando em modo `Development`.
- Verifique se o `AddServiceDefaults()` e `app.ApplyDatabaseMigrationsAsync()` (ou similar) estão presentes no `Program.cs` do seu projeto.

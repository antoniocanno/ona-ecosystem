# Ona Ecosystem

Uma plataforma modular de microserviços construída em .NET 8 + Aspire, projetada para crescer app por app.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Aspire](https://img.shields.io/badge/Aspire-9.3-0078D4?style=for-the-badge&logo=microsoft&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)
![Redis](https://img.shields.io/badge/Redis-DC382D?style=for-the-badge&logo=redis&logoColor=white)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-FF6600?style=for-the-badge&logo=rabbitmq&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)

---

## Sumário

- [Visão Geral](#visão-geral)
- [Arquitetura](#arquitetura)
- [Stack](#stack)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Apps](#apps)
  - [Identity](#identity)
  - [Commit](#commit)
- [Pré-requisitos](#pré-requisitos)
- [Como Rodar](#como-rodar)
- [Scripts de Migration](#scripts-de-migration)
- [CI/CD](#cicd)
- [Roadmap](#roadmap)
- [Licença](#licença)

---

## Visão Geral

O Ona Ecosystem é um conjunto de APIs independentes orquestradas pelo .NET Aspire. A ideia é simples: cada app resolve um problema de negócio específico e compartilha uma base comum de identidade, infraestrutura e padrões. Novos apps entram sem mexer no que já existe.

A arquitetura segue Clean Architecture em todos os domínios (Domain / Application / Infrastructure / API), com autenticação centralizada via JWT multi-tenant e toda a infraestrutura (bancos, cache, filas, containers) gerenciada pelo Aspire.

---

## Arquitetura

```
┌─────────────────────────────────────────────────────────────┐
│                    .NET Aspire (AppHost)                    │
│           Orquestração · Observabilidade · Config           │
├──────────────┬──────────────────────┬───────────────────────┤
│   Identity   │        Commit        │    Futuro App ...     │
│  (Auth API)  │    (API + Worker)    │                       │
├──────────────┴──────────────────────┴───────────────────────┤
│                       Shared Libraries                      │
│             Ona.Core · Ona.Application.Shared               │
│                 Ona.Infrastructure.Shared                   │
├─────────────────────────────────────────────────────────────┤
│              Infraestrutura Containerizada                  │
│     PostgreSQL  ·  Redis  ·  RabbitMQ  ·  Evolution API     │
└─────────────────────────────────────────────────────────────┘
```

Cada domínio é dividido em quatro projetos:

```
App/
├── Domain           →  Entidades, Enums, Interfaces, Regras de Negócio
├── Application      →  Use Cases, DTOs, Mapeamentos (Mapster)
├── Infrastructure   →  EF Core, Repositórios, Integrações Externas
└── API              →  Controllers, Middlewares, Configuração
```

---

## Stack

**Core:** .NET 8, .NET Aspire 9.3, ASP.NET Core, Entity Framework Core 8

**Infraestrutura:** PostgreSQL (um banco lógico por contexto), Redis, RabbitMQ via MassTransit, Docker

**Bibliotecas:** Hangfire, Mapster, BCrypt.NET, JWT Bearer, RazorLight, Swashbuckle

**Integrações:** Google Calendar API, Microsoft Graph, Evolution API (WhatsApp)

---

## Estrutura do Projeto

```
Ona/
├── backend/
│   └── Ona.Ecosystem/
│       ├── Ona.Ecosystem.sln
│       ├── scripts/            # Scripts de migrations (PowerShell)
│       └── src/
│           ├── Orchestration/       # AppHost + ServiceDefaults (Aspire)
│           ├── Identity/
│           │   ├── Ona.Auth.API
│           │   ├── Ona.Auth.Application
│           │   ├── Ona.Auth.Domain
│           │   └── Ona.Auth.Infrastructure
│           ├── Apps/
│           │   └── Commit/
│           │       ├── Ona.Commit.API
│           │       ├── Ona.Commit.Application
│           │       ├── Ona.Commit.Domain
│           │       ├── Ona.Commit.Infrastructure
│           │       └── Ona.Commit.Worker.Hangfire
│           └── Shared/
│               ├── Ona.Core
│               ├── Ona.Application.Shared
│               └── Ona.Infrastructure.Shared
└── frontend/                   # Reservado para implementação futura
```

---

## Apps

### Identity

Camada de autenticação e autorização centralizada para todo o ecossistema. Funciona como gateway de identidade: nenhum app implementa auth próprio.

Suporta JWT (access token + refresh token), multi-tenant com roles e sistema de convites, ciclo de vida completo do usuário (registro, verificação de e-mail, reset de senha, desbloqueio), cache de sessão via Redis e comunicação assíncrona via RabbitMQ/MassTransit.

---

### Commit

Primeiro app de domínio do ecossistema. O problema que resolve é direto: eliminar no-show em compromissos através de lembretes via WhatsApp e sincronização com calendários externos.

Inclui sincronização bidirecional com Google Calendar e Microsoft Outlook, envio de confirmações via WhatsApp (Evolution API) e um worker Hangfire dedicado que cuida do agendamento de lembretes, sincronização periódica de calendários e refresh de tokens OAuth. Dados sensíveis (tokens OAuth, credenciais) são criptografados em repouso.

---

## Pré-requisitos

- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (última estável)
- .NET Aspire Workload 9.3+

---

## Como Rodar

**1. Clone o repositório**

```bash
git clone https://github.com/tonicocanno/ona-ecosystem.git
cd ona-ecosystem
```

**2. Configure os secrets**

O projeto usa User Secrets do .NET. Configure no AppHost:

```bash
cd backend/Ona.Ecosystem
dotnet user-secrets set "Parameters:JwtSecret" "sua-chave-jwt-segura" --project src/Orchestration/Ona.AppHost
dotnet user-secrets set "Parameters:JwtIssuer" "Ona" --project src/Orchestration/Ona.AppHost
dotnet user-secrets set "Parameters:JwtAudience" "OnaUsers" --project src/Orchestration/Ona.AppHost
dotnet user-secrets set "Parameters:InternalApiKey" "sua-api-key-interna" --project src/Orchestration/Ona.AppHost
dotnet user-secrets set "Parameters:pg-password" "sua-senha-postgres" --project src/Orchestration/Ona.AppHost
dotnet user-secrets set "Parameters:CryptographyKey" "sua-chave-criptografia" --project src/Orchestration/Ona.AppHost
```

**3. Suba o ecossistema**

```bash
dotnet run --project src/Orchestration/Ona.AppHost/Ona.AppHost.csproj
```

O Aspire vai provisionar PostgreSQL (`auth-db`, `commit-db`, `evolution-db`), Redis, RabbitMQ e o container da Evolution API, depois iniciar as APIs e o worker Hangfire. O dashboard de observabilidade abre automaticamente — a URL aparece no terminal (geralmente `https://localhost:17225`).

> Migrations do EF Core são aplicadas automaticamente em modo `Development`.

---

## Scripts de Migration

Scripts PowerShell para gerenciamento de migrations:

```powershell
# Criar uma nova migration
.\scripts\create-migration.ps1 -Name "NomeDaMigration" -Project "auth|commit"

# Aplicar migrations pendentes
.\scripts\update-database.ps1 -Project "auth|commit"

# Remover a última migration
.\scripts\remove-migration.ps1 -Project "auth|commit"
```

Veja o [README de Scripts](backend/Ona.Ecosystem/scripts/README.md) para mais detalhes.

---

## CI/CD

Pipeline GitHub Actions executado a cada push ou pull request na `main`: checkout, setup .NET 8, restore, build e testes.

---

## Roadmap

- [x] Identity — autenticação, autorização e multi-tenancy
- [x] Commit — prevenção de no-show com lembretes via WhatsApp e integração com calendários
- [ ] Frontend — interface web para as APIs do ecossistema
- [ ] Novos apps de domínio
# Backend — ASP.NET Core .NET 10 (Minimal API na AWS Lambda)

```
src/
  Portfolio.Domain              entidades (sem dependências)
  Portfolio.Application         contratos (requests) e abstrações (IMediaStorage, IPublishTrigger)
  Portfolio.Infrastructure      EF Core + Npgsql (Aurora Serverless v2), S3 (imagens e snapshot)
  Portfolio.Api                 endpoints /api/admin — roda como Lambda atrás do API Gateway (HTTP API)
  Portfolio.PublishDispatcher   Lambda acionada pelo S3 (snapshot.json) que dispara o GitHub Actions
tests/
  Portfolio.UnitTests / Portfolio.IntegrationTests
```

## Como funciona na AWS

- **API:** Lambda `dotnet10` (arm64) dentro da VPC, sem NAT Gateway. Acesso ao S3 por VPC endpoint (gateway, gratuito).
- **Auth:** o JWT authorizer do API Gateway valida o access token do Cognito; a API exige o grupo `admin` (`cognito:groups`).
- **Banco:** Aurora Serverless v2 PostgreSQL com mínimo de 0 ACU (pausa quando ocioso; a primeira requisição depois da pausa leva ~15 s).
- **Publicar:** `POST /api/admin/publish` grava `snapshot.json` no bucket privado → notificação do S3 → `PublishDispatcher` (fora da VPC) → `repository_dispatch` no GitHub.
- **Migrations:** o Aurora fica em subnet privada; o deploy liga `Database__MigrateOnStartup=true` e a Lambda aplica migrations pendentes ao iniciar.

## Rodando localmente

```bash
docker compose up -d                       # Postgres local
dotnet tool install --global dotnet-ef     # uma vez
dotnet ef migrations add Inicial -p src/Portfolio.Infrastructure -s src/Portfolio.Api -o Persistence/Migrations
dotnet ef database update -p src/Portfolio.Infrastructure -s src/Portfolio.Api
dotnet run --project src/Portfolio.Api     # http://localhost:5080  (OpenAPI em /openapi/v1.json)
dotnet test
```

Em desenvolvimento, `Auth:DevBypass=true` libera as rotas `/api/admin` sem login.
Upload de imagens e Publicar usam o S3 de verdade: configure um perfil AWS local (`aws configure`) e os buckets de dev em `appsettings.Development.json`.

### Testando os endpoints

Coleções do Insomnia com todos os endpoints (Insomnia → Import → arquivo):

- `docs/insomnia-develop.json`: aponta para `http://localhost:5080`; com `Auth:DevBypass=true` não precisa de token.
- `docs/insomnia-prod.json`: aponta para o API Gateway; preencha `base_url` e `token` (access token do Cognito) no ambiente.

Os IDs usados nas rotas (`category_id`, `project_id`, `media_id` etc.) ficam no ambiente de cada coleção.
Sempre que algo mudar na API (rotas, métodos, contratos, validações, autenticação), atualize as duas coleções na mesma alteração.

## Deploy

Feito pelo GitHub Actions (`.github/workflows/deploy-api.yml`) com OIDC — sem chaves AWS no repositório.

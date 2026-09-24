# Backend — ASP.NET Core .NET 10 (Minimal API)

```
src/
  Portfolio.Domain          entidades (sem dependências)
  Portfolio.Application     contratos (requests) e abstrações (IMediaStorage, IPublishTrigger)
  Portfolio.Infrastructure  EF Core + Npgsql (Neon), R2 (S3), gatilho do GitHub Actions
  Portfolio.Api             endpoints /api/admin, auth Cloudflare Access, FluentValidation
tests/
  Portfolio.UnitTests / Portfolio.IntegrationTests
```

## Rodando localmente

```bash
docker compose up -d                       # Postgres local
dotnet tool install --global dotnet-ef     # uma vez
dotnet ef migrations add Inicial -p src/Portfolio.Infrastructure -s src/Portfolio.Api -o Persistence/Migrations
dotnet ef database update -p src/Portfolio.Infrastructure -s src/Portfolio.Api
dotnet run --project src/Portfolio.Api     # http://localhost:5080  (OpenAPI em /openapi/v1.json)
dotnet test
```

Em desenvolvimento, sem `CloudflareAccess:TeamName` configurado, as rotas `/api/admin` ficam liberadas.
Segredos (connection string do Neon, chaves do R2, token do GitHub) vão em `dotnet user-secrets`, nunca no appsettings.

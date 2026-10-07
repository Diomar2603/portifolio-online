# ADR 0002 — Migração da stack para AWS

- **Status:** aceito (06/10/2026)
- **Substitui:** a v2 (Cloudflare Pages/R2/Access + Azure Container Apps + Neon Postgres)

## Contexto

A solução passa a usar AWS como provedor único. Requisitos mantidos: site público sempre no ar,
1 admin, tráfego baixo, custo próximo de zero, .NET no back e Angular no front.

## Decisão

| Peça | v2 | v3 (AWS) |
|---|---|---|
| Site e admin | Cloudflare Pages | S3 + CloudFront (OAC) |
| Imagens | Cloudflare R2 | S3 + CloudFront (`media.`) |
| Auth | Cloudflare Access | Amazon Cognito (Hosted UI + PKCE) + JWT authorizer do API Gateway |
| API | Azure Container Apps (container) | AWS Lambda `dotnet10` arm64 + API Gateway HTTP API |
| Banco | Neon Postgres | Aurora Serverless v2 PostgreSQL, 0–1 ACU (pausa automática) |
| Publicar | API → GitHub (direto) | API → `snapshot.json` no S3 → Lambda PublishDispatcher → GitHub |
| DNS/TLS | Cloudflare | Route 53 + ACM |
| Deploy | GHCR + OIDC Azure + wrangler | GitHub Actions com OIDC na AWS |
| Backup | `pg_dump` noturno → R2 | Backups automáticos do Aurora (7 dias) |

**Sem NAT Gateway:** a Lambda da API precisa estar na VPC para falar com o Aurora. Para evitar o NAT
(~US$ 32/mês), ela só acessa o Aurora e o S3 (gateway endpoint, gratuito). A chamada ao GitHub, que
precisa de internet, foi movida para a Lambda PublishDispatcher, fora da VPC, acionada pelo evento do S3.
O JWT do Cognito é validado pelo API Gateway, então a API também não precisa buscar o JWKS.

## Consequências

- **Custo:** ~US$ 1–4/mês (armazenamento do Aurora + hosted zone) + domínio. Antes: ~US$ 1/mês.
- **Cold start:** após inatividade, a primeira chamada do admin pode levar ~15 s (Aurora retomando de 0 ACU)
  + ~1–2 s (Lambda). O site público não é afetado.
- **Migrations:** como o Aurora não é acessível de fora da VPC, a Lambda aplica migrations pendentes ao iniciar
  (`Database__MigrateOnStartup=true`).
- **Segredos:** a senha do banco fica em variável de ambiente da Lambda (criptografada) e no state do Terraform,
  que deve ficar em bucket S3 privado. Secrets Manager foi evitado porque exigiria VPC endpoint (~US$ 7/mês).
- **Infra como código:** toda a infraestrutura está em `infra/terraform`.

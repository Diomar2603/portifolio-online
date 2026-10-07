# Portfólio Dinâmico

Currículo/portfólio online: landing pública prerenderizada (Angular) + painel admin (CMS) com API .NET 10, hospedados na AWS.

Escopo técnico e backlog: Notion — "Portfólio Dinâmico — Escopo Técnico". Diagramas: `docs/arquitetura.drawio`.

## Estrutura

```
portifolio-online/
├── .github/workflows/   # ci · deploy-api (Lambdas) · deploy-admin · publish-site (prerender → S3 + CloudFront)
├── docs/                # arquitetura.drawio (arquitetura, fluxos, modelo de dados) + adr/
├── frontend/            # Workspace Angular (multi-projeto)
│   └── projects/
│       ├── site/        # Landing pública (SSG/prerender) — home, projetos, detalhe
│       ├── admin/       # Painel CMS com login no Cognito (PKCE)
│       └── shared/      # Modelos, UI comum e pipeline de imagens (Web Worker: EXIF, resize, WebP, SHA-256)
├── backend/             # ASP.NET Core Minimal API .NET 10
│   ├── src/
│   │   ├── Portfolio.Api/                # Endpoints /api/admin — roda como AWS Lambda atrás do API Gateway
│   │   ├── Portfolio.Application/        # Contratos e abstrações
│   │   ├── Portfolio.Domain/             # Entidades
│   │   ├── Portfolio.Infrastructure/     # EF Core + Aurora PostgreSQL, S3 (imagens e snapshot)
│   │   └── Portfolio.PublishDispatcher/  # Lambda: snapshot no S3 → repository_dispatch no GitHub
│   └── tests/
├── infra/terraform/     # Toda a infraestrutura AWS (VPC, Aurora, Lambda, API Gateway, Cognito, S3, CloudFront, Route 53)
└── scripts/
```

## Arquitetura (v3 · AWS)

- **Site público:** S3 + CloudFront, 100% estático — não depende da API nem do banco.
- **Admin:** S3 + CloudFront; login no Amazon Cognito (Hosted UI + PKCE, MFA opcional).
- **API:** Lambda .NET 10 (arm64) atrás do API Gateway HTTP API com JWT authorizer do Cognito.
- **Banco:** Aurora Serverless v2 PostgreSQL (0–1 ACU, pausa quando ocioso), em VPC privada sem NAT.
- **Imagens:** S3 + CloudFront (`media.`), upload direto do navegador com URL pré-assinada.
- **Publicar:** API grava `snapshot.json` no S3 → Lambda PublishDispatcher → GitHub Actions (prerender + `s3 sync` + invalidação).
- **Custo estimado:** ~US$ 1–4/mês + domínio. Detalhes em `infra/terraform/README.md`.

## Como rodar

```bash
# Backend
cd backend
docker compose up -d
dotnet run --project src/Portfolio.Api

# Frontend
cd frontend
npm install
npm run start:site     # landing pública
npm run start:admin    # painel admin
```

Detalhes em `backend/README.md`, `frontend/README.md` e `infra/terraform/README.md`.

## Stack
Angular · Tailwind · TipTap · .NET 10 · EF Core · Aurora Serverless v2 (PostgreSQL) · AWS Lambda · API Gateway · Cognito · S3 · CloudFront · Route 53 · Terraform · GitHub Actions (OIDC)

## Commits
Conventional Commits (`feat:`, `fix:`, `docs:`, `chore:`, `refactor:`, `test:`, `ci:`).

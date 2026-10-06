# Portfólio Dinâmico

Currículo/portfólio online: landing pública prerenderizada (Angular) + painel admin (CMS) com API .NET 10.

Escopo técnico e backlog: Notion — "Portfólio Dinâmico — Escopo Técnico".

## Estrutura

```
portifolio-online/
├── .github/workflows/   # CI/CD: api (build → GHCR → Azure Container Apps), site (prerender → Cloudflare Pages), backup noturno pg_dump → R2
├── docs/                # arquitetura.drawio (arquitetura, fluxos, modelo de dados) + adr/
├── frontend/            # Workspace Angular 20+ (multi-projeto)
│   └── projects/
│       ├── site/        # Landing pública (SSG/prerender) — home, projetos, detalhe
│       ├── admin/       # Painel CMS — perfil, formação, experiência, certificações, categorias, projetos, mídia, publicar
│       └── shared/      # Modelos, UI comum e pipeline de imagens (Web Worker: EXIF, resize, WebP, SHA-256)
├── backend/             # ASP.NET Core Minimal API .NET 10
│   ├── src/
│   │   ├── Portfolio.Api/            # Endpoints, validação JWT Cloudflare Access, middleware
│   │   ├── Portfolio.Application/    # Casos de uso por módulo
│   │   ├── Portfolio.Domain/         # Entidades
│   │   └── Portfolio.Infrastructure/ # EF Core + Neon Postgres, R2 (URLs pré-assinadas), disparo do GitHub Actions
│   └── tests/
├── infra/terraform/     # Providers Cloudflare, AzureRM e Neon
└── scripts/             # Scripts utilitários (seed, backup local etc.)
```

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

Detalhes em `backend/README.md` e `frontend/README.md`.

## Stack
Angular · Tailwind · TipTap · .NET 10 · EF Core · Neon Postgres · Cloudflare Pages/R2/Access · Azure Container Apps · Terraform · GitHub Actions

## Commits
Conventional Commits (`feat:`, `fix:`, `docs:`, `chore:`, `refactor:`, `test:`, `ci:`).

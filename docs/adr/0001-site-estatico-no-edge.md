# ADR 0001 — Site público 100% estático no edge

- **Status:** aceito (24/09/2026) · atualizado na v3 (06/10/2026): hospedagem passou de Cloudflare Pages para S3 + CloudFront ([ADR 0002](0002-migracao-para-aws.md))
- **Contexto:** o site público precisa ficar sempre no ar, com custo quase zero e tráfego baixo.
- **Decisão:** o botão Publicar dispara um workflow que prerenderiza todas as rotas com o conteúdo e publica o resultado na CDN. O visitante nunca acessa a API nem o banco.
- **Consequências:** alterações aparecem 1–3 min após publicar; em troca, disponibilidade máxima, rollback simples (republicar a versão anterior) e custo quase zero.

# ADR 0001 — Site público 100% estático no edge

- **Status:** aceito (24/09/2026)
- **Contexto:** o site público precisa ficar sempre no ar, com custo quase zero e tráfego baixo.
- **Decisão:** o botão Publicar dispara um workflow que prerenderiza todas as rotas com o conteúdo e faz deploy no Cloudflare Pages. O visitante nunca acessa a API nem o banco.
- **Consequências:** alterações aparecem 1–3 min após publicar; em troca, disponibilidade máxima, rollback instantâneo e custo zero.

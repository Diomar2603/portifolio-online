# Frontend — workspace Angular

| Projeto | Descrição | Comando |
|---|---|---|
| `site` | Landing pública, 100% prerenderizada (SSG) | `npm run start:site` |
| `admin` | Painel CMS (protegido pelo Cloudflare Access) | `npm run start:admin -- --port 4201` |
| `shared` | Modelos, diretiva de imagens (arquivo / drag & drop / Ctrl+V), pipeline WebP | importado como `@portfolio/shared` |

```bash
npm install
npm run start:site
npm run build:site      # gera dist/site/browser com uma página HTML por rota
npm test
```

O site lê o conteúdo de `projects/site/src/content/snapshot.json`. No workflow de publicação, `npm run fetch:snapshot` baixa o conteúdo da API antes do build.

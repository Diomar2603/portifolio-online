# Frontend — workspace Angular

| Projeto | Descrição | Comando |
|---|---|---|
| `site` | Landing pública, 100% prerenderizada (SSG) | `npm run start:site` |
| `admin` | Painel CMS (login pelo Amazon Cognito) | `npm run start:admin -- --port 4201` |
| `shared` | Modelos, diretiva de imagens (arquivo / drag & drop / Ctrl+V), pipeline WebP | importado como `@portfolio/shared` |

```bash
npm install
npm run start:site
npm run build:site      # gera dist/site/browser com uma página HTML por rota
npm test
```

O site lê o conteúdo de `projects/site/src/content/snapshot.json`. No workflow de publicação, o arquivo vem do S3 (gravado pelo botão Publicar); localmente, `npm run fetch:snapshot` baixa da API.

O admin faz login no Cognito (Authorization Code + PKCE). Preencha `projects/admin/src/environments/environment.ts` com os outputs do Terraform. Em desenvolvimento (`authEnabled: false`) o login é pulado e a API local roda com `Auth:DevBypass`.

// Baixa o conteúdo publicado da API e grava em projects/site/src/content/snapshot.json.
// Usado pelo workflow de publicação antes de `ng build site`.
// Variáveis: API_URL, CF_ACCESS_CLIENT_ID, CF_ACCESS_CLIENT_SECRET (service token do Cloudflare Access).
import { writeFile } from 'node:fs/promises';

const { API_URL, CF_ACCESS_CLIENT_ID, CF_ACCESS_CLIENT_SECRET } = process.env;
if (!API_URL) throw new Error('API_URL não definida');

const res = await fetch(`${API_URL}/api/admin/snapshot`, {
  headers: {
    'CF-Access-Client-Id': CF_ACCESS_CLIENT_ID ?? '',
    'CF-Access-Client-Secret': CF_ACCESS_CLIENT_SECRET ?? '',
  },
});
if (!res.ok) throw new Error(`Falha ao buscar snapshot: ${res.status}`);

await writeFile('projects/site/src/content/snapshot.json', JSON.stringify(await res.json(), null, 2));
console.log('snapshot.json atualizado');

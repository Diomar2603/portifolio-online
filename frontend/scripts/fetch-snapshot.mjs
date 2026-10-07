// Desenvolvimento local: baixa o snapshot da API e grava em projects/site/src/content/snapshot.json.
// Em produção o workflow publish-site copia o snapshot.json direto do S3 (aws s3 cp).
// Variáveis: API_URL (padrão http://localhost:5080), ACCESS_TOKEN (opcional, token do Cognito).
import { writeFile } from 'node:fs/promises';

const apiUrl = process.env.API_URL ?? 'http://localhost:5080';
const headers = process.env.ACCESS_TOKEN ? { Authorization: `Bearer ${process.env.ACCESS_TOKEN}` } : {};

const res = await fetch(`${apiUrl}/api/admin/snapshot`, { headers });
if (!res.ok) throw new Error(`Falha ao buscar snapshot: ${res.status}`);

await writeFile('projects/site/src/content/snapshot.json', JSON.stringify(await res.json(), null, 2));
console.log('snapshot.json atualizado');

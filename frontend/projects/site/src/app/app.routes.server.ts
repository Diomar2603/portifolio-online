import { RenderMode, ServerRoute } from '@angular/ssr';
import snapshot from '../content/snapshot.json';

// Site 100% estático: todas as rotas são prerenderizadas no build.
export const serverRoutes: ServerRoute[] = [
  {
    path: 'projetos/:slug',
    renderMode: RenderMode.Prerender,
    getPrerenderParams: async () =>
      snapshot.projects.filter((p) => p.status === 'published').map((p) => ({ slug: p.slug })),
  },
  { path: '**', renderMode: RenderMode.Prerender },
];

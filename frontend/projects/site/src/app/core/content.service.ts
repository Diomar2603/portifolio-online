import { Injectable } from '@angular/core';
import { Snapshot } from '@portfolio/shared';
import snapshot from '../../content/snapshot.json';

/**
 * Conteúdo do site. O JSON é gerado a partir de GET /api/admin/snapshot
 * durante o workflow de publicação e embutido no HTML prerenderizado —
 * o visitante nunca chama a API.
 */
@Injectable({ providedIn: 'root' })
export class ContentService {
  readonly snapshot = snapshot as unknown as Snapshot;

  get profile() {
    return this.snapshot.profile;
  }

  get projects() {
    return this.snapshot.projects.filter((p) => p.status === 'published');
  }

  projectBySlug(slug: string) {
    return this.projects.find((p) => p.slug === slug);
  }
}

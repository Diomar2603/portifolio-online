import { Component, computed, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ContentService } from '../../core/content.service';

@Component({
  selector: 'site-project-detail',
  imports: [RouterLink],
  template: `
    <article class="mx-auto max-w-3xl px-6 py-16">
      <a routerLink="/projetos" class="text-sm text-coral">← Projetos</a>
      @if (project(); as p) {
        <h1 class="mt-4 font-display text-4xl text-moss">{{ p.title }}</h1>
        <p class="mt-4 text-lg opacity-80">{{ p.summary }}</p>
        <!-- TODO: renderizar p.content (TipTap JSON), galeria com lightbox e cards de repositório -->
      } @else {
        <p class="mt-4">Projeto não encontrado.</p>
      }
    </article>
  `,
})
export class ProjectDetail {
  /** Vinculado ao parâmetro de rota via withComponentInputBinding() */
  readonly slug = input.required<string>();
  private readonly content = inject(ContentService);
  protected readonly project = computed(() => this.content.projectBySlug(this.slug()));
}

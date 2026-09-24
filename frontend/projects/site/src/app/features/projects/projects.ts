import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ContentService } from '../../core/content.service';

@Component({
  selector: 'site-projects',
  imports: [RouterLink],
  template: `
    <section class="mx-auto max-w-6xl px-6 py-16">
      <h1 class="font-display text-4xl text-moss">Projetos</h1>
      <div class="mt-8 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
        @for (p of projects; track p.id) {
          <a [routerLink]="['/projetos', p.slug]" class="rounded-3xl bg-white/60 p-6 shadow-sm transition hover:-translate-y-1">
            <h2 class="text-xl font-semibold">{{ p.title }}</h2>
            <p class="mt-2 opacity-80">{{ p.summary }}</p>
            <div class="mt-4 flex flex-wrap gap-2">
              @for (t of p.tags; track t) {
                <span class="rounded-full bg-moss/10 px-3 py-1 text-xs">{{ t }}</span>
              }
            </div>
          </a>
        } @empty {
          <p>Nenhum projeto publicado ainda.</p>
        }
      </div>
    </section>
  `,
})
export class Projects {
  protected readonly projects = inject(ContentService).projects;
}

import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { ContentService } from '../core/content.service';

@Component({
  selector: 'site-header',
  imports: [RouterLink, RouterLinkActive],
  template: `
    <header class="mx-auto flex max-w-6xl items-center justify-between px-6 py-6">
      <a routerLink="/" class="font-display text-xl text-moss">{{ content.profile.name }}</a>
      <nav class="flex gap-6 text-sm">
        <a routerLink="/" routerLinkActive="text-coral" [routerLinkActiveOptions]="{ exact: true }">Início</a>
        <a routerLink="/projetos" routerLinkActive="text-coral">Projetos</a>
      </nav>
    </header>
  `,
})
export class Header {
  protected readonly content = inject(ContentService);
}

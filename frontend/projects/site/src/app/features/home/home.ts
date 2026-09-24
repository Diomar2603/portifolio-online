import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ContentService } from '../../core/content.service';

// Seções previstas: Hero → Áreas de atuação → Timeline → Certificações → Contato
@Component({
  selector: 'site-home',
  imports: [RouterLink],
  template: `
    <section class="mx-auto max-w-6xl px-6 py-24">
      <p class="text-coral">Olá, eu sou</p>
      <h1 class="font-display text-[clamp(2.5rem,6vw,5rem)] leading-tight text-moss">
        {{ profile.name }}
      </h1>
      <p class="mt-4 max-w-2xl text-lg">{{ profile.headline }}</p>
      <p class="mt-2 max-w-2xl opacity-80">{{ profile.bio }}</p>
      <a routerLink="/projetos" class="mt-8 inline-block rounded-full bg-moss px-6 py-3 text-sand">
        Ver projetos
      </a>
    </section>
  `,
})
export class Home {
  protected readonly profile = inject(ContentService).profile;
}

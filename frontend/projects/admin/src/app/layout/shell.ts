import { Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ApiService } from '../core/api/api.service';
import { AuthService } from '../core/auth/auth.service';

@Component({
  selector: 'adm-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="flex min-h-screen">
      <aside class="w-60 shrink-0 border-r border-slate-200 bg-white p-4">
        <p class="font-semibold">Portfólio · Admin</p>
        <p class="mb-6 truncate text-xs text-slate-500">{{ auth.email() ?? 'modo local' }}</p>
        <nav class="flex flex-col gap-1 text-sm">
          @for (item of menu; track item.path) {
            <a [routerLink]="item.path" routerLinkActive="bg-slate-100 font-medium" class="rounded-lg px-3 py-2 hover:bg-slate-50">
              {{ item.label }}
            </a>
          }
        </nav>
        @if (auth.isAuthenticated()) {
          <button type="button" (click)="auth.logout()" class="mt-6 px-3 text-sm text-slate-500 hover:text-slate-900">Sair</button>
        }
      </aside>
      <main class="flex-1 p-8">
        <router-outlet />
      </main>
      <button
        type="button"
        (click)="publish()"
        [disabled]="publishing()"
        class="fixed right-6 bottom-6 rounded-full bg-emerald-700 px-6 py-3 text-white shadow-lg disabled:opacity-60"
      >
        {{ publishing() ? 'Publicando…' : 'Publicar' }}
      </button>
    </div>
  `,
})
export class Shell {
  private readonly api = inject(ApiService);
  protected readonly auth = inject(AuthService);
  protected readonly publishing = signal(false);

  protected readonly menu = [
    { path: 'perfil', label: 'Perfil' },
    { path: 'formacao', label: 'Formação' },
    { path: 'experiencia', label: 'Experiência' },
    { path: 'certificacoes', label: 'Certificações' },
    { path: 'categorias', label: 'Categorias' },
    { path: 'projetos', label: 'Projetos' },
    { path: 'midia', label: 'Mídia' },
  ];

  protected publish(): void {
    this.publishing.set(true);
    this.api.publish().subscribe({ complete: () => this.publishing.set(false), error: () => this.publishing.set(false) });
  }
}

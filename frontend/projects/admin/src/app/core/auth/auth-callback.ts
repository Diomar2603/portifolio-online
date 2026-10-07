import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from './auth.service';

@Component({
  selector: 'adm-auth-callback',
  template: `<p class="p-8 text-slate-500">{{ message() }}</p>`,
})
export class AuthCallback implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  protected readonly message = signal('Entrando…');

  async ngOnInit(): Promise<void> {
    const params = this.route.snapshot.queryParamMap;
    const code = params.get('code');
    const state = params.get('state');
    if (!code || !state) {
      this.message.set(params.get('error_description') ?? 'Login cancelado.');
      return;
    }
    try {
      await this.router.navigateByUrl(await this.auth.handleCallback(code, state), { replaceUrl: true });
    } catch (e) {
      this.message.set(e instanceof Error ? e.message : 'Falha no login.');
    }
  }
}

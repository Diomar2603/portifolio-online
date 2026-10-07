import { HttpClient, HttpParams } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';

interface TokenResponse {
  access_token: string;
  id_token: string;
  refresh_token?: string;
  expires_in: number;
}

interface Session {
  accessToken: string;
  idToken: string;
  refreshToken?: string;
  expiresAt: number;
}

const SESSION_KEY = 'portfolio.admin.session';
const VERIFIER_KEY = 'portfolio.admin.pkce';

/**
 * Login no Cognito (Hosted UI / Managed Login) com Authorization Code + PKCE, sem client secret.
 * Tokens ficam no sessionStorage: somem ao fechar a aba.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly cfg = environment.cognito;
  private readonly session = signal<Session | null>(this.load());

  readonly isAuthenticated = computed(() => {
    const s = this.session();
    return !!s && s.expiresAt > Date.now();
  });

  readonly email = computed(() => {
    const token = this.session()?.idToken;
    return token ? (decodeJwt(token)['email'] as string | undefined) : undefined;
  });

  get accessToken(): string | null {
    return this.session()?.accessToken ?? null;
  }

  async login(returnUrl = '/'): Promise<void> {
    const verifier = randomString(64);
    const state = randomString(24);
    sessionStorage.setItem(VERIFIER_KEY, JSON.stringify({ verifier, state, returnUrl }));

    const params = new HttpParams({
      fromObject: {
        response_type: 'code',
        client_id: this.cfg.clientId,
        redirect_uri: this.cfg.redirectUri,
        scope: this.cfg.scopes,
        state,
        code_challenge: await sha256Base64Url(verifier),
        code_challenge_method: 'S256',
      },
    });
    window.location.assign(`${this.cfg.domain}/oauth2/authorize?${params}`);
  }

  /** Troca o código pelo token. Retorna a URL para onde voltar. */
  async handleCallback(code: string, state: string): Promise<string> {
    const pending = JSON.parse(sessionStorage.getItem(VERIFIER_KEY) ?? 'null');
    sessionStorage.removeItem(VERIFIER_KEY);
    if (!pending || pending.state !== state) throw new Error('Estado de login inválido.');

    const body = new HttpParams({
      fromObject: {
        grant_type: 'authorization_code',
        client_id: this.cfg.clientId,
        code,
        redirect_uri: this.cfg.redirectUri,
        code_verifier: pending.verifier,
      },
    });
    this.save(await this.requestToken(body));
    return pending.returnUrl ?? '/';
  }

  /** Renova o access token com o refresh token (válido por 30 dias por padrão). */
  async refresh(): Promise<boolean> {
    const refreshToken = this.session()?.refreshToken;
    if (!refreshToken) return false;
    try {
      const body = new HttpParams({
        fromObject: { grant_type: 'refresh_token', client_id: this.cfg.clientId, refresh_token: refreshToken },
      });
      this.save(await this.requestToken(body), refreshToken);
      return true;
    } catch {
      this.clear();
      return false;
    }
  }

  logout(): void {
    this.clear();
    const params = new HttpParams({ fromObject: { client_id: this.cfg.clientId, logout_uri: this.cfg.logoutUri } });
    window.location.assign(`${this.cfg.domain}/logout?${params}`);
  }

  private requestToken(body: HttpParams): Promise<TokenResponse> {
    return firstValueFrom(
      this.http.post<TokenResponse>(`${this.cfg.domain}/oauth2/token`, body.toString(), {
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      }),
    );
  }

  private save(t: TokenResponse, keepRefreshToken?: string): void {
    const s: Session = {
      accessToken: t.access_token,
      idToken: t.id_token,
      refreshToken: t.refresh_token ?? keepRefreshToken,
      expiresAt: Date.now() + (t.expires_in - 60) * 1000,
    };
    sessionStorage.setItem(SESSION_KEY, JSON.stringify(s));
    this.session.set(s);
  }

  private clear(): void {
    sessionStorage.removeItem(SESSION_KEY);
    this.session.set(null);
  }

  private load(): Session | null {
    try {
      return JSON.parse(sessionStorage.getItem(SESSION_KEY) ?? 'null');
    } catch {
      return null;
    }
  }
}

function randomString(length: number): string {
  const bytes = crypto.getRandomValues(new Uint8Array(length));
  return base64Url(bytes).slice(0, length);
}

async function sha256Base64Url(value: string): Promise<string> {
  const digest = await crypto.subtle.digest('SHA-256', new TextEncoder().encode(value));
  return base64Url(new Uint8Array(digest));
}

function base64Url(bytes: Uint8Array): string {
  return btoa(String.fromCharCode(...bytes)).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
}

function decodeJwt(token: string): Record<string, unknown> {
  const payload = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
  return JSON.parse(decodeURIComponent(escape(atob(payload))));
}

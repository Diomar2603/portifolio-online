import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = async (_route, state) => {
  if (!environment.authEnabled) return true;
  const auth = inject(AuthService);
  if (auth.isAuthenticated() || (await auth.refresh())) return true;
  await auth.login(state.url);
  return false;
};

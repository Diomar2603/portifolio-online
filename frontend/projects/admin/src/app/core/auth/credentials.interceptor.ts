import { HttpInterceptorFn } from '@angular/common/http';

/**
 * O login é feito pelo Cloudflare Access na frente de admin.* e api.*.
 * Enviar cookies garante que o CF_Authorization acompanhe as chamadas à API,
 * que valida o JWT Cf-Access-Jwt-Assertion.
 */
export const credentialsInterceptor: HttpInterceptorFn = (req, next) =>
  next(req.clone({ withCredentials: true }));

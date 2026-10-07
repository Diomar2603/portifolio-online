export const environment = {
  production: false,
  apiUrl: 'http://localhost:5080',
  /** Em dev a API roda com Auth:DevBypass=true; o login só é exigido se authEnabled = true. */
  authEnabled: false,
  cognito: {
    domain: 'https://auth.seudominio.dev',
    clientId: 'PREENCHER_COM_OUTPUT_DO_TERRAFORM',
    redirectUri: 'http://localhost:4201/auth/callback',
    logoutUri: 'http://localhost:4201/',
    scopes: 'openid email profile',
  },
};

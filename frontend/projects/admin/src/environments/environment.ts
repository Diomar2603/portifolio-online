export const environment = {
  production: true,
  apiUrl: 'https://api.seudominio.dev',
  authEnabled: true,
  /** Valores gerados pelo Terraform (outputs cognito_*). */
  cognito: {
    domain: 'https://auth.seudominio.dev',
    clientId: 'PREENCHER_COM_OUTPUT_DO_TERRAFORM',
    redirectUri: 'https://admin.seudominio.dev/auth/callback',
    logoutUri: 'https://admin.seudominio.dev/',
    scopes: 'openid email profile',
  },
};

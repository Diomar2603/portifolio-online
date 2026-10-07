# Infra — Terraform (AWS)

Cria toda a infraestrutura da v3: VPC sem NAT, Aurora Serverless v2 (PostgreSQL, 0–1 ACU),
Lambda da API + API Gateway HTTP API com JWT authorizer do Cognito, Lambda PublishDispatcher,
S3 + CloudFront (site, admin e media), Route 53 + ACM, OIDC do GitHub e alertas de custo.

## Pré-requisitos

1. Conta AWS com MFA no usuário root e um usuário/SSO para o dia a dia.
2. Domínio com hosted zone no Route 53 (registrar no próprio Route 53 é o mais simples).
3. Terraform >= 1.6 e AWS CLI configurado (`aws configure sso` ou perfil).

## Primeira execução

```bash
cd infra/terraform
cp terraform.tfvars.example terraform.tfvars   # preencha
terraform init
terraform apply
```

Depois do apply:

1. **Token do GitHub** (fine-grained, *Contents: write* só no repositório):
   `aws ssm put-parameter --name /portfolio/github-token --type SecureString --overwrite --value <token>`
2. **Variáveis do GitHub Actions** (Settings → Secrets and variables → Actions → *Variables*), com os outputs:
   `AWS_REGION`, `AWS_ROLE_ARN` (=`github_role_arn`), `API_FUNCTION`, `DISPATCHER_FUNCTION`, `SITE_BUCKET`, `ADMIN_BUCKET`,
   `SNAPSHOT_BUCKET`, `SITE_DISTRIBUTION_ID`, `ADMIN_DISTRIBUTION_ID`, `API_URL`, `COGNITO_DOMAIN`, `COGNITO_CLIENT_ID`.
3. Rode os workflows **deploy-api**, **deploy-admin** e **publish-site** (aba Actions → *Run workflow*).
4. Confirme o e-mail do SNS (alertas) e faça o primeiro login no admin com a senha temporária que o Cognito enviar.

## Custos estimados (tráfego baixo)

| Item | Custo |
|---|---|
| CloudFront, S3, Lambda, Cognito (até 10 mil MAU) | US$ 0 (free tier) |
| API Gateway HTTP API | US$ 0 nos 12 primeiros meses; depois ~US$ 0,01 |
| Aurora Serverless v2 pausado (0 ACU) | ~US$ 0,10/GB de armazenamento + uso quando ativo (~US$ 1–3) |
| Route 53 hosted zone | US$ 0,50 |
| Domínio | ~US$ 12–15/ano |

**Evitado de propósito:** NAT Gateway (~US$ 32/mês), Secrets Manager e VPC interface endpoints (~US$ 7/mês cada).
A senha do banco fica em variável de ambiente da Lambda (criptografada com KMS) e no state do Terraform —
mantenha o state em bucket privado e criptografado (bloco `backend "s3"` em `versions.tf`).

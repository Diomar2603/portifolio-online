variable "aws_region" {
  description = "Região principal (API, banco, buckets)."
  type        = string
  default     = "us-east-1"
}

variable "domain_name" {
  description = "Domínio raiz com hosted zone no Route 53 (ex.: seudominio.dev)."
  type        = string
}

variable "github_repository" {
  description = "Repositório no formato usuario/repo (OIDC e repository_dispatch)."
  type        = string
}

variable "admin_email" {
  description = "E-mail do único usuário admin no Cognito (recebe a senha temporária)."
  type        = string
}

variable "budget_email" {
  description = "E-mail para alertas de custo."
  type        = string
}

variable "monthly_budget_usd" {
  description = "Limite mensal do alerta de orçamento."
  type        = number
  default     = 5
}

variable "db_max_acu" {
  description = "Máximo de ACUs do Aurora Serverless v2 (mínimo é 0 = pausa automática)."
  type        = number
  default     = 1
}

variable "db_auto_pause_seconds" {
  description = "Segundos de inatividade até o Aurora pausar."
  type        = number
  default     = 300
}

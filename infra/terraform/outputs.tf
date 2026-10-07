# Valores usados no GitHub (Settings → Secrets and variables → Actions → Variables)
# e em frontend/projects/admin/src/environments/environment.ts.

output "aws_region" { value = var.aws_region }
output "github_role_arn" { value = aws_iam_role.github_deploy.arn }

output "api_url" { value = "https://${local.api_domain}" }
output "api_function_name" { value = aws_lambda_function.api.function_name }
output "dispatcher_function_name" { value = aws_lambda_function.dispatcher.function_name }

output "site_bucket" { value = aws_s3_bucket.this["site"].bucket }
output "admin_bucket" { value = aws_s3_bucket.this["admin"].bucket }
output "snapshot_bucket" { value = aws_s3_bucket.this["snapshots"].bucket }
output "site_distribution_id" { value = aws_cloudfront_distribution.this["site"].id }
output "admin_distribution_id" { value = aws_cloudfront_distribution.this["admin"].id }

output "cognito_domain" { value = "https://${aws_cognito_user_pool_domain.main.domain}.auth.${var.aws_region}.amazoncognito.com" }
output "cognito_client_id" { value = aws_cognito_user_pool_client.admin.id }

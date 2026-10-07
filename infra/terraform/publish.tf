# Publicar: a API grava snapshot.json → S3 notifica a Lambda PublishDispatcher (fora da VPC)
# → repository_dispatch no GitHub → workflow publish-site.

# Token do GitHub (fine-grained, Contents: write só neste repositório).
# Defina o valor real depois: aws ssm put-parameter --name /portfolio/github-token --type SecureString --overwrite --value <token>
resource "aws_ssm_parameter" "github_token" {
  name  = "/${local.name}/github-token"
  type  = "SecureString"
  value = "PREENCHER"
  lifecycle {
    ignore_changes = [value]
  }
}

resource "aws_iam_role" "dispatcher" {
  name               = "${local.name}-publish-dispatcher"
  assume_role_policy = data.aws_iam_policy_document.lambda_assume.json
}

resource "aws_iam_role_policy_attachment" "dispatcher_logs" {
  role       = aws_iam_role.dispatcher.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

data "aws_iam_policy_document" "dispatcher" {
  statement {
    actions   = ["ssm:GetParameter"]
    resources = [aws_ssm_parameter.github_token.arn]
  }
  statement {
    actions   = ["kms:Decrypt"]
    resources = ["*"]
    condition {
      test     = "StringEquals"
      variable = "kms:ViaService"
      values   = ["ssm.${var.aws_region}.amazonaws.com"]
    }
  }
}

resource "aws_iam_role_policy" "dispatcher" {
  role   = aws_iam_role.dispatcher.id
  policy = data.aws_iam_policy_document.dispatcher.json
}

resource "aws_cloudwatch_log_group" "dispatcher" {
  name              = "/aws/lambda/${local.name}-publish-dispatcher"
  retention_in_days = 7
}

resource "aws_lambda_function" "dispatcher" {
  function_name = "${local.name}-publish-dispatcher"
  role          = aws_iam_role.dispatcher.arn
  runtime       = "dotnet10"
  architectures = ["arm64"]
  handler       = "Portfolio.PublishDispatcher::Portfolio.PublishDispatcher.Function::Handler"
  memory_size   = 256
  timeout       = 15
  filename      = data.archive_file.placeholder.output_path

  environment {
    variables = {
      GITHUB_REPOSITORY      = var.github_repository
      GITHUB_TOKEN_PARAMETER = aws_ssm_parameter.github_token.name
      GITHUB_EVENT_TYPE      = "publish-site"
    }
  }

  depends_on = [aws_cloudwatch_log_group.dispatcher, aws_iam_role_policy_attachment.dispatcher_logs]

  lifecycle {
    ignore_changes = [filename, source_code_hash]
  }
}

resource "aws_lambda_permission" "s3_dispatcher" {
  statement_id   = "AllowS3"
  action         = "lambda:InvokeFunction"
  function_name  = aws_lambda_function.dispatcher.function_name
  principal      = "s3.amazonaws.com"
  source_arn     = aws_s3_bucket.this["snapshots"].arn
  source_account = data.aws_caller_identity.current.account_id
}

resource "aws_s3_bucket_notification" "snapshot" {
  bucket = aws_s3_bucket.this["snapshots"].id

  lambda_function {
    lambda_function_arn = aws_lambda_function.dispatcher.arn
    events              = ["s3:ObjectCreated:*"]
    filter_prefix       = "snapshot.json"
  }

  depends_on = [aws_lambda_permission.s3_dispatcher]
}

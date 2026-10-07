# Buckets privados: o acesso público é só via CloudFront (Origin Access Control).
locals {
  buckets = {
    site      = "${local.name}-site-${random_id.suffix.hex}"
    admin     = "${local.name}-admin-${random_id.suffix.hex}"
    media     = "${local.name}-media-${random_id.suffix.hex}"
    snapshots = "${local.name}-snapshots-${random_id.suffix.hex}"
  }
}

resource "aws_s3_bucket" "this" {
  for_each = local.buckets
  bucket   = each.value
}

resource "aws_s3_bucket_public_access_block" "this" {
  for_each                = aws_s3_bucket.this
  bucket                  = each.value.id
  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

resource "aws_s3_bucket_ownership_controls" "this" {
  for_each = aws_s3_bucket.this
  bucket   = each.value.id
  rule {
    object_ownership = "BucketOwnerEnforced"
  }
}

# Upload direto do navegador (URL pré-assinada) a partir do admin.
resource "aws_s3_bucket_cors_configuration" "media" {
  bucket = aws_s3_bucket.this["media"].id
  cors_rule {
    allowed_methods = ["PUT"]
    allowed_origins = ["https://${local.admin_domain}", "http://localhost:4201"]
    allowed_headers = ["content-type"]
    max_age_seconds = 3600
  }
}

# Versões antigas do snapshot não são necessárias.
resource "aws_s3_bucket_lifecycle_configuration" "snapshots" {
  bucket = aws_s3_bucket.this["snapshots"].id
  rule {
    id     = "limpar-uploads-incompletos"
    status = "Enabled"
    filter {}
    abort_incomplete_multipart_upload {
      days_after_initiation = 1
    }
  }
}

# Leitura pelo CloudFront para site, admin e media.
data "aws_iam_policy_document" "cloudfront_read" {
  for_each = { site = "site", admin = "admin", media = "media" }

  statement {
    actions   = ["s3:GetObject"]
    resources = ["${aws_s3_bucket.this[each.key].arn}/*"]
    principals {
      type        = "Service"
      identifiers = ["cloudfront.amazonaws.com"]
    }
    condition {
      test     = "StringEquals"
      variable = "AWS:SourceArn"
      values   = [aws_cloudfront_distribution.this[each.key].arn]
    }
  }
}

resource "aws_s3_bucket_policy" "cloudfront_read" {
  for_each = data.aws_iam_policy_document.cloudfront_read
  bucket   = aws_s3_bucket.this[each.key].id
  policy   = each.value.json

  depends_on = [aws_s3_bucket_public_access_block.this]
}

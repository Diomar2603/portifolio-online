# Certificado único (raiz + subdomínios) para o CloudFront, em us-east-1.
resource "aws_acm_certificate" "cdn" {
  provider                  = aws.us_east_1
  domain_name               = var.domain_name
  subject_alternative_names = ["*.${var.domain_name}"]
  validation_method         = "DNS"
  lifecycle { create_before_destroy = true }
}

resource "aws_route53_record" "cert_validation" {
  for_each = {
    for o in aws_acm_certificate.cdn.domain_validation_options : o.resource_record_name => o...
  }
  zone_id         = data.aws_route53_zone.main.zone_id
  name            = each.value[0].resource_record_name
  type            = each.value[0].resource_record_type
  records         = [each.value[0].resource_record_value]
  ttl             = 300
  allow_overwrite = true
}

resource "aws_acm_certificate_validation" "cdn" {
  provider                = aws.us_east_1
  certificate_arn         = aws_acm_certificate.cdn.arn
  validation_record_fqdns = [for r in aws_route53_record.cert_validation : r.fqdn]
}

resource "aws_cloudfront_origin_access_control" "s3" {
  name                              = "${local.name}-s3"
  origin_access_control_origin_type = "s3"
  signing_behavior                  = "always"
  signing_protocol                  = "sigv4"
}

# Site prerenderizado: /projetos → /projetos/index.html
resource "aws_cloudfront_function" "index_rewrite" {
  name    = "${local.name}-index-rewrite"
  runtime = "cloudfront-js-2.0"
  publish = true
  code    = <<-JS
    function handler(event) {
      var request = event.request;
      var uri = request.uri;
      if (uri.endsWith('/')) {
        request.uri += 'index.html';
      } else if (!uri.split('/').pop().includes('.')) {
        request.uri += '/index.html';
      }
      return request;
    }
  JS
}

locals {
  # Políticas gerenciadas da AWS
  cache_policy_optimized  = "658327ea-f89d-4fab-a63d-7e88639e58f6" # CachingOptimized
  security_headers_policy = "67f7725c-6f97-4210-82d7-5512b31e9d03" # SecurityHeadersPolicy
  cors_s3_origin_policy   = "88a5eaf4-2fd4-4709-b370-b4c650ea3fcf" # CORS-S3Origin

  distributions = {
    site  = { domain = local.site_domain, spa = false, rewrite = true }
    admin = { domain = local.admin_domain, spa = true, rewrite = false }
    media = { domain = local.media_domain, spa = false, rewrite = false }
  }
}

resource "aws_cloudfront_distribution" "this" {
  for_each = local.distributions

  enabled             = true
  is_ipv6_enabled     = true
  http_version        = "http2and3"
  price_class         = "PriceClass_All" # inclui borda em São Paulo
  aliases             = [each.value.domain]
  default_root_object = each.key == "media" ? null : "index.html"
  comment             = "${local.name}-${each.key}"

  origin {
    domain_name              = aws_s3_bucket.this[each.key].bucket_regional_domain_name
    origin_id                = "s3"
    origin_access_control_id = aws_cloudfront_origin_access_control.s3.id
  }

  default_cache_behavior {
    target_origin_id           = "s3"
    viewer_protocol_policy     = "redirect-to-https"
    allowed_methods            = ["GET", "HEAD", "OPTIONS"]
    cached_methods             = ["GET", "HEAD"]
    compress                   = true
    cache_policy_id            = local.cache_policy_optimized
    origin_request_policy_id   = each.key == "media" ? local.cors_s3_origin_policy : null
    response_headers_policy_id = local.security_headers_policy

    dynamic "function_association" {
      for_each = each.value.rewrite ? [1] : []
      content {
        event_type   = "viewer-request"
        function_arn = aws_cloudfront_function.index_rewrite.arn
      }
    }
  }

  # Admin é SPA: rotas desconhecidas (ex.: /auth/callback) voltam para o index.html.
  dynamic "custom_error_response" {
    for_each = each.value.spa ? [403, 404] : []
    content {
      error_code         = custom_error_response.value
      response_code      = 200
      response_page_path = "/index.html"
    }
  }

  restrictions {
    geo_restriction { restriction_type = "none" }
  }

  viewer_certificate {
    acm_certificate_arn      = aws_acm_certificate_validation.cdn.certificate_arn
    ssl_support_method       = "sni-only"
    minimum_protocol_version = "TLSv1.2_2021"
  }
}

resource "aws_route53_record" "cdn" {
  for_each = {
    for pair in setproduct(keys(local.distributions), ["A", "AAAA"]) : "${pair[0]}-${pair[1]}" => { key = pair[0], type = pair[1] }
  }
  zone_id = data.aws_route53_zone.main.zone_id
  name    = local.distributions[each.value.key].domain
  type    = each.value.type

  alias {
    name                   = aws_cloudfront_distribution.this[each.value.key].domain_name
    zone_id                = aws_cloudfront_distribution.this[each.value.key].hosted_zone_id
    evaluate_target_health = false
  }
}

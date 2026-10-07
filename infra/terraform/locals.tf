locals {
  name         = "portfolio"
  site_domain  = var.domain_name
  admin_domain = "admin.${var.domain_name}"
  media_domain = "media.${var.domain_name}"
  api_domain   = "api.${var.domain_name}"
}

data "aws_caller_identity" "current" {}
data "aws_availability_zones" "available" { state = "available" }

data "aws_route53_zone" "main" {
  name = var.domain_name
}

resource "random_id" "suffix" {
  byte_length = 3
}

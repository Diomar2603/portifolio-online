terraform {
  required_version = ">= 1.6"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 6.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.6"
    }
    archive = {
      source  = "hashicorp/archive"
      version = "~> 2.4"
    }
  }

  # Estado remoto (recomendado). Crie o bucket uma vez e descomente.
  # backend "s3" {
  #   bucket       = "portfolio-tfstate-<sua-conta>"
  #   key          = "portfolio/terraform.tfstate"
  #   region       = "us-east-1"
  #   use_lockfile = true
  # }
}

provider "aws" {
  region = var.aws_region
  default_tags {
    tags = { Project = "portfolio", ManagedBy = "terraform" }
  }
}

# Certificados do CloudFront precisam estar em us-east-1.
provider "aws" {
  alias  = "us_east_1"
  region = "us-east-1"
  default_tags {
    tags = { Project = "portfolio", ManagedBy = "terraform" }
  }
}

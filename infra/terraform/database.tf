resource "random_password" "db" {
  length  = 32
  special = false
}

resource "aws_db_subnet_group" "main" {
  name       = local.name
  subnet_ids = aws_subnet.private[*].id
}

# Aurora Serverless v2 com mínimo de 0 ACU: pausa após inatividade e só cobra armazenamento.
resource "aws_rds_cluster" "main" {
  cluster_identifier     = local.name
  engine                 = "aurora-postgresql"
  engine_mode            = "provisioned"
  engine_version         = "16.6"
  database_name          = "portfolio"
  master_username        = "portfolio"
  master_password        = random_password.db.result
  db_subnet_group_name   = aws_db_subnet_group.main.name
  vpc_security_group_ids = [aws_security_group.db.id]
  storage_encrypted      = true

  backup_retention_period   = 7
  preferred_backup_window   = "06:00-07:00"
  deletion_protection       = true
  skip_final_snapshot       = false
  final_snapshot_identifier = "${local.name}-final"

  serverlessv2_scaling_configuration {
    min_capacity             = 0
    max_capacity             = var.db_max_acu
    seconds_until_auto_pause = var.db_auto_pause_seconds
  }
}

resource "aws_rds_cluster_instance" "main" {
  identifier          = "${local.name}-1"
  cluster_identifier  = aws_rds_cluster.main.id
  instance_class      = "db.serverless"
  engine              = aws_rds_cluster.main.engine
  engine_version      = aws_rds_cluster.main.engine_version
  publicly_accessible = false
}

locals {
  db_connection_string = join(";", [
    "Host=${aws_rds_cluster.main.endpoint}",
    "Port=5432",
    "Database=${aws_rds_cluster.main.database_name}",
    "Username=${aws_rds_cluster.main.master_username}",
    "Password=${random_password.db.result}",
    "SSL Mode=Require",
    "Timeout=30",
  ])
}

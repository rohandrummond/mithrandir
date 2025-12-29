variable "aws_region" {
  description = "AWS region for resources"
  type        = string
  default     = "ap-southeast-2"
}

variable "project_name" {
  description = "Project name used for resource naming"
  type        = string
  default     = "mithrandir"
}

variable "my_ip" {
  description = "Your public IP address for SSH access"
  type        = string
  default     = "161.29.122.70"
}

variable "key_name" {
  description = "Name of the SSH key pair in AWS"
  type        = string
  default     = "mithrandir"
}

variable "instance_type" {
  description = "EC2 instance type"
  type        = string
  default     = "t4g.micro"
}

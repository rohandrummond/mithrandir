output "ecr_repository_url" {
  description = "URL of the ECR repository"
  value       = aws_ecr_repository.api.repository_url
}

output "ecr_repository_arn" {
  description = "ARN of the ECR repository"
  value       = aws_ecr_repository.api.arn
}

output "public_ip" {
  description = "Public IP address of the EC2 instance"
  value       = aws_eip.api.public_ip
}

output "ssh_command" {
  description = "SSH command to connect to the instance"
  value       = "ssh -i ~/.ssh/mithrandir.pem ec2-user@${aws_eip.api.public_ip}"
}

output "api_url" {
  description = "URL to access the API"
  value       = "http://${aws_eip.api.public_ip}"
}

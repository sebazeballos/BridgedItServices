@echo off

REM Install AWS SDK for .NET
dotnet tool install -g Amazon.Lambda.Tools

REM Set AWS credentials as environment variables
setx AWS_ACCESS_KEY_ID "your-access-key-id"
setx AWS_SECRET_ACCESS_KEY "your-secret-access-key"
setx AWS_REGION "your-aws-region"

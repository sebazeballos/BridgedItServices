#!/bin/bash

# Install AWS SDK for .NET
dotnet tool install -g Amazon.Lambda.Tools

# Set AWS credentials as environment variables
export AWS_ACCESS_KEY_ID="your-access-key-id"
export AWS_SECRET_ACCESS_KEY="your-secret-access-key"
export AWS_REGION="your-aws-region"
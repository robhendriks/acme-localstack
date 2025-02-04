# Acme

Transactional inbox & outbox built on top of AWS Lambda, DynamoDB, SQS and SNS.

## Architecture Diagram

t.b.d.

## Requirements

- [LocalStack](https://www.localstack.cloud/) Pro or Hobby<sup>1</sup> license
- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Docker Desktop](https://docs.docker.com/desktop/)
- [AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html)

> 1: You can apply for a Hobby (Pro) license when you sign up using a personal email address.

## Getting Started

### LocalStack

1. Open your terminal and navigate to the `localstack` folder inside this repository
2. Copy the `.env.example` file to `.env` and open it in a text editor
3. Provide the value for `LOCALSTACK_AUTH_TOKEN`, use your personal LocalStack auth token
4. Run `docker compose up -d` to start LocalStack

### AWS

Add the following snippet to `~/.aws/credentials`:

```ini
[localstack]
aws_access_key_id = test
aws_secret_access_key = test
```

Add the following snippet to `~/.aws/config`:

```ini
[profile localstack]
region = us-east-1
output = json
endpoint_url = http://localhost:4566
```

> `~` on Windows is equal to `%USERPROFILE%`

### CDK

1. Open your terminal and navigate to the `infra` folder
2. Run `npm i` to install node modules
3. Run `npm run cdk:bootstrap`

#### Deploy Infra Stack

When CDK is bootstrapped you can use the following command to deploy the core infra stack:

```sh
npm run cdk:infra
```

- ApiGatewayV2
- EventBridge

#### Deploy Ordering Stack

When the core infra is deployed, run the following command to deploy the `ordering` stack:

```sh
npm run cdk:ordering
```

- SNS Topic
- Inbox
    - DynamoDB Table
    - SQS Queue + DLQ
    - EventBridge Pipe
- Outbox
    - DynamoDB Table
    - SQS Queue + DLQ
    - EventBridge Pipe
- CreateOrder Lambda
    - DynamoDB Table
- GetOrder Lambda
- OrderRequestedProcessor Lambda
    - SQS Queue + DLQ

## HTTP Apis

- [Ordering](apis/Ordering.http)
- [LocalStack](apis/LocalStack.http)

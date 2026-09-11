# Partner Integration BFF

A .NET 8 Backend-For-Frontend (BFF) microservice that receives partner transaction
requests, verifies the partner against an (intentionally unreliable) external API
using a Polly resilience policy, and publishes verified transactions to RabbitMQ.

# Tech stack

.NET 8
Clean Architecture
ASP.NET Core Web API
FluentValidation
Polly Retry Policy
RabbitMQ
Global Exception Handler
API Key Authentication
Unit Test (xUnit, Moq, FluentAssertions)
Docker
Swagger

---

# Request Flow Diagram

```
Partner ──POST /api/v1/partner/transactions──► TransactionsController
                                                      │
                                          1. Validate (FluentValidation)
                                             │ invalid → 400 ProblemDetails
                                             ▼ valid
                                          2. Build domain PartnerTransaction
                                                      │
                                          3. VerifyPartnerAsync(partnerId)
                                                      │
                              ┌───────────────────────┴────────────────────────┐
                              ▼                                                │
                    HttpClient → GET /mock-api/partners/{id}                   │
                              │                                                │
              ┌───────────────┴───────────────┐                                │
              │ 70%: 200 OK { valid: true }    │ 30%: throws → API layer       │
              │                                │ turns it into HTTP 500        │
              └───────────────┬────────────────┘                               │
                               │                                               │
                    Polly: is response a failure? ───────────────────────────►─┘
                               │ yes → retry (3s), log each attempt
                               │ after 3 retries still failing → give up gracefully
                               ▼
                    4. verified?
                       │ no  → mark PartnerVerificationFailed → 200 response,
                       │        Published=false, explains why (no crash)
                       │ yes → mark PartnerVerified
                       ▼
                    5. PublishAsync(partner-transactions, message)
                       │ success → mark Published
                       │ failure → mark PublishFailed (still 200, never throws)
                       ▼
                    6. Return TransactionResponseModel (200 OK)
```

# Project structure

PartnerIntegration.Bff
│
├── API
│ ├── Controllers
│ └── Middleware
│
├── Application
│ ├── Exceptions
│ ├── Interfaces
│ ├── Models
│ ├── Services
│ └── Validators
│
├── Domain
│ ├── Common
│ ├── Constants
│ └── Enums
│
├── Infrastructure
│ ├── DependencyInjection
│ ├── ExternalServices
│ ├── Messaging
│ └── Resilience
│
├── Tests
│ └── UnitTests
│
└── Docker

I implement the Api, Application, Domain, and Infrastructure structure because it follows Clean Architecture principles and clearly separates concerns. The Domain layer contains business concepts and remains independent of frameworks and external systems. The Application layer implements business workflows and use cases. The Infrastructure layer handles technical concerns such as RabbitMQ, HttpClient, and external integrations. The API layer is responsible only for HTTP communication. This structure improves maintainability, testability, scalability, and allows infrastructure technologies to be replaced without affecting business logic. For a partner integration service, this approach provides a clean, production-ready design and demonstrates proper use of dependency inversion and separation of concerns.

- **FluentValidation for request validation**, run inside
  `TransactionService` rather than via an MVC filter, so the same validation
  path is exercised identically whether the request arrives over HTTP or from another entry point later.
  Validation Rules:
  partnerId required
  transactionReference required
  amount > 0
  currency must be ISO currency format
  timestamp required

- **Polly Retry Policy in `PartnerVerificationPolicies`**, registered via
  `AddPolicyHandler` on a named `HttpClient`, rather than inlined in
  `Program.cs`. This makes the retry/backoff/timeout behavior
  Flow:
  Timeout
  ↓
  Retry 1
  ↓
  Retry 2
  ↓
  Retry 3
  ↓
  Fail Gracefully

- **Global exception handling via `IExceptionHandler`** (the .NET 8 idiomatic
  approach, replacing the older exception-handling middleware pattern),
  translating both expected (`ValidationException`)

- **RabbitMQ** publisher declares the queue as durable and publishes
  messages, lightweight and easy to settup.

- API Key Authentication because this service is a partner-to-partner integration API
  rather than an end-user application. The coding test does not include user management, login, roles, or identity requirements.
  API Key Authentication is simpler, faster to implement, and more appropriate for machine-to-machine communication.

# Local Setup (without Docker)

Prerequisites: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0),
a running RabbitMQ instance (or use `docker compose up rabbitmq` from this repo).

```bash
# from the repository root
dotnet restore
dotnet build

# run RabbitMQ only, via Docker, if you don't have one locally
docker compose up -d rabbitmq

```

# Running Application

## Run API project

Navigate to API project:

```bash
cd API
```

Run:

```bash
dotnet run
```

The API listens on `http://localhost:5277`. Default API key (see
`appsettings.json`, override via `ApiKey` config/env var for anything beyond
local dev): `my-secret-key`.

Example request:

```bash
curl -X POST http://localhost:5277/api/v1/partner/transactions \
  -H "Content-Type: application/json" \
  -H "X-API-KEY: my-secret-key" \
  -d '{
        "partnerId": "P-1001",
        "transactionReference": "TXN-99823",
        "amount": 250,
        "currency": "USD",
        "timestamp": "2024-05-10T14:30:00Z"
      }'
```

# Docker Setup

```bash
docker compose up --build
```

This starts two containers:

| Container                      | Port(s)         | Purpose                        |
| ------------------------------ | --------------- | ------------------------------ |
| `partner-integration-api`      | `5277`          | The BFF API + mock partner API |
| `partner-integration-rabbitmq` | `5672`, `15672` | AMQP broker + management UI    |

RabbitMQ management UI: `http://localhost:15672` (guest/guest).
API: `http://localhost:5277/swagger`.

# API Reference

## `POST /api/v1/partner/transactions`

Headers: `X-API-KEY: <key>` (required), `Content-Type: application/json`.

Request body:

```json
{
  "partnerId": "P-1001",
  "transactionReference": "TXN-99823",
  "amount": 250,
  "currency": "USD",
  "timestamp": "2024-05-10T14:30:00Z"
}
```

Response in common object

```json
{
  "data": {
    "partnerId": "P-1001",
    "transactionReference": "TXN-99823",
    "status": "Published",
    "partnerVerified": true,
    "published": true,
    "message": "Transaction accepted and published."
  },
  "status": {
    "code": "success",
    "msg": ""
  }
}
```

## `GET /mock-api/partners/{partnerId}`

# Running Tests

```bash
dotnet test PartnerIntegration.UnitTests
```

run specific Unit test

```bash
dotnet test --filter "ClassName=TransactionServiceTests"
```

---

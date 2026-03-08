# FAZ 2: Vitrin Proje #1 - Backend Odakli (.NET API) (3 Hafta)

> Bu proje, senin C#/.NET backend yetkinligini gostermek icin VAR.
> Avrupa'daki Senior .NET Developer ilanlarinin istedigi HER SEY bu projede olmali.

---

## PROJE: "OrderFlow" - E-Commerce Order Management Microservice

### Neden bu proje?
- E-commerce domain'i herkes tarafindan anlasilir
- CQRS, Event Sourcing, Microservice pattern'leri gosterir
- Senin Gervig'deki deneyiminle (CQRS, MediatR, Kafka) dogrudan otuyor
- Avrupa'daki fintech/e-commerce sirketleri tam bunu ariyor

### Tech Stack:
- .NET 8 Web API
- Entity Framework Core 8 + PostgreSQL
- MediatR (CQRS pattern)
- FluentValidation
- Redis (caching + distributed locking)
- RabbitMQ (event-driven communication)
- Docker + Docker Compose
- xUnit + Moq + Testcontainers
- Serilog + Seq (structured logging)
- Swagger/OpenAPI
- GitHub Actions (CI/CD)

---

## Hafta 1: Temel Altyapi

### Gun 1-2: Proje Yapisini Olustur (Clean Architecture)

```
OrderFlow/
├── src/
│   ├── OrderFlow.API/              # Presentation Layer
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   ├── Filters/
│   │   └── Program.cs
│   ├── OrderFlow.Application/      # Application Layer (Use Cases)
│   │   ├── Commands/
│   │   ├── Queries/
│   │   ├── DTOs/
│   │   ├── Interfaces/
│   │   ├── Validators/
│   │   └── Behaviors/
│   ├── OrderFlow.Domain/           # Domain Layer (Entities, Value Objects)
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   ├── Enums/
│   │   ├── Events/
│   │   └── Exceptions/
│   └── OrderFlow.Infrastructure/   # Infrastructure Layer
│       ├── Persistence/
│       ├── Repositories/
│       ├── Caching/
│       ├── Messaging/
│       └── Services/
├── tests/
│   ├── OrderFlow.UnitTests/
│   ├── OrderFlow.IntegrationTests/
│   └── OrderFlow.ArchitectureTests/
├── docker/
│   ├── Dockerfile
│   └── docker-compose.yml
├── .github/
│   └── workflows/
│       └── ci.yml
├── README.md
├── .editorconfig
└── OrderFlow.sln
```

- [ ] Solution olustur: `dotnet new sln -n OrderFlow`
- [ ] Katmanli projeleri olustur
- [ ] NuGet paketlerini ekle
- [ ] .editorconfig ekle (kod stili tutarliligi)
- [ ] .gitignore olustur
- [ ] Ilk commit: `feat: initialize clean architecture solution structure`

### Gun 3-4: Domain Layer

- [ ] Order Entity (Id, CustomerId, Items, Status, TotalAmount, CreatedAt, UpdatedAt)
- [ ] OrderItem Value Object (ProductId, ProductName, Quantity, UnitPrice)
- [ ] OrderStatus Enum (Pending, Confirmed, Processing, Shipped, Delivered, Cancelled)
- [ ] Domain Events:
  - OrderCreatedEvent
  - OrderConfirmedEvent
  - OrderCancelledEvent
  - OrderShippedEvent
- [ ] Custom Domain Exceptions (OrderNotFoundException, InvalidOrderStateException)
- [ ] Domain Validation (Guard clauses)

### Gun 5: Application Layer - Commands

- [ ] CreateOrderCommand + Handler
- [ ] ConfirmOrderCommand + Handler
- [ ] CancelOrderCommand + Handler
- [ ] UpdateOrderStatusCommand + Handler
- [ ] FluentValidation validators her command icin
- [ ] MediatR Pipeline Behaviors:
  - ValidationBehavior
  - LoggingBehavior
  - PerformanceBehavior (yavaş query'leri logla)

---

## Hafta 2: Infrastructure + API

### Gun 6-7: Infrastructure Layer

- [ ] EF Core DbContext + Configuration (Fluent API)
- [ ] Repository pattern implementation
- [ ] Unit of Work pattern
- [ ] PostgreSQL migration'lari
- [ ] Redis cache service (IDistributedCache)
- [ ] RabbitMQ publisher/consumer setup
- [ ] Serilog configuration (console + file + Seq)

### Gun 8-9: API Layer

- [ ] Controllers:
  - OrdersController (CRUD + status transitions)
  - HealthCheckController
- [ ] API Versioning (v1, v2 - best practice gostermek icin)
- [ ] Global Exception Handling Middleware
- [ ] Request/Response logging middleware
- [ ] Authentication & Authorization (JWT Bearer)
- [ ] Rate Limiting
- [ ] CORS configuration
- [ ] Swagger/OpenAPI dokumantasyonu (XML comments ile)
- [ ] Response caching

### Gun 10: Docker & Docker Compose

```yaml
# docker-compose.yml icermesi gerekenler:
services:
  api:
    build: .
    ports: ["5000:8080"]
    depends_on: [postgres, redis, rabbitmq]
  postgres:
    image: postgres:16
  redis:
    image: redis:7-alpine
  rabbitmq:
    image: rabbitmq:3-management
  seq:
    image: datalust/seq
```

- [ ] Multi-stage Dockerfile (build + runtime)
- [ ] docker-compose.yml (tum servisler)
- [ ] docker-compose.override.yml (development)
- [ ] Health check'ler ekle
- [ ] `docker compose up` ile tek komutla calisir hale getir

---

## Hafta 3: Test + CI/CD + Dokumantasyon

### Gun 11-12: Testler

- [ ] Unit Tests (Domain + Application layer):
  - Order entity testleri
  - Command handler testleri
  - Validator testleri
  - En az 20+ unit test
- [ ] Integration Tests (Testcontainers ile):
  - API endpoint testleri
  - Repository testleri
  - Gercek PostgreSQL container'i ile
  - En az 10+ integration test
- [ ] Architecture Tests (NetArchTest ile):
  - Domain katmani baska katmana bagimli olmamali
  - Application katmani Infrastructure'a bagimli olmamali

### Gun 13: GitHub Actions CI/CD

```yaml
# .github/workflows/ci.yml
name: CI
on: [push, pull_request]
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
      - run: dotnet build
      - run: dotnet test --collect:"XPlat Code Coverage"
      - name: Upload coverage to Codecov
        uses: codecov/codecov-action@v4
```

- [ ] Build + Test pipeline
- [ ] Code coverage raporu (Codecov badge)
- [ ] Docker image build
- [ ] Lint checks

### Gun 14-15: README ve Dokumantasyon (SUPER ONEMLI)

README.md su bolumleri icermeli:

```markdown
# OrderFlow - E-Commerce Order Management API

[![CI](badge)](link)
[![Coverage](badge)](link)
[![License: MIT](badge)](link)

## Overview
Clean Architecture ile olusturulmus, production-ready bir order management API.

## Architecture
[Diyagram - draw.io veya Mermaid ile]

## Tech Stack
- .NET 8, EF Core 8, PostgreSQL, Redis, RabbitMQ
- CQRS + MediatR, FluentValidation
- Docker, GitHub Actions

## Getting Started
### Prerequisites
### Running with Docker
### Running Locally
### API Documentation

## Project Structure
[Klasor yapisi aciklamasi]

## Design Decisions
- Neden Clean Architecture?
- Neden CQRS?
- Neden Event-driven?

## Testing
### Running Tests
### Test Coverage

## API Endpoints
[Tablo veya Swagger screenshot]

## Contributing
## License
```

- [ ] Detayli README yaz (INGILIZCE)
- [ ] Architecture diagram ciz (Mermaid veya draw.io)
- [ ] API endpoint dokumantasyonu
- [ ] "Design Decisions" bolumu (CQRS, Clean Arch secimlerini acikla)
- [ ] Screenshots/GIF'ler ekle (Swagger UI, calisir hali)
- [ ] Codecov + CI badge'leri ekle

---

## BU PROJENIN GOSTERDIGI YETKINLIKLER:

| Yetkinlik | Kanitlayan Ozellik |
|---|---|
| Clean Architecture | 4 katmanli proje yapisi |
| CQRS Pattern | MediatR commands/queries |
| Domain-Driven Design | Entities, Value Objects, Domain Events |
| Testing | Unit + Integration + Architecture tests |
| DevOps | Docker, CI/CD, Health checks |
| API Design | Versioning, Swagger, Rate limiting |
| Event-Driven | RabbitMQ integration |
| Caching | Redis distributed cache |
| Logging | Structured logging with Serilog |
| Code Quality | .editorconfig, analyzers, coverage |

---

## CHECKLIST

- [ ] Clean Architecture solution structure
- [ ] Domain layer (entities, value objects, events)
- [ ] CQRS with MediatR
- [ ] EF Core + PostgreSQL
- [ ] Redis caching
- [ ] RabbitMQ messaging
- [ ] JWT authentication
- [ ] Docker + Docker Compose
- [ ] 20+ unit tests, 10+ integration tests
- [ ] GitHub Actions CI/CD
- [ ] Kapsamli README (Ingilizce)
- [ ] Architecture diagram

> Tahmini sure: 15-20 gun
> Sonraki adim: [FAZ 3 - Vitrin Proje #2 Full Stack](./faz-3-proje-fullstack.md)

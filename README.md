# Enterprise E-Commerce Platform – Microservices Architecture & Saga Pattern

A production-grade, enterprise-scale E-Commerce platform built with **.NET 9**, **ASP.NET Core Web API**, **Clean Architecture**, **CQRS with MediatR**, **Ocelot API Gateway**, and the **Orchestration-based Saga Pattern**.

---

## 🏛️ Architecture Overview

The platform is designed following Domain-Driven Design (DDD), Clean Architecture, and autonomous Database-per-Microservice boundaries:

- **API Gateway**: Ocelot Gateway with centralized routing, rate limiting, and correlation tracking.
- **Messaging & Event-Driven Architecture**: Transactional Outbox & Inbox patterns with Azure Service Bus contracts.
- **Distributed Transactions**: Orchestration-based Saga State Machine with automated compensations.
- **Observability**: OpenTelemetry distributed tracing and structured Correlation ID propagation.

```
                           +--------------------------+
                           |  Ocelot API Gateway      | (Port 5000)
                           +------------+-------------+
                                        |
      +------------------+--------------+---------------+------------------+
      |                  |              |               |                  |
+-----v-------+    +-----v-------+  +---v---------+ +---v----------+ +-----v---------+
| Identity    |    | Product     |  | Cart        | | Inventory    | | Order         |
| Service     |    | Service     |  | Service     | | Service      | | Service       |
| (Port 5001) |    | (Port 5002) |  | (Port 5003) | | (Port 5006)  | | (Port 5004)   |
+-------------+    +-------------+  +-------------+ +--------------+ +---------------+
      |                                                                    |
+-----v-------+                                                      +-----v---------+
| Payment     |                                                      | Shipping      |
| Service     |                                                      | Service       |
| (Port 5005) |                                                      | (Port 5007)   |
+-------------+                                                      +---------------+
```

---

## 🚀 Implemented Microservices & Modules

### 1. Building Blocks (`src/BuildingBlocks/`)
- `BuildingBlocks.Common`: Domain primitives (`Entity<T>`, `AggregateRoot<T>`, `ValueObject`, `IDomainEvent`), Result pattern, MediatR pipeline behaviors (`LoggingBehavior`, `ValidationBehavior`).
- `BuildingBlocks.Contracts`: Strongly-typed Integration Events & Saga Commands (`OrderEvents`, `InventoryEvents`, `PaymentEvents`, `ShippingEvents`, `SagaCommands`).
- `BuildingBlocks.Messaging`: Transactional Outbox pattern (`OutboxMessage`, background processor), Inbox deduplication pattern.
- `BuildingBlocks.Observability`: Correlation ID middleware, Global exception handling, OpenTelemetry tracing hooks.

### 2. Microservices (`src/Services/`)
- **Identity Service** (Port 5001): User Aggregate, PBKDF2 hashing (100k iterations), JWT access & refresh tokens, Role-based authorization (`Customer`, `Admin`).
- **Product Service** (Port 5002): Product & Category aggregates, normalized SKUs, image handling, pagination & search queries.
- **Cart Service** (Port 5003): ShoppingCart aggregate with item collection, quantity aggregation, total calculation.
- **Inventory Service** (Port 5006): InventoryItem aggregate (Reserve, Release, CommitDeduction, Restock), atomic pre-reservation validation.
- **Order Service** (Port 5004): Order aggregate (`Pending`, `StockReserved`, `Paid`, `Confirmed`, `Cancelled`, `Shipped`), Transactional Outbox with `OrderPlacedIntegrationEvent`, returns `202 Accepted` for async Saga tracking.
- **Payment Service** (Port 5005): Payment aggregate, Sandbox transaction gateway, Transactional Outbox with `PaymentCompletedIntegrationEvent` / `PaymentFailedIntegrationEvent`, refund handling.
- **Shipping Service** (Port 5007): Shipment aggregate (`Created`, `Dispatched`, `InTransit`, `Delivered`, `Failed`), Tracking number generation (`TRK-...`), Transactional Outbox with `ShipmentCreatedIntegrationEvent`, carrier status advance.
- **Notification Service** (Port 5008): NotificationLog aggregate, multi-channel support (`Email`, `Sms`), event-based customer notifications (`OrderConfirmation`, `PaymentReceipt`, `ShipmentDispatched`, `OrderCancelled`), delivery audit log.
- **Saga Orchestrator Service** (Port 5009): Coordinates distributed order-processing workflow with explicit state transitions, compensating transactions (inventory release, order cancellation), manual intervention escalation queue, and tracking endpoints.

### 3. API Gateway (`src/ApiGateway/`)
- **OcelotGateway** (Port 5000): Centralized routing, rate limiting, JWT Bearer configuration, CORS policy for Angular. Configured for auth, users, products, categories, cart, inventory, orders, payments, shipping, notifications, and saga orchestration.

---

## 🧪 Testing

149 unit tests with 100% pass rate:
- `BuildingBlocks.Tests` (12 tests)
- `Identity.UnitTests` (12 tests)
- `Product.UnitTests` (19 tests)
- `Cart.UnitTests` (14 tests)
- `Inventory.UnitTests` (24 tests)
- `Order.UnitTests` (17 tests)
- `Payment.UnitTests` (12 tests)
- `Shipping.UnitTests` (14 tests)
- `Notification.UnitTests` (10 tests)
- `SagaOrchestrator.UnitTests` (15 tests)

Run tests locally:
```bash
dotnet test ECommercePlatform.sln
```

---

## 🛠️ Technology Stack
- **Backend**: .NET 9 / C# 13, ASP.NET Core Web API, Entity Framework Core 9
- **Architecture**: Clean Architecture, CQRS, MediatR, FluentValidation
- **Gateway**: Ocelot 25
- **Testing**: xUnit, FluentAssertions, Moq, EF Core InMemory

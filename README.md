# Rezerv API

Minimal Clean Architecture API targeting .NET 8, MySQL, Entity Framework Core, and Swagger UI.

## Projects

- `src/Rezerv.Domain`: framework-independent business core.
- `src/Rezerv.Application`: future CQRS contracts, handlers, DTOs, and validation.
- `src/Rezerv.Infrastructure`: EF Core and MySQL persistence services.
- `src/Rezerv.Api`: HTTP host, Swagger UI, and health endpoints.
- `tests/Rezerv.IntegrationTests`: API integration tests.

## Run locally

1. Copy `.env.example` to `.env` and replace the passwords.
2. Run `docker compose up -d`.
3. Run `dotnet run --project src/Rezerv.Api`.
4. Open `/swagger` in the Development environment.

The API provides a liveness probe at `/health` and a MySQL readiness probe at `/health/ready`.

## Package APIs

- `GET /api/packages?customerId={customerId}&businessId={businessId}` lists a customer's unexpired packages with remaining credits. The business filter is optional.
- `POST /api/packages/purchase` creates a customer-owned package. Send `customerId`, `businessId`, `totalCredits`, and `expiresAtUtc` in the request body.

`Package` stores the business, customer, total credits, remaining credits, and expiry date, matching the assessment specification.

## Build and test

Run `dotnet build Rezerv.slnx -c Release` and `dotnet test Rezerv.slnx -c Release`.
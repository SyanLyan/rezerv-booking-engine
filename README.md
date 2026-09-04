# Rezerv API

Minimal Clean Architecture API targeting .NET 8, MySQL, Entity Framework Core, and Swagger UI.

## Projects

- `src/Rezerv.Domain`: framework-independent business core.
- `src/Rezerv.Application`: future CQRS contracts, handlers, DTOs, and validation.
- `src/Rezerv.Infrastructure`: EF Core and MySQL persistence services.
- `src/Rezerv.Api`: HTTP host, Swagger UI, and health endpoints.
- `tests/Rezerv.IntegrationTests`: API integration tests.

## Run locally

1. Run `docker compose up -d`.
2. Run `dotnet run --project src/Rezerv.Api`.
3. Open `/swagger` in the Development environment.

The API provides a liveness probe at `/health` and a MySQL readiness probe at `/health/ready`.

## Package APIs

- `GET /api/packages?businessId={businessId}` lists active, unexpired package offers. The business filter is optional.
- `POST /api/packages` creates a business-owned package offer.
- `POST /api/packages/purchase` creates a customer-owned package balance. Send `customerId` and `packageId` in the request body.

`Package` is a business-owned offer with a fixed expiry date. `CustomerPackage` stores the purchased credits for a specific customer.

## Build and test

Run `dotnet build Rezerv.slnx -c Release` and `dotnet test Rezerv.slnx -c Release`.

# Rezerv API

Rezerv Booking engine API for fitness studios, with .net 8, entity framework, MySQL

## Projects

- `src/Rezerv.Domain`: entities and shared domain types.
- `src/Rezerv.Application`: commands, DTOs, services, repository abstractions, and booking rules.
- `src/Rezerv.Infrastructure`: Entity Framework Core, MySQL configuration, migrations, and repository implementations.
- `src/Rezerv.Api`: HTTP controllers, request contracts, validation, standard response bodies, Swagger, and health endpoints.

## Local setup

1. Start MySQL with docker script:

	```powershell
	docker compose up -d
	```

2. Apply database migrations:

	 ```powershell
	 dotnet tool run dotnet-ef database update --project src/Rezerv.Infrastructure --startup-project src/Rezerv.Api
	 ```

3. Run the API:

	 ```powershell
	 dotnet run --project src/Rezerv.Api
	 ```

4. Open `/swagger` while running in the Development environment.

`appsettings.Development.json` is local-only and must use `127.0.0.1` as the MySQL server when Docker publishes MySQL to the host.

## API response body

All controller responses follow this format:

{
	"success": true,
	"message": "Fetched successfully.",
	"data": {},
	"errors": null
}

## Package APIs

- `GET /api/packages?businessId={businessId}`: lists active, unexpired package offers. The business filter is optional.
- `POST /api/packages`: creates a business-owned package offer.
- `POST /api/packages/purchase`: creates a customer-owned package balance using `customerId` and `packageId`.

## Timetable APIs

- `GET /api/timetable?businessId={businessId}&date={yyyy-MM-dd}`: lists schedules. Both filters are optional and can be used together.
- `POST /api/timetable`: creates a timetable schedule.

## Booking APIs

- `POST /api/bookings`: creates a booking using `customerId`, `timetableScheduleId`, and `customerPackageId`.


When a schedule has availability, the response contains a booking with `status` set to `Booked`; one slot and one package credit are deducted in the same database transaction. When the schedule is full, the response has `status` set to `Waitlisted`; no slot or credit is deducted.

## Assumptions

- I treat each booking as a reservation for one slot in one timetable schedule.
- I keep each package purchase as a separate customer package balance, even when the customer buys the same package more than once. I do not combine credits; the customer selects the separate package to use for each booking.
- I allow each customer to have only one active booking or waitlist entry for the same schedule.
- I prevent customers from booking classes that overlap in time as requested, but allow classes that start exactly when another class ends.
- For the waitlist FIFO auto booked scenario, I promote waitlisted customers in the order they joined the queue, using `CreatedAtUtc`; the booking ID breaks a tie when two entries have the same timestamp.

## Validation and booking rules

All POST request models use FluentValidation. Validation errors are returned in the standard response body's `errors` array.

The booking rule engine enforces these conditions before creating a booking:

- The schedule has not started.
- `AvailableSlots` is greater than zero.
- The customer has at least one package credit.
- The package has not expired and belongs to the schedule's business.
- The customer does not already have a booking for the schedule.

Each booking reserves one slot and consumes one package credit. A cancellation made at least four hours before the schedule refunds one credit; a later cancellation does not refund a credit.

## Health checks

- `GET /api/health`: API controller health response.
- `GET /health`: liveness probe without a database dependency.
- `GET /health/ready`: MySQL readiness probe.

## Build

```powershell
dotnet build Rezerv.slnx -c Release
```

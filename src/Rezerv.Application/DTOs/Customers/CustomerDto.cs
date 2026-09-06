namespace Rezerv.Application.DTOs.Customers;

public sealed record CustomerDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    DateTime CreatedAtUtc);
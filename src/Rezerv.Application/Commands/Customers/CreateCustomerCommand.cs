namespace Rezerv.Application.Commands.Customers;

public sealed record CreateCustomerCommand(
    string FirstName,
    string LastName,
    string Email);
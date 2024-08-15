namespace KittySaver.Api.Features.Persons.Contracts;

public sealed class PersonResponse
{
    public required Guid Id { get; init; }
    public required string FullName { get; init; }
    public required string Email { get; init; }
    public required string PhoneNumber { get; init; }
}
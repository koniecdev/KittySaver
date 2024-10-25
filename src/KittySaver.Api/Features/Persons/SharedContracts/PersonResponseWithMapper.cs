using KittySaver.Api.Shared.Domain.Entites;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Api.Features.Persons.SharedContracts;

public sealed class PersonResponse
{
    public required Guid Id { get; init; }
    public required Guid UserIdentityId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string FullName { get; init; }
    public required string Email { get; init; }
    public required string PhoneNumber { get; init; }
    public required string AddressCountry { get; init; }
    public required string? AddressState { get; init; }
    public required string AddressCity { get; init; }
    public required string AddressStreet { get; init; }
    public required string AddressBuildingNumber { get; init; }
    public required string AddressZipCode { get; init; }
}

[Mapper]
public static partial class PersonResponseMapper
{
    public static partial IQueryable<PersonResponse> ProjectToDto(
        this IQueryable<Person> persons);
}
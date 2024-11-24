using KittySaver.Domain.Persons;
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
    public required string DefaultAdvertisementsContactInfoEmail { get; init; }
    public required string DefaultAdvertisementsContactInfoPhoneNumber { get; init; }
    public required AddressDto ResidentalAddress { get; init; }
    public required AddressDto DefaultAdvertisementsPickupAddress { get; init; }

    public sealed class AddressDto
    {
        public required string Country { get; init; }
        public required string? State { get; init; }
        public required string ZipCode { get; init; }
        public required string City { get; init; }
        public required string Street { get; init; }
        public required string BuildingNumber { get; init; }
    }
}

[Mapper]
public static partial class PersonResponseMapper
{
    public static partial IQueryable<PersonResponse> ProjectToDto(
        this IQueryable<Person> persons);
    
    [MapperIgnoreSource(nameof(Person.CreatedBy))]
    [MapperIgnoreSource(nameof(Person.CreatedOn))]
    [MapperIgnoreSource(nameof(Person.LastModificationBy))]
    [MapperIgnoreSource(nameof(Person.LastModificationOn))]
    [MapperIgnoreSource(nameof(Person.Cats))]
    [MapperIgnoreSource(nameof(Person.CurrentRole))]
    // ReSharper disable once UnusedMember.Local - Required for mapperly to ignore unused properties.
    private static partial PersonResponse Map(Person person);
}
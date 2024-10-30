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
    public required AddressDto Address { get; init; }
    public required PickupAddressDto DefaultAdvertisementsPickupAddress { get; init; }
    public required ContactInfoDto DefaultAdvertisementsContactInfo { get; init; }

    public sealed class AddressDto
    {
        public required string Country { get; init; }
        public required string? State { get; init; }
        public required string ZipCode { get; init; }
        public required string City { get; init; }
        public required string Street { get; init; }
        public required string BuildingNumber { get; init; }
    }
    public sealed class PickupAddressDto
    {
        public required string Country { get; init; }
        public required string? State { get; init; }
        public required string ZipCode { get; init; }
        public required string City { get; init; }
        public required string? Street { get; init; }
        public required string? BuildingNumber { get; init; }
    }

    public sealed class ContactInfoDto
    {
        public required string Email { get; init; }
        public required string PhoneNumber { get; init; }
    }
}

[Mapper]
public static partial class PersonResponseMapper
{
    public static partial IQueryable<PersonResponse> ProjectToDto(
        this IQueryable<Person> persons);
}
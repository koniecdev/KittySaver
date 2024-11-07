using KittySaver.Api.Shared.Domain.Entites;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Api.Features.Advertisements.SharedContracts;

public sealed class AdvertisementResponse
{
    public required Guid Id { get; init; }
    public required Guid PersonId { get; init; }
    public required string PersonName { get; init; }
    public required string Title { get; init; }
    public required double PriorityScore { get; init; }
    public required string? Description { get; init; }
    public required PickupAddressDto PickupAddress { get; init; }
    public required ContactInfoDto ContactInfo { get; init; }
    
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
public static partial class AdvertisementResponseMapper
{

}
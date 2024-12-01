using KittySaver.Domain.Advertisements;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Api.Features.Advertisements.SharedContracts;

public sealed class AdvertisementResponse
{
    public required Guid Id { get; init; }
    public required Guid PersonId { get; init; }
    public string PersonName { get; set; } = "";
    public string Title => string.Join(", ", Cats.Select(c => c.Name));
    public required double PriorityScore { get; init; }
    public required string? Description { get; init; }
    public required string ContactInfoEmail { get; init; }
    public required string ContactInfoPhoneNumber { get; init; }
    public required AdvertisementStatus Status { get; init; }
    public ICollection<CatDto> Cats { get; set; } = new List<CatDto>();
    public required PickupAddressDto PickupAddress { get; init; }
    
    public sealed class PickupAddressDto
    {
        public required string Country { get; init; }
        public required string? State { get; init; }
        public required string ZipCode { get; init; }
        public required string City { get; init; }
        public required string Street { get; init; }
        public required string BuildingNumber { get; init; }
    }

    public sealed class CatDto
    {
        public required Guid Id { get; init; }
        public required string Name { get; init; }
    }
    
    public enum AdvertisementStatus
    {
        Draft,
        Active,
        Closed,
        Expired
    }
}

[Mapper]
public static partial class AdvertisementStatusMapper
{
    public static partial AdvertisementResponse.AdvertisementStatus MapStatus(Advertisement.AdvertisementStatus status);
}
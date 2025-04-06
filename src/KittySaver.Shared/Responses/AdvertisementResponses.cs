using KittySaver.Shared.Common;
using KittySaver.Shared.Common.Enums;
using KittySaver.Shared.Hateoas;
using KittySaver.Shared.TypedIds;

namespace KittySaver.Shared.Responses;

public sealed class AdvertisementResponse : IHateoasAdvertisementResponse
{
    public required AdvertisementId Id { get; init; }
    public required PersonId PersonId { get; init; }
    public required string PersonName { get; init; }
    public string Title => string.Join(", ", Cats.Select(c => c.Name));
    public required double PriorityScore { get; init; }
    public required string? Description { get; init; }
    public required string ContactInfoEmail { get; init; }
    public required string ContactInfoPhoneNumber { get; init; }
    public required AdvertisementStatus Status { get; init; }
    public required ICollection<CatDto> Cats { get; init; }
    public required PickupAddressDto PickupAddress { get; init; }
    public ICollection<Link> Links { get; set; } = new List<Link>();

    
    public sealed class PickupAddressDto
    {
        public required string Country { get; init; }
        public required string? State { get; init; }
        public required string ZipCode { get; init; }
        public required string City { get; init; }
        public required string? Street { get; init; }
        public required string? BuildingNumber { get; init; }
    }

    public sealed class CatDto
    {
        public required CatId Id { get; init; }
        public required string Name { get; init; }
    }
}
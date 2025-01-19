namespace KittySaver.Wasm.Shared.HttpClients.MainApiResponses;

public sealed class AdvertisementResponse
{
    public required Guid Id { get; init; }
    public required Guid PersonId { get; init; }
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
        public required Guid Id { get; init; }
        public required string Name { get; init; }
    }
    
    public enum AdvertisementStatus
    {
        Active,
        Closed,
        Expired,
        ThumbnailNotUploaded
    }
}
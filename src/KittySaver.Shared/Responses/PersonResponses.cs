using KittySaver.Shared.Hateoas;

namespace KittySaver.Shared.Responses;

public sealed class PersonResponse : IHateoasPersonResponse
{
    public required Guid Id { get; init; }
    public required Guid UserIdentityId { get; init; }
    public required DateTimeOffset CreatedOn { get; init; }
    public required string Nickname { get; init; }
    public required string Email { get; init; }
    public required string PhoneNumber { get; init; }
    public required string DefaultAdvertisementsContactInfoEmail { get; init; }
    public required string DefaultAdvertisementsContactInfoPhoneNumber { get; init; }
    public required AddressDto DefaultAdvertisementsPickupAddress { get; init; }
    public ICollection<Link> Links { get; set; } = new List<Link>();

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
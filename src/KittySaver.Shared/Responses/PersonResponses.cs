using KittySaver.Shared.Hateoas;
using KittySaver.Shared.TypedIds;

namespace KittySaver.Shared.Responses;

public sealed class PersonResponse : IHateoasPersonResponse
{
    public required PersonId Id { get; init; }
    public required Guid UserIdentityId { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
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
using KittySaver.Shared.TypedIds;

// ReSharper disable CollectionNeverUpdated.Global

namespace KittySaver.ReadModels.PersonAggregate;

public sealed class PersonReadModel
{
    public required PersonId Id { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required int CurrentRole { get; init; }
    public required Guid UserIdentityId { get; init; }
    public required string DefaultAdvertisementsContactInfoEmail { get; init; }
    public required string DefaultAdvertisementsContactInfoPhoneNumber { get; init; }
    public required string DefaultAdvertisementsPickupAddressBuildingNumber { get; init; }
    public required string DefaultAdvertisementsPickupAddressCity { get; init; }
    public required string DefaultAdvertisementsPickupAddressCountry { get; init; }
    public required string? DefaultAdvertisementsPickupAddressState { get; init; }
    public required string DefaultAdvertisementsPickupAddressStreet { get; init; }
    public required string DefaultAdvertisementsPickupAddressZipCode { get; init; }
    public required string Email { get; init; }
    public required string Nickname { get; init; }
    public required string PhoneNumber { get; init; }
    public ICollection<CatReadModel> Cats { get; } = new List<CatReadModel>();
    public ICollection<AdvertisementReadModel> Advertisements { get; } = new List<AdvertisementReadModel>();
}
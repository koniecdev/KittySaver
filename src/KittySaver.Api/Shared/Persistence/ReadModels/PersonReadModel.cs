using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
// ReSharper disable CollectionNeverUpdated.Global

namespace KittySaver.Api.Shared.Persistence.ReadModels;

public sealed class PersonReadModel
{
    public required Guid Id { get; init; }
    public required string CreatedBy { get; init; }
    public required DateTimeOffset CreatedOn { get; init; }
    public required string? LastModificationBy { get; init; }
    public required DateTimeOffset? LastModificationOn { get; init; }
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
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string PhoneNumber { get; init; }
    public required string ResidentalAddressBuildingNumber { get; init; }
    public required string ResidentalAddressCity { get; init; }
    public required string ResidentalAddressCountry { get; init; }
    public required string? ResidentalAddressState { get; init; }
    public required string ResidentalAddressStreet { get; init; }
    public required string ResidentalAddressZipCode { get; init; }
    public ICollection<CatReadModel> Cats { get; } = new List<CatReadModel>();
    public ICollection<AdvertisementReadModel> Advertisements { get; } = new List<AdvertisementReadModel>();
}
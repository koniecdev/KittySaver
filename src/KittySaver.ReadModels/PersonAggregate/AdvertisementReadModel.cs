using KittySaver.Shared.Common.Enums;
using KittySaver.Shared.TypedIds;

// ReSharper disable CollectionNeverUpdated.Global

namespace KittySaver.ReadModels.PersonAggregate;

public sealed class AdvertisementReadModel
{
    public required AdvertisementId Id { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset? ClosedOn { get; init; }
    public required DateTimeOffset ExpiresOn { get; init; }
    public required AdvertisementStatus Status { get; init; }
    public required double PriorityScore { get; init; }
    public required string ContactInfoEmail { get; init; }
    public required string ContactInfoPhoneNumber { get; init; }
    public required string Description { get; init; }
    public required string? PickupAddressBuildingNumber { get; init; }
    public required string PickupAddressCity { get; init; }
    public required string PickupAddressCountry { get; init; }
    public required string? PickupAddressState { get; init; }
    public required string? PickupAddressStreet { get; init; }
    public required string PickupAddressZipCode { get; init; }
    public required PersonId PersonId { get; init; }
    public PersonReadModel Person { get; private set; } = null!;
    public ICollection<CatReadModel> Cats { get; } = new List<CatReadModel>();
}
namespace KittySaver.Api.Shared.Persistence.ReadModels;

public sealed record AdvertisementReadModel(
    Guid Id,
    string CreatedBy,
    DateTimeOffset CreatedOn,
    string? LastModificationBy,
    DateTimeOffset? LastModificationOn,
    DateTimeOffset? ClosedOn,
    DateTimeOffset ExpiresOn,
    int Status,
    double PriorityScore,
    Guid PersonId,
    PersonReadModel Person,
    ICollection<CatReadModel> Cats,
    string ContactInfoEmail,
    string ContactInfoPhoneNumber,
    string Description,
    string PickupAddressBuildingNumber,
    string PickupAddressCity,
    string PickupAddressCountry,
    string PickupAddressState,
    string PickupAddressStreet,
    string PickupAddressZipCode
);
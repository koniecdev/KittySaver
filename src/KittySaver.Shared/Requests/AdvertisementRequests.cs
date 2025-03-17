namespace KittySaver.Shared.Requests;

public sealed record CreateAdvertisementRequest(
    IEnumerable<Guid> CatsIdsToAssign,
    string? Description,
    string PickupAddressCountry,
    string? PickupAddressState,
    string PickupAddressZipCode,
    string PickupAddressCity,
    string? PickupAddressStreet,
    string? PickupAddressBuildingNumber,
    string ContactInfoEmail,
    string ContactInfoPhoneNumber
);

public sealed record UpdateAdvertisementRequest(
    string? Description,
    string PickupAddressCountry,
    string? PickupAddressState,
    string PickupAddressZipCode,
    string PickupAddressCity,
    string? PickupAddressStreet,
    string? PickupAddressBuildingNumber,
    string ContactInfoEmail,
    string ContactInfoPhoneNumber);

public sealed record ReassignCatsToAdvertisementRequest(IEnumerable<Guid> CatIds);

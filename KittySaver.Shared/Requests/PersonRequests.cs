namespace KittySaver.Shared.Requests;

public sealed record CreatePersonRequest(
    string Nickname,
    string Email,
    string PhoneNumber,
    string Password,
    string DefaultAdvertisementPickupAddressCountry,
    string? DefaultAdvertisementPickupAddressState,
    string DefaultAdvertisementPickupAddressZipCode,
    string DefaultAdvertisementPickupAddressCity,
    string? DefaultAdvertisementPickupAddressStreet,
    string? DefaultAdvertisementPickupAddressBuildingNumber,
    string DefaultAdvertisementContactInfoEmail,
    string DefaultAdvertisementContactInfoPhoneNumber);

public sealed record UpdatePersonRequest(
    string Nickname,
    string Email,
    string PhoneNumber,
    string DefaultAdvertisementPickupAddressCountry,
    string? DefaultAdvertisementPickupAddressState,
    string DefaultAdvertisementPickupAddressZipCode,
    string DefaultAdvertisementPickupAddressCity,
    string? DefaultAdvertisementPickupAddressStreet,
    string? DefaultAdvertisementPickupAddressBuildingNumber,
    string DefaultAdvertisementContactInfoEmail,
    string DefaultAdvertisementContactInfoPhoneNumber);
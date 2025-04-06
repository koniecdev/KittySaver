using KittySaver.Api.Shared.Persistence.ReadModels;
using KittySaver.Shared.Responses;

namespace KittySaver.Api.Features.Persons.SharedContracts;

public static class PersonMapper
{
    public static IQueryable<PersonResponse> ProjectToDto(
        this IQueryable<PersonReadModel> persons) =>
        persons.Select(entity => new PersonResponse
        {
            Id = entity.Id,
            UserIdentityId = entity.UserIdentityId,
            Nickname = entity.Nickname,
            Email = entity.Email,
            PhoneNumber = entity.PhoneNumber,
            CreatedAt = entity.CreatedAt,
            DefaultAdvertisementsContactInfoEmail = entity.DefaultAdvertisementsContactInfoEmail,
            DefaultAdvertisementsContactInfoPhoneNumber = entity.DefaultAdvertisementsContactInfoPhoneNumber,
            DefaultAdvertisementsPickupAddress = new PersonResponse.AddressDto
            {
                Country = entity.DefaultAdvertisementsPickupAddressCountry,
                State = entity.DefaultAdvertisementsPickupAddressState,
                ZipCode = entity.DefaultAdvertisementsPickupAddressZipCode,
                City = entity.DefaultAdvertisementsPickupAddressCity,
                Street = entity.DefaultAdvertisementsPickupAddressStreet,
                BuildingNumber = entity.DefaultAdvertisementsPickupAddressBuildingNumber
            }
        });
}
using KittySaver.Api.Shared.Persistence.ReadModels;
using KittySaver.Shared.Responses;

namespace KittySaver.Api.Features.Advertisements.SharedContracts;

public static class AdvertisementMapper
{
    public static IQueryable<AdvertisementResponse> ProjectToDto(
        this IQueryable<AdvertisementReadModel> persons) =>
        persons.Select(entity => new AdvertisementResponse
        {
            Id = entity.Id,
            ContactInfoEmail = entity.ContactInfoEmail,
            ContactInfoPhoneNumber = entity.ContactInfoPhoneNumber,
            PriorityScore = entity.PriorityScore,
            Description = entity.Description,
            PersonId = entity.PersonId,
            PersonName = entity.Person.Nickname,
            Cats = entity.Cats.Select(cat => new AdvertisementResponse.CatDto
            {
                Id = cat.Id,
                Name = cat.Name
            }).ToList(),
            Status = entity.Status,
            PickupAddress = new AdvertisementResponse.PickupAddressDto
            {
                BuildingNumber = entity.PickupAddressBuildingNumber,
                City = entity.PickupAddressCity,
                Country = entity.PickupAddressCountry,
                State = entity.PickupAddressState,
                Street = entity.PickupAddressStreet,
                ZipCode = entity.PickupAddressZipCode
            }
        });
}
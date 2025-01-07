using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Hateoas;
using KittySaver.Api.Shared.Persistence.ReadModels;
using KittySaver.Domain.Persons;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Api.Features.Advertisements.SharedContracts;

public sealed class AdvertisementResponse : IHateoasAdvertisementResponse
{
    public required Guid Id { get; init; }
    public required Guid PersonId { get; init; }
    public required string PersonName { get; init; }
    public string Title => string.Join(", ", Cats.Select(c => c.Name));
    public required double PriorityScore { get; init; }
    public required string? Description { get; init; }
    public required string ContactInfoEmail { get; init; }
    public required string ContactInfoPhoneNumber { get; init; }
    public required AdvertisementStatus Status { get; init; }
    public required ICollection<CatDto> Cats { get; init; }
    public required PickupAddressDto PickupAddress { get; init; }
    public ICollection<Link> Links { get; set; } = new List<Link>();

    
    public sealed class PickupAddressDto
    {
        public required string Country { get; init; }
        public required string? State { get; init; }
        public required string ZipCode { get; init; }
        public required string City { get; init; }
        public required string? Street { get; init; }
        public required string? BuildingNumber { get; init; }
    }

    public sealed class CatDto
    {
        public required Guid Id { get; init; }
        public required string Name { get; init; }
    }
    
    public enum AdvertisementStatus
    {
        Active,
        Closed,
        Expired
    }
}

[Mapper]
public static partial class AdvertisementStatusMapper
{
    public static partial AdvertisementResponse.AdvertisementStatus MapStatus(Advertisement.AdvertisementStatus status);
}

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
            Status = AdvertisementStatusMapper.MapStatus((Advertisement.AdvertisementStatus)entity.Status),
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
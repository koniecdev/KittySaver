using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Api.Shared.Persistence.ReadModels;
using KittySaver.Domain.Persons;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Api.Features.Persons.SharedContracts;

public sealed class PersonResponse
{
    public required Guid Id { get; init; }
    public required Guid UserIdentityId { get; init; }
    public required string Nickname { get; init; }
    public required string Email { get; init; }
    public required string PhoneNumber { get; init; }
    public required string DefaultAdvertisementsContactInfoEmail { get; init; }
    public required string DefaultAdvertisementsContactInfoPhoneNumber { get; init; }
    public required AddressDto DefaultAdvertisementsPickupAddress { get; init; }
    public required ICollection<Link> Links { get; init; }

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

public static class PersonResponseMapper
{
    private static List<Link> AddLinks(
        PersonReadModel person,
        ILinkService linkService,
        CurrentlyLoggedInPerson? currentlyLoggedInPerson)
    {
        List<Link> links =
        [
            linkService.Generate(
                endpointInfo: EndpointNames.GetPerson,
                routeValues: new { id = person.Id },
                isSelf: true),
        ];

        if (currentlyLoggedInPerson is null)
        {
            return links;
        }
        
        bool isLoggedInPersonAnOwner = currentlyLoggedInPerson.PersonId == person.Id;
        if (currentlyLoggedInPerson.Role is not Person.Role.Admin && isLoggedInPersonAnOwner)
        {
            return links;
        }

        links.Add(linkService.Generate(
            endpointInfo: EndpointNames.UpdatePerson,
            routeValues: new { id = person.Id }));

        links.Add(linkService.Generate(
            endpointInfo: EndpointNames.DeletePerson,
            routeValues: new { id = person.Id }));

        links.Add(linkService.Generate(
            endpointInfo: EndpointNames.GetCats,
            routeValues: new { personId = person.Id }));

        links.Add(linkService.Generate(
            endpointInfo: EndpointNames.CreateCat,
            routeValues: new { personId = person.Id }));

        links.Add(linkService.Generate(
            endpointInfo: EndpointNames.GetPersonAdvertisements,
            routeValues: new { personId = person.Id }));

        links.Add(linkService.Generate(
            endpointInfo: EndpointNames.CreateAdvertisement,
            routeValues: new { personId = person.Id }));
        
        return links;
    }
    public static IQueryable<PersonResponse> ProjectToDto(
        this IQueryable<PersonReadModel> persons,
        ILinkService linkService,
        CurrentlyLoggedInPerson? currentlyLoggedInPerson) =>
        persons.Select(entity => new PersonResponse
        {
            Id = entity.Id,
            UserIdentityId = entity.UserIdentityId,
            Nickname = entity.Nickname,
            Email = entity.Email,
            PhoneNumber = entity.PhoneNumber,
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
            },
            Links = AddLinks(entity, linkService, currentlyLoggedInPerson)
        });
}
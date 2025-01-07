using KittySaver.Api.Features.Advertisements.SharedContracts;
using KittySaver.Api.Shared.Endpoints;
using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Domain.Persons;

namespace KittySaver.Api.Shared.Hateoas;

public interface ILinkService
{
    public List<Link> GeneratePersonRelatedLinks(Guid personId, CurrentlyLoggedInPerson? currentlyLoggedInPerson);
    public List<Link> GenerateCatRelatedLinks(Guid id,
        Guid personId,
        Guid? advertisementId,
        CurrentlyLoggedInPerson? currentlyLoggedInPerson);
    public List<Link> GenerateAdvertisementRelatedLinks(Guid id,
        AdvertisementResponse.AdvertisementStatus advertisementStatus,
        Guid personId,
        CurrentlyLoggedInPerson? currentlyLoggedInPerson);
    Link Generate(string endpointName, object? routeValues, string rel, string verb = "GET", bool isTemplated = false);
    List<Link> GeneratePaginationLinks(string endpointName, int? offset, int? limit, Guid? personId);
}

public sealed class LinkService(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor) : ILinkService
{
    public List<Link> GeneratePersonRelatedLinks(
        Guid personId,
        CurrentlyLoggedInPerson? currentlyLoggedInPerson)
    {
        List<Link> links =
        [
            Generate(
                endpointInfo: EndpointNames.GetPerson,
                routeValues: new { id = personId },
                isSelf: true),
        ];

        if (currentlyLoggedInPerson is null)
        {
            return links;
        }
        
        bool isLoggedInPersonAnOwner = currentlyLoggedInPerson.PersonId == personId;
        if (currentlyLoggedInPerson.Role is not Person.Role.Admin && isLoggedInPersonAnOwner)
        {
            return links;
        }

        links.Add(Generate(
            endpointInfo: EndpointNames.UpdatePerson,
            routeValues: new { id = personId }));

        links.Add(Generate(
            endpointInfo: EndpointNames.DeletePerson,
            routeValues: new { id = personId }));

        links.Add(Generate(
            endpointInfo: EndpointNames.GetCats,
            routeValues: new { personId }));

        links.Add(Generate(
            endpointInfo: EndpointNames.CreateCat,
            routeValues: new { personId }));

        links.Add(Generate(
            endpointInfo: EndpointNames.GetPersonAdvertisements,
            routeValues: new { searchTerm = $"personid-eq-{personId}" }));

        links.Add(Generate(
            endpointInfo: EndpointNames.CreateAdvertisement,
            routeValues: new { personId }));
        
        return links;
    }
    
    public List<Link> GenerateCatRelatedLinks(
        Guid id,
        Guid personId,
        Guid? advertisementId,
        CurrentlyLoggedInPerson? currentlyLoggedInPerson)
    {
        List<Link> links =
        [
            Generate(
                endpointInfo: EndpointNames.GetCat,
                routeValues: new { id, personId},
                isSelf: true),
        ];

        if (currentlyLoggedInPerson is null)
        {
            return links;
        }

        bool isLoggedInPersonAnOwner = currentlyLoggedInPerson.PersonId == personId;
        if (currentlyLoggedInPerson.Role is not Person.Role.Admin && isLoggedInPersonAnOwner)
        {
            return links;
        }
        
        links.Add(Generate(
            endpointInfo: EndpointNames.UpdateCat,
            routeValues: new { id, personId}));

        links.Add(Generate(
            endpointInfo: EndpointNames.DeleteCat,
            routeValues: new { id, personId }));
            
        if (advertisementId is not null)
        {
            links.Add(Generate(
                endpointInfo: EndpointNames.GetAdvertisement,
                routeValues: new { id = advertisementId }));
        }

        return links;
    }
    
    public List<Link> GenerateAdvertisementRelatedLinks(
        Guid id,
        AdvertisementResponse.AdvertisementStatus advertisementStatus,
        Guid personId,
        CurrentlyLoggedInPerson? currentlyLoggedInPerson)
    {
        List<Link> links =
        [
            Generate(
                endpointInfo: EndpointNames.GetAdvertisement,
                routeValues: new { id },
                isSelf: true)
        ];

        if (currentlyLoggedInPerson?.Role is not Person.Role.Admin && currentlyLoggedInPerson?.PersonId != personId)
        {
            return links;
        }

        switch (advertisementStatus)
        {
            case AdvertisementResponse.AdvertisementStatus.Active:
            {
                links.Add(Generate(
                    endpointInfo: EndpointNames.UpdateAdvertisement,
                    routeValues: new { id, personId }));

                links.Add(Generate(
                    endpointInfo: EndpointNames.DeleteAdvertisement,
                    routeValues: new { id, personId }));
            
                links.Add(Generate(
                    endpointInfo: EndpointNames.ReassignCatsToAdvertisement,
                    routeValues: new { id, personId }));
            
                links.Add(Generate(
                    endpointInfo: EndpointNames.CloseAdvertisement,
                    routeValues: new { id, personId }));
            
                if (currentlyLoggedInPerson.Role is Person.Role.Job or Person.Role.Admin)
                {
                    links.Add(Generate(
                        endpointInfo: EndpointNames.ExpireAdvertisement,
                        routeValues: new { id, personId }));
                }

                break;
            }
            case AdvertisementResponse.AdvertisementStatus.Expired:
                links.Add(Generate(
                    endpointInfo: EndpointNames.RefreshAdvertisement,
                    routeValues: new { id, personId }));
                break;
            case AdvertisementResponse.AdvertisementStatus.Closed:
            default:
                break;
        }

        return links;
    }
    
    private Link Generate(EndpointInfo endpointInfo, object? routeValues = null, bool isSelf = false, bool isTemplated = false)
    {
        string href = isTemplated 
                ? $"{linkGenerator.GetUriByName(httpContextAccessor.HttpContext!, endpointInfo.EndpointName)!}"
                : linkGenerator.GetUriByName(httpContextAccessor.HttpContext!, endpointInfo.EndpointName, routeValues)!;
        Link link = new Link(
            href,
            isSelf ? EndpointNames.SelfRel : endpointInfo.Rel,
            endpointInfo.Verb,
            isTemplated);
        return link;
    }

    public Link Generate(string endpointName, object? routeValues, string rel, string verb = "GET", bool isTemplated = false)
    {
        string href = linkGenerator.GetUriByName(httpContextAccessor.HttpContext!, endpointName, routeValues)!;
        Link link = new Link(
            href,
            rel,
            verb,
            isTemplated);
        return link;
    }
    
    public List<Link> GeneratePaginationLinks(string endpointName, int? offset, int? limit, Guid? personId)
    {
        List<Link> links = [];
        string? href = linkGenerator.GetUriByName(httpContextAccessor.HttpContext!, endpointName, new {personId});
        if (limit is not null)
        {
            links.Add(new Link(
                $"{href}?offset={{offset}}&limit={limit}",
                "by-offset",
                "GET",
                true));
        }

        if (offset is not null)
        {
            links.Add(new Link(
                $"{href}?offset={offset}&limit={{limit}}",
                "by-limit",
                "GET",
                true));
        }
        
        links.Add(new Link(
            $"{href}?offset={{offset}}&limit={{limit}}",
            "by-page",
            "GET",
            true));
        return links;
    }
}
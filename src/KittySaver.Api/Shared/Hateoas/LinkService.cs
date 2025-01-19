using KittySaver.Api.Features.Advertisements;
using KittySaver.Api.Features.Advertisements.SharedContracts;
using KittySaver.Api.Shared.Endpoints;
using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Domain.Persons;
using Microsoft.AspNetCore.Routing.Patterns;

namespace KittySaver.Api.Shared.Hateoas;

public interface ILinkService
{
    public List<Link> GeneratePersonRelatedLinks(Guid personId, CurrentlyLoggedInPerson? currentlyLoggedInPerson);

    public List<Link> GenerateCatRelatedLinks(Guid id,
        Guid personId,
        Guid? advertisementId,
        CurrentlyLoggedInPerson? currentlyLoggedInPerson);

    public List<Link> GenerateAdvertisementRelatedLinks(Guid id,
        Advertisement.AdvertisementStatus advertisementStatus,
        Guid personId,
        CurrentlyLoggedInPerson? currentlyLoggedInPerson,
        bool doesRequestRequireAuthorization);

    Link Generate(string endpointName, object? routeValues, string rel, string verb = "GET", bool isTemplated = false);
    List<Link> GeneratePaginationLinks(string endpointName, int? offset, int? limit, Guid? personId);
    List<Link> GenerateApiDiscoveryV1Links(Guid? personId);
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
                routeValues: new { id, personId },
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
            routeValues: new { id, personId }));

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
        Advertisement.AdvertisementStatus advertisementStatus,
        Guid personId,
        CurrentlyLoggedInPerson? currentlyLoggedInPerson,
        bool doesRequestRequireAuthorization)
    {
        return doesRequestRequireAuthorization ? GetPersonAdvertisements() : GetPublicActiveAdvertisements();

        List<Link> GetPublicActiveAdvertisements()
        {
            List<Link> links =
            [
                Generate(endpointInfo: EndpointNames.GetPublicAdvertisement,
                    routeValues: new { id },
                    isSelf: true),
                Generate(endpointInfo: EndpointNames.GetAdvertisementThumbnail,
                    routeValues: new { id })
            ];
            return links;
        }
        
        List<Link> GetPersonAdvertisements()
        {
            List<Link> links =
            [
                Generate(endpointInfo: EndpointNames.GetAdvertisement,
                        routeValues: new { id, personId },
                        isSelf: true)
            ];

            if (currentlyLoggedInPerson?.Role is not Person.Role.Admin && currentlyLoggedInPerson?.PersonId != personId)
            {
                return links;
            }

            switch (advertisementStatus)
            {
                case Advertisement.AdvertisementStatus.Active:
                    links.Add(Generate(
                        endpointInfo: EndpointNames.UpdateAdvertisement,
                        routeValues: new { id, personId }));
                    
                    links.Add(Generate(endpointInfo: EndpointNames.GetAdvertisementThumbnail,
                        routeValues: new { id }));
                    
                    links.Add(Generate(
                        endpointInfo: EndpointNames.DeleteAdvertisement,
                        routeValues: new { id, personId }));

                    links.Add(Generate(
                        endpointInfo: EndpointNames.ReassignCatsToAdvertisement,
                        routeValues: new { id, personId }));

                    links.Add(Generate(
                        endpointInfo: EndpointNames.UpdateAdvertisementThumbnail,
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

                case Advertisement.AdvertisementStatus.Expired:
                    links.Add(Generate(
                        endpointInfo: EndpointNames.RefreshAdvertisement,
                        routeValues: new { id, personId }));
                    
                    links.Add(Generate(endpointInfo: EndpointNames.GetAdvertisementThumbnail,
                        routeValues: new { id }));
                    
                    links.Add(Generate(
                        endpointInfo: EndpointNames.DeleteAdvertisement,
                        routeValues: new { id, personId }));
                    
                    break;

                case Advertisement.AdvertisementStatus.ThumbnailNotUploaded:
                    links.Add(Generate(
                        endpointInfo: EndpointNames.UpdateAdvertisementThumbnail,
                        routeValues: new { id, personId }));
                    
                    links.Add(Generate(
                        endpointInfo: EndpointNames.UpdateAdvertisement,
                        routeValues: new { id, personId }));

                    links.Add(Generate(
                        endpointInfo: EndpointNames.DeleteAdvertisement,
                        routeValues: new { id, personId }));

                    links.Add(Generate(
                        endpointInfo: EndpointNames.ReassignCatsToAdvertisement,
                        routeValues: new { id, personId }));
                    break;
                
                case Advertisement.AdvertisementStatus.Closed:
                    links.Add(Generate(endpointInfo: EndpointNames.GetAdvertisementThumbnail,
                        routeValues: new { id }));
                    break;
            }

            return links;
        }
    }

    private Link Generate(EndpointInfo endpointInfo, object? routeValues = null, bool isSelf = false,
        bool isTemplated = false)
    {
        string href = linkGenerator.GetUriByName(
            httpContextAccessor.HttpContext!,
            endpointInfo.EndpointName,
            routeValues)!;

        if (isTemplated)
        {
            href = href
                .Replace(UrlPlaceholders.Id.ToString(), "{id}")
                .Replace(UrlPlaceholders.PersonId.ToString(), "{personId}");
        }

        Link link = new Link(
            href,
            isSelf ? EndpointNames.SelfRel : endpointInfo.Rel,
            endpointInfo.Verb,
            isTemplated);
        return link;
    }

    public Link Generate(string endpointName, object? routeValues, string rel, string verb = "GET",
        bool isTemplated = false)
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
        string? href = linkGenerator.GetUriByName(httpContextAccessor.HttpContext!, endpointName, new { personId });
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

    public List<Link> GenerateApiDiscoveryV1Links(Guid? personId)
    {
        List<Link> links =
        [
            Generate(endpointInfo: EndpointNames.GetApiDiscoveryV1,
                routeValues: null,
                isSelf: true),

            Generate(endpointInfo: EndpointNames.GetPublicAdvertisements,
                routeValues: null),

            Generate(endpointInfo: EndpointNames.GetPublicAdvertisement,
                routeValues: new { id = UrlPlaceholders.Id },
                isTemplated: true),

            Generate(endpointInfo: EndpointNames.GetPublicPersonAdvertisements,
                routeValues: new { searchTerm = $"personid-eq-{UrlPlaceholders.Id}" },
                isTemplated: true)
        ];

        if (personId is null)
        {
            return links;
        }

        links.Add(Generate(
            endpointInfo: EndpointNames.GetCats,
            routeValues: new { personId = personId.Value }));

        links.Add(Generate(
            endpointInfo: EndpointNames.CreateCat,
            routeValues: new { personId = personId.Value }));

        links.Add(Generate(
            endpointInfo: EndpointNames.CreateAdvertisement,
            routeValues: new { personId = personId.Value }));

        return links;
    }
}

public static class UrlPlaceholders
{
    public static readonly Guid Id = Guid.Empty;
    public static readonly Guid PersonId = Guid.Parse("11111111-1111-1111-1111-111111111111");
}
using KittySaver.Api.Infrastructure.Endpoints;
using KittySaver.Api.Infrastructure.Services;
using KittySaver.Shared.Common.Enums;
using KittySaver.Shared.Hateoas;
using KittySaver.Shared.TypedIds;

namespace KittySaver.Api.Hateoas;

public interface ILinkService
{
    public List<Link> GeneratePersonRelatedLinks(PersonId personId, CurrentlyLoggedInPerson? currentlyLoggedInPerson);

    public List<Link> GenerateCatRelatedLinks(
        CatId catId,
        PersonId personId,
        AdvertisementId? advertisementId,
        bool isThumbnailUploaded,
        bool isAdopted,
        CurrentlyLoggedInPerson? currentlyLoggedInPerson);

    public List<Link> GenerateAdvertisementRelatedLinks(
        AdvertisementId advertisementId,
        AdvertisementStatus advertisementStatus,
        PersonId personId,
        CurrentlyLoggedInPerson? currentlyLoggedInPerson,
        bool doesRequestRequireAuthorization);

    Link Generate(string endpointName, object? routeValues, string rel, string verb = "GET", bool isTemplated = false);
    List<Link> GeneratePaginationLinks(string endpointName, int? offset, int? limit, PersonId? personId);
    List<Link> GenerateApiDiscoveryV1Links(PersonId? personId);
}

public sealed class LinkService(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor) : ILinkService
{
    public List<Link> GeneratePersonRelatedLinks(
        PersonId personId,
        CurrentlyLoggedInPerson? currentlyLoggedInPerson)
    {
        List<Link> links =
        [
            Generate(
                endpointInfo: EndpointNames.Persons.GetById,
                routeValues: new { id = personId },
                isSelf: true),
        ];

        if (currentlyLoggedInPerson is null)
        {
            return links;
        }

        bool isLoggedInPersonAnOwner = currentlyLoggedInPerson.PersonId == personId;
        if (currentlyLoggedInPerson.Role is not PersonRole.Admin && isLoggedInPersonAnOwner)
        {
            return links;
        }

        links.Add(Generate(
            endpointInfo: EndpointNames.Persons.Update,
            routeValues: new { id = personId }));

        links.Add(Generate(
            endpointInfo: EndpointNames.Persons.Delete,
            routeValues: new { id = personId }));

        links.Add(Generate(
            endpointInfo: EndpointNames.Cats.GetAll,
            routeValues: new { personId }));

        links.Add(Generate(
            endpointInfo: EndpointNames.Cats.Create,
            routeValues: new { personId }));

        links.Add(Generate(
            endpointInfo: EndpointNames.Advertisements.GetAll,
            routeValues: new { personId }));

        links.Add(Generate(
            endpointInfo: EndpointNames.Advertisements.Create,
            routeValues: new { personId }));

        return links;
    }

    public List<Link> GenerateCatRelatedLinks(
        CatId catId,
        PersonId personId,
        AdvertisementId? advertisementId,
        bool isThumbnailUploaded,
        bool isAdopted,
        CurrentlyLoggedInPerson? currentlyLoggedInPerson)
    {
        List<Link> links =
        [
            Generate(endpointInfo: EndpointNames.Cats.GetById,
                    routeValues: new { id = catId, personId },
                    isSelf: true),
            Generate(endpointInfo: EndpointNames.Cats.GetGallery,
                    routeValues: new { id = catId, personId }),
            Generate(endpointInfo: EndpointNames.Cats.GetGalleryPicture,
                routeValues: new { id = UrlPlaceholders.Id, personId = UrlPlaceholders.PersonId, filename = UrlPlaceholders.Filename },
                isTemplated: true)
        ];

        if (isThumbnailUploaded)
        {
            links.Add(Generate(endpointInfo: EndpointNames.Cats.GetThumbnail,
                routeValues: new { id = catId, personId }));
        }
        
        if (currentlyLoggedInPerson is null)
        {
            return links;
        }

        if (currentlyLoggedInPerson.PersonId != personId && currentlyLoggedInPerson.Role is not PersonRole.Admin)
        {
            return links;
        }

        if (!isAdopted)
        {
            links.Add(Generate(
                endpointInfo: EndpointNames.Cats.UpdateThumbnail,
                routeValues: new { id = catId, personId }));
            links.Add(Generate(endpointInfo: EndpointNames.Cats.AddPictures,
                routeValues: new { id = catId, personId }));
            links.Add(Generate(endpointInfo: EndpointNames.Cats.RemovePicture,
                routeValues: new
                {
                    id = UrlPlaceholders.Id, personId = UrlPlaceholders.PersonId, filename = UrlPlaceholders.Filename
                },
                isTemplated: true));
            links.Add(Generate(
                endpointInfo: EndpointNames.Cats.Update,
                routeValues: new { id = catId, personId }));
        }
        
        
        if (advertisementId is null)
        {
            links.Add(Generate(
                endpointInfo: EndpointNames.Cats.Delete,
                routeValues: new { id = catId, personId }));
            return links;
        }
        
        links.Add(Generate(
            endpointInfo: EndpointNames.Advertisements.GetById,
            routeValues: new { id = advertisementId.Value, personId }));
        
        return links;
    }

    public List<Link> GenerateAdvertisementRelatedLinks(
        AdvertisementId id,
        AdvertisementStatus advertisementStatus,
        PersonId personId,
        CurrentlyLoggedInPerson? currentlyLoggedInPerson,
        bool doesRequestRequireAuthorization)
    {
        return doesRequestRequireAuthorization ? GetPersonAdvertisements() : GetPublicActiveAdvertisements();

        List<Link> GetPublicActiveAdvertisements()
        {
            List<Link> links =
            [
                Generate(endpointInfo: EndpointNames.Advertisements.GetPublicById,
                    routeValues: new { id },
                    isSelf: true),
                Generate(endpointInfo: EndpointNames.Advertisements.GetThumbnail,
                    routeValues: new { id }),
                Generate(endpointInfo: EndpointNames.Advertisements.GetAdvertisementCats,
                    routeValues: new { id, personId })
            ];
            return links;
        }
        
        List<Link> GetPersonAdvertisements()
        {
            List<Link> links =
            [
                Generate(endpointInfo: EndpointNames.Advertisements.GetById,
                        routeValues: new { id, personId },
                        isSelf: true)
            ];

            if (currentlyLoggedInPerson?.Role is not PersonRole.Admin && currentlyLoggedInPerson?.PersonId != personId)
            {
                return links;
            }

            switch (advertisementStatus)
            {
                case AdvertisementStatus.Active:
                    links.Add(Generate(
                        endpointInfo: EndpointNames.Advertisements.Update,
                        routeValues: new { id, personId }));
                    
                    links.Add(Generate(endpointInfo: EndpointNames.Advertisements.GetThumbnail,
                        routeValues: new { id }));
                    
                    links.Add(Generate(
                        endpointInfo: EndpointNames.Advertisements.Delete,
                        routeValues: new { id, personId }));

                    links.Add(Generate(
                        endpointInfo: EndpointNames.Advertisements.ReassignCats,
                        routeValues: new { id, personId }));
                    
                    links.Add(Generate(
                        endpointInfo: EndpointNames.Advertisements.GetAdvertisementCats,
                        routeValues: new { id, personId }));
                    
                    links.Add(Generate(
                        endpointInfo: EndpointNames.Advertisements.UpdateThumbnail,
                        routeValues: new { id, personId }));

                    links.Add(Generate(
                        endpointInfo: EndpointNames.Advertisements.Close,
                        routeValues: new { id, personId }));

                    if (currentlyLoggedInPerson.Role is PersonRole.Job or PersonRole.Admin)
                    {
                        links.Add(Generate(
                            endpointInfo: EndpointNames.Advertisements.Expire,
                            routeValues: new { id, personId }));
                    }

                    break;

                case AdvertisementStatus.Expired:
                    links.Add(Generate(
                        endpointInfo: EndpointNames.Advertisements.Refresh,
                        routeValues: new { id, personId }));
                    
                    links.Add(Generate(endpointInfo: EndpointNames.Advertisements.GetThumbnail,
                        routeValues: new { id }));
                    
                    links.Add(Generate(
                        endpointInfo: EndpointNames.Advertisements.Delete,
                        routeValues: new { id, personId }));
                    
                    break;

                case AdvertisementStatus.ThumbnailNotUploaded:
                    links.Add(Generate(
                        endpointInfo: EndpointNames.Advertisements.UpdateThumbnail,
                        routeValues: new { id, personId }));
                    
                    links.Add(Generate(
                        endpointInfo: EndpointNames.Advertisements.Update,
                        routeValues: new { id, personId }));

                    links.Add(Generate(
                        endpointInfo: EndpointNames.Advertisements.Delete,
                        routeValues: new { id, personId }));

                    links.Add(Generate(
                        endpointInfo: EndpointNames.Advertisements.ReassignCats,
                        routeValues: new { id, personId }));
                    
                    links.Add(Generate(
                        endpointInfo: EndpointNames.Advertisements.GetAdvertisementCats,
                        routeValues: new { id, personId }));
                    break;
                
                case AdvertisementStatus.Closed:
                    links.Add(Generate(endpointInfo: EndpointNames.Advertisements.GetThumbnail,
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
                .Replace(UrlPlaceholders.PersonId.ToString(), "{personId}")
                .Replace(UrlPlaceholders.Filename.ToString(), "{filename}");
        }

        Link link = new Link(
            href,
            isSelf ? EndpointRels.SelfRel : endpointInfo.Rel,
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

    public List<Link> GeneratePaginationLinks(string endpointName, int? offset, int? limit, PersonId? personId)
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

    public List<Link> GenerateApiDiscoveryV1Links(PersonId? personId)
    {
        List<Link> links =
        [
            Generate(endpointInfo: EndpointNames.Discovery.GetApiDiscoveryV1,
                routeValues: null,
                isSelf: true),

            Generate(endpointInfo: EndpointNames.Advertisements.GetPublicAll,
                routeValues: null),

            Generate(endpointInfo: EndpointNames.Advertisements.GetPublicById,
                routeValues: new { id = UrlPlaceholders.Id },
                isTemplated: true),

            Generate(endpointInfo: EndpointNames.Advertisements.GetPublicPersonAdvertisements,
                routeValues: new { searchTerm = $"personid-eq-{UrlPlaceholders.Id}" },
                isTemplated: true)
        ];

        if (personId is null)
        {
            links.Add(Generate(endpointInfo: EndpointNames.Persons.Create));
            return links;
        }

        links.Add(Generate(
            endpointInfo: EndpointNames.Persons.GetById,
            routeValues: new { id = personId.Value }));
        
        links.Add(Generate(
            endpointInfo: EndpointNames.Cats.GetAll,
            routeValues: new { personId = personId.Value }));
        
        links.Add(Generate(
            endpointInfo: EndpointNames.Cats.GetById,
            isTemplated: true,
            routeValues: new { personId = UrlPlaceholders.PersonId, id = UrlPlaceholders.Id }));
        
        links.Add(Generate(
            endpointInfo: EndpointNames.Advertisements.GetPersonAdvertisements,
            routeValues: new { personId = personId.Value }));
        
        links.Add(Generate(
            endpointInfo: EndpointNames.Advertisements.GetPersonAdvertisement,
            isTemplated: true,
            routeValues: new { personId = UrlPlaceholders.PersonId, id = UrlPlaceholders.Id }));

        links.Add(Generate(
            endpointInfo: EndpointNames.Cats.Create,
            routeValues: new { personId = personId.Value }));

        links.Add(Generate(
            endpointInfo: EndpointNames.Advertisements.Create,
            routeValues: new { personId = personId.Value }));

        return links;
    }
}

public static class UrlPlaceholders
{
    public static readonly Guid Id = Guid.Empty;
    public static readonly Guid PersonId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid Filename = Guid.Parse("22222222-2222-2222-2222-222222222222");
}
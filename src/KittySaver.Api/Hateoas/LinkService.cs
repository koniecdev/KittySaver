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
                endpointInfo: EndpointNames.GetPerson,
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
            endpointInfo: EndpointNames.GetAdvertisements,
            routeValues: new { personId }));

        links.Add(Generate(
            endpointInfo: EndpointNames.CreateAdvertisement,
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
            Generate(endpointInfo: EndpointNames.GetCat,
                    routeValues: new { id = catId, personId },
                    isSelf: true),
            Generate(endpointInfo: EndpointNames.GetCatGallery,
                    routeValues: new { id = catId, personId }),
            Generate(endpointInfo: EndpointNames.GetCatGalleryPicture,
                routeValues: new { id = UrlPlaceholders.Id, personId = UrlPlaceholders.PersonId, filename = UrlPlaceholders.Filename },
                isTemplated: true)
        ];

        if (isThumbnailUploaded)
        {
            links.Add(Generate(endpointInfo: EndpointNames.GetCatThumbnail,
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
                endpointInfo: EndpointNames.UpdateCatThumbnail,
                routeValues: new { id = catId, personId }));
            links.Add(Generate(endpointInfo: EndpointNames.AddPicturesToCatGallery,
                routeValues: new { id = catId, personId }));
            links.Add(Generate(endpointInfo: EndpointNames.RemovePictureFromCatGallery,
                routeValues: new
                {
                    id = UrlPlaceholders.Id, personId = UrlPlaceholders.PersonId, filename = UrlPlaceholders.Filename
                },
                isTemplated: true));
            links.Add(Generate(
                endpointInfo: EndpointNames.UpdateCat,
                routeValues: new { id = catId, personId }));
        }
        
        
        if (advertisementId is null)
        {
            links.Add(Generate(
                endpointInfo: EndpointNames.DeleteCat,
                routeValues: new { id = catId, personId }));
            return links;
        }
        
        links.Add(Generate(
            endpointInfo: EndpointNames.GetAdvertisement,
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
                Generate(endpointInfo: EndpointNames.GetPublicAdvertisement,
                    routeValues: new { id },
                    isSelf: true),
                Generate(endpointInfo: EndpointNames.GetAdvertisementThumbnail,
                    routeValues: new { id }),
                Generate(endpointInfo: EndpointNames.GetAdvertisementCats,
                    routeValues: new { id, personId })
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

            if (currentlyLoggedInPerson?.Role is not PersonRole.Admin && currentlyLoggedInPerson?.PersonId != personId)
            {
                return links;
            }

            switch (advertisementStatus)
            {
                case AdvertisementStatus.Active:
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
                        endpointInfo: EndpointNames.GetAdvertisementCats,
                        routeValues: new { id, personId }));
                    
                    links.Add(Generate(
                        endpointInfo: EndpointNames.UpdateAdvertisementThumbnail,
                        routeValues: new { id, personId }));

                    links.Add(Generate(
                        endpointInfo: EndpointNames.CloseAdvertisement,
                        routeValues: new { id, personId }));

                    if (currentlyLoggedInPerson.Role is PersonRole.Job or PersonRole.Admin)
                    {
                        links.Add(Generate(
                            endpointInfo: EndpointNames.ExpireAdvertisement,
                            routeValues: new { id, personId }));
                    }

                    break;

                case AdvertisementStatus.Expired:
                    links.Add(Generate(
                        endpointInfo: EndpointNames.RefreshAdvertisement,
                        routeValues: new { id, personId }));
                    
                    links.Add(Generate(endpointInfo: EndpointNames.GetAdvertisementThumbnail,
                        routeValues: new { id }));
                    
                    links.Add(Generate(
                        endpointInfo: EndpointNames.DeleteAdvertisement,
                        routeValues: new { id, personId }));
                    
                    break;

                case AdvertisementStatus.ThumbnailNotUploaded:
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
                    
                    links.Add(Generate(
                        endpointInfo: EndpointNames.GetAdvertisementCats,
                        routeValues: new { id, personId }));
                    break;
                
                case AdvertisementStatus.Closed:
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
            links.Add(Generate(endpointInfo: EndpointNames.CreatePerson));
            return links;
        }

        links.Add(Generate(
            endpointInfo: EndpointNames.GetPerson,
            routeValues: new { id = personId.Value }));
        
        links.Add(Generate(
            endpointInfo: EndpointNames.GetCats,
            routeValues: new { personId = personId.Value }));
        
        links.Add(Generate(
            endpointInfo: EndpointNames.GetCat,
            isTemplated: true,
            routeValues: new { personId = UrlPlaceholders.PersonId, id = UrlPlaceholders.Id }));
        
        links.Add(Generate(
            endpointInfo: EndpointNames.GetPersonAdvertisements,
            routeValues: new { personId = personId.Value }));
        
        links.Add(Generate(
            endpointInfo: EndpointNames.GetPersonAdvertisement,
            isTemplated: true,
            routeValues: new { personId = UrlPlaceholders.PersonId, id = UrlPlaceholders.Id }));

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
    public static readonly Guid Filename = Guid.Parse("22222222-2222-2222-2222-222222222222");
}
using KittySaver.Api.Features.Advertisements.SharedContracts;
using KittySaver.Api.Features.Cats.SharedContracts;
using KittySaver.Api.Features.Persons.SharedContracts;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Hateoas;
using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Api.Shared.Pagination;
using MediatR;

namespace KittySaver.Api.Shared.Behaviours;

public sealed class PagedHateoasBehaviour<TRequest, TResponse>(
    ICurrentUserService currentUserService,
    ILinkService linkService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IPagedQuery
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        TResponse response = await next();

        CurrentlyLoggedInPerson? issuingPerson = await currentUserService.GetCurrentlyLoggedInPersonAsync(cancellationToken);

        switch (response)
        {
            case IPagedList<PersonResponse> personPagedResponse:
                foreach (PersonResponse personResponse in personPagedResponse.Items)
                {
                    personResponse.Links = linkService.GeneratePersonRelatedLinks(
                        personResponse.Id,
                        issuingPerson);
                }
                break;
            
            case IPagedList<CatResponse> catPagedResponse:
                foreach (CatResponse catResponse in catPagedResponse.Items)
                {
                    catResponse.Links = linkService.GenerateCatRelatedLinks(
                        catResponse.Id,
                        catResponse.PersonId,
                        catResponse.AdvertisementId,
                        issuingPerson);
                }
                break;
            
            case IPagedList<AdvertisementResponse> advertisementPagedResponse:
                foreach (AdvertisementResponse advertisementResponse in advertisementPagedResponse.Items)
                {
                    advertisementResponse.Links = linkService.GenerateAdvertisementRelatedLinks(
                        advertisementResponse.Id,
                        advertisementResponse.Status,
                        advertisementResponse.PersonId,
                        issuingPerson);
                }
                break;
        }

        return response;
    }
}
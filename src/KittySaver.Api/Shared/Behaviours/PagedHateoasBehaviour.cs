using KittySaver.Api.Features.Advertisements.SharedContracts;
using KittySaver.Api.Features.Cats.SharedContracts;
using KittySaver.Api.Features.Persons.SharedContracts;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Infrastructure.Services;
using MediatR;

namespace KittySaver.Api.Shared.Behaviours;

public sealed class PagedHateoasBehaviour<TRequest, TResponse>(
    ICurrentUserService currentUserService,
    ILinkService linkService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IQuery<IPagedList<IHateoasResponse>>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        TResponse genericResponse = await next();

        CurrentlyLoggedInPerson? issuingPerson = await currentUserService.GetCurrentlyLoggedInPersonAsync(cancellationToken);

        var response = genericResponse as IPagedList<IHateoasResponse>;
        
        switch (response.Items)
        {
            case ICollection<PersonResponse> personResponses:
                foreach (PersonResponse personResponse in personResponses)
                {
                    personResponse.Links = linkService.GeneratePersonRelatedLinks(
                        personResponse.Id,
                        issuingPerson);
                }
                break;
            
            case ICollection<CatResponse> catResponses:
                foreach (CatResponse catResponse in catResponses)
                {
                    catResponse.Links = linkService.GenerateCatRelatedLinks(
                        catResponse.Id,
                        catResponse.PersonId,
                        catResponse.AdvertisementId,
                        issuingPerson);
                }
                break;
            
            case ICollection<AdvertisementResponse> advertisementResponses:
                foreach (AdvertisementResponse advertisementResponse in advertisementResponses)
                {
                    advertisementResponse.Links = linkService.GenerateAdvertisementRelatedLinks(
                        advertisementResponse.Id,
                        advertisementResponse.Status,
                        advertisementResponse.PersonId,
                        issuingPerson);
                }
                break;
        }

        return genericResponse;
    }
}
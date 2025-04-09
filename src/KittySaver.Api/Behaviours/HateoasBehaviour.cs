using KittySaver.Api.Hateoas;
using KittySaver.Api.Infrastructure.Services;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Shared.Hateoas;
using KittySaver.Shared.Responses;
using MediatR;

namespace KittySaver.Api.Behaviours;

public sealed class HateoasBehaviour<TRequest, TResponse>(
    ICurrentUserService currentUserService,
    ILinkService linkService) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull 
    where TResponse : IHateoasResponse 
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        TResponse response = await next();

        CurrentlyLoggedInPerson? issuingPerson = await currentUserService.GetCurrentlyLoggedInPersonAsync(cancellationToken);

        bool doesRequestRequireAuthorization = request is IAuthorizedRequest;
        response.Links = response switch
        {
            GetApiDiscoveryV1Response => linkService.GenerateApiDiscoveryV1Links(issuingPerson?.PersonId ?? null),
            IHateoasPersonResponse personResponse => linkService.GeneratePersonRelatedLinks(
                personResponse.Id,
                issuingPerson),
            IHateoasCatResponse catResponse => linkService.GenerateCatRelatedLinks(
                catResponse.Id,
                catResponse.PersonId,
                catResponse.AdvertisementId,
                catResponse.IsThumbnailUploaded,
                catResponse.IsAdopted,
                issuingPerson),
            IHateoasAdvertisementResponse advertisementResponse => linkService.GenerateAdvertisementRelatedLinks(
                advertisementResponse.Id,
                advertisementResponse.Status,
                advertisementResponse.PersonId,
                issuingPerson,
                doesRequestRequireAuthorization),
            _ => response.Links
        };

        return response;
    }
}
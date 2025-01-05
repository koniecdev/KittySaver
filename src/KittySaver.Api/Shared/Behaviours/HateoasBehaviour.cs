using KittySaver.Api.Features.Advertisements.SharedContracts;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Domain.Persons;
using MediatR;

namespace KittySaver.Api.Shared.Behaviours;

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

        response.Links = response switch
        {
            IHateoasPersonResponse personResponse => linkService.GeneratePersonRelatedLinks(
                personResponse.Id,
                issuingPerson),
            IHateoasCatResponse catResponse => linkService.GenerateCatRelatedLinks(
                catResponse.Id,
                catResponse.PersonId,
                catResponse.AdvertisementId,
                issuingPerson),
            IHateoasAdvertisementResponse advertisementResponse => linkService.GenerateAdvertisementRelatedLinks(
                advertisementResponse.Id,
                advertisementResponse.Status,
                advertisementResponse.PersonId,
                issuingPerson),
            _ => response.Links
        };

        return response;
    }
}
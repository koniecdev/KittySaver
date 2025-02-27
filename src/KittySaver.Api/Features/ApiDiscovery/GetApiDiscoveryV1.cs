using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Endpoints;
using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Shared.Responses;
using MediatR;

namespace KittySaver.Api.Features.ApiDiscovery;

public class GetApiDiscoveryV1 : IEndpoint
{
    public sealed record GetApiDiscoveryV1Query : IQuery<GetApiDiscoveryV1Response>;
    
    internal sealed class GetAdvertisementQueryHandler(ICurrentUserService currentUserService)
        : IRequestHandler<GetApiDiscoveryV1Query, GetApiDiscoveryV1Response>
    {
        public async Task<GetApiDiscoveryV1Response> Handle(GetApiDiscoveryV1Query request, CancellationToken cancellationToken)
        {
            CurrentlyLoggedInPerson? issuingPerson = await currentUserService.GetCurrentlyLoggedInPersonAsync(cancellationToken);
            
            return new GetApiDiscoveryV1Response
            {
                PersonId = issuingPerson?.PersonId
            };
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("/", async(
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                GetApiDiscoveryV1Response response = await sender.Send(new GetApiDiscoveryV1Query(), cancellationToken);
                return Results.Ok(response);
            }).AllowAnonymous()
            .WithName(EndpointNames.GetApiDiscoveryV1.EndpointName)
            .WithTags(EndpointNames.GroupNames.Discovery);
    }
}
using KittySaver.Api.Features.Advertisements.SharedContracts;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Endpoints;
using KittySaver.Api.Shared.Hateoas;
using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Shared.Hateoas;
using KittySaver.Shared.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.ApiDiscovery;

public class GetApiDiscoveryV1 : IEndpoint
{
    public sealed record GetApiDiscoveryV1Query : IQuery<GetApiDiscoveryV1Response>;
    
    internal sealed class GetAdvertisementQueryHandler
        : IRequestHandler<GetApiDiscoveryV1Query, GetApiDiscoveryV1Response>
    {
        public async Task<GetApiDiscoveryV1Response> Handle(GetApiDiscoveryV1Query request, CancellationToken cancellationToken) 
            => await Task.FromResult(new GetApiDiscoveryV1Response());
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
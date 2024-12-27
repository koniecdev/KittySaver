using KittySaver.Api.Features.Advertisements.SharedContracts;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Persons;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Advertisements;

public sealed class GetAdvertisements : IEndpoint
{
    public sealed record GetAdvertisementsQuery : IAdvertisementQuery<ICollection<AdvertisementResponse>>;

    internal sealed class GetAdvertisementsQueryHandler(ApplicationReadDbContext db)
        : IRequestHandler<GetAdvertisementsQuery, ICollection<AdvertisementResponse>>
    {
        public async Task<ICollection<AdvertisementResponse>> Handle(GetAdvertisementsQuery request, CancellationToken cancellationToken)
        {
            List<AdvertisementResponse> advertisements = await db.Advertisements
                .ProjectToDto()
                .ToListAsync(cancellationToken);

            return advertisements;
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("advertisements", async (
            int? offset,
            int? limit,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            GetAdvertisementsQuery query = new();
            ICollection<AdvertisementResponse> advertisements = await sender.Send(query, cancellationToken);
            return Results.Ok(advertisements);
        }).AllowAnonymous()
        .WithName(EndpointNames.GetAdvertisements.EndpointName)
        .WithTags(EndpointNames.GroupNames.AdvertisementGroup);
    }
}
using KittySaver.Api.Features.Advertisements.SharedContracts;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Api.Shared.Persistence.ReadModels;
using KittySaver.Domain.Persons;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Advertisements;

public sealed class GetAdvertisements : IEndpoint
{
    public sealed class GetAdvertisementsQuery(int? offset, int? limit)
        : IAdvertisementQuery<IPagedList<AdvertisementResponse>>
    {
        public int? Offset { get; } = offset;
        public int? Limit { get; } = limit;
    }

    internal sealed class GetAdvertisementsQueryHandler(
        ApplicationReadDbContext db,
        IPaginationLinksService paginationLinksService)
        : IRequestHandler<GetAdvertisementsQuery, IPagedList<AdvertisementResponse>>
    {
        public async Task<IPagedList<AdvertisementResponse>> Handle(GetAdvertisementsQuery request, CancellationToken cancellationToken)
        {
            IQueryable<AdvertisementReadModel> query = db.Advertisements;
            int totalRecords = await query.CountAsync(cancellationToken);

            if (request.Offset.HasValue)
            {
                query = query.Skip(request.Offset.Value);
            }

            if (request.Limit.HasValue)
            {
                query = query.Take(request.Limit.Value);
            }
            
            List<AdvertisementResponse> advertisements =
                await query
                    .ProjectToDto()
                    .ToListAsync(cancellationToken);

            PagedList<AdvertisementResponse> response = new()
            {
                Items = advertisements,
                Total = totalRecords,
                Links = paginationLinksService.GeneratePaginationLinks(
                    EndpointNames.GetAdvertisements.EndpointName,
                    request.Offset,
                    request.Limit,
                    totalRecords)
            };
            
            return response;
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
            GetAdvertisementsQuery query = new(offset, limit);
            IPagedList<AdvertisementResponse> advertisements = await sender.Send(query, cancellationToken);
            return Results.Ok(advertisements);
        }).AllowAnonymous()
        .WithName(EndpointNames.GetAdvertisements.EndpointName)
        .WithTags(EndpointNames.GroupNames.AdvertisementGroup);
    }
}
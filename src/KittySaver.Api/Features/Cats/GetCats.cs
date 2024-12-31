using KittySaver.Api.Features.Cats.SharedContracts;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Api.Shared.Persistence.ReadModels;
using KittySaver.Domain.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Cats;

public sealed class GetCats : IEndpoint
{
    public sealed class GetCatsQuery(Guid personId, int? offset, int? limit)
        : ICatQuery<IPagedList<CatResponse>>
    {
        public Guid PersonId { get; } = personId;
        public int? Offset { get; } = offset;
        public int? Limit { get; } = limit;
    }

    internal sealed class GetCatsQueryHandler(
        ApplicationReadDbContext db,
        IPaginationLinksService paginationLinksService)
        : IRequestHandler<GetCatsQuery, IPagedList<CatResponse>>
    {
        public async Task<IPagedList<CatResponse>> Handle(GetCatsQuery request, CancellationToken cancellationToken)
        {
            bool personExists = await db.Persons
                .AnyAsync(x => x.Id == request.PersonId, cancellationToken);
            if (!personExists)
            {
                throw new NotFoundExceptions.PersonNotFoundException(request.PersonId);
            }

            IQueryable<CatReadModel> query = db.Cats
                .Where(x => x.PersonId == request.PersonId);
            
            int totalRecords = await query.CountAsync(cancellationToken);

            if (request.Offset.HasValue)
            {
                query = query.Skip(request.Offset.Value);
            }

            if (request.Limit.HasValue)
            {
                query = query.Take(request.Limit.Value);
            }
            List<CatResponse> cats =
                await query
                    .ProjectToDto()
                    .ToListAsync(cancellationToken);
            
            PagedList<CatResponse> response = new PagedList<CatResponse>
            {
                Items = cats,
                Total = totalRecords,
                Links = paginationLinksService.GeneratePaginationLinks(
                    EndpointNames.GetCats.EndpointName,
                    request.Offset,
                    request.Limit,
                    totalRecords)
            };
            
            return response;
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("persons/{personId:guid}/cats", async (
                Guid personId,
                int? offset,
                int? limit,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                GetCatsQuery query = new(personId, offset, limit);
                IPagedList<CatResponse> cats = await sender.Send(query, cancellationToken);
                return Results.Ok(cats);
            }).RequireAuthorization()
            .WithName(EndpointNames.GetCats.EndpointName)
            .WithTags(EndpointNames.GroupNames.CatGroup);
    }
}
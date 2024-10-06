using KittySaver.Api.Features.Cats.SharedContracts;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Cats;

public sealed class GetCats : IEndpoint
{
    public sealed class GetCatsQuery : IQuery<ICollection<CatResponse>>
    {
        public List<ApiFilter>? Filters { get; init; } = [];
        public List<ApiSort>? Sorts { get; init; } = [];
    }

    internal sealed class GetCatsQueryHandler(ApplicationDbContext db)
        : IRequestHandler<GetCatsQuery, ICollection<CatResponse>>
    {
        public async Task<ICollection<CatResponse>> Handle(GetCatsQuery request, CancellationToken cancellationToken)
        {
            List<CatResponse> cats = await db.Cats
                .AsNoTracking()
                .ProjectToDto()
                .ToListAsync(cancellationToken);
            return cats;
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("cats", async
            (string? filterBy,
            string? sortBy,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            GetCatsQuery query = new();
            ICollection<CatResponse> cats = await sender.Send(query, cancellationToken);
            return Results.Ok(cats);
        });
    }
}
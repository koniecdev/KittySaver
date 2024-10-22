using KittySaver.Api.Features.Cats.SharedContracts;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Cats;

public sealed class GetCats : IEndpoint
{
    public sealed class GetCatsQuery(Guid personId) : IQuery<ICollection<CatResponse>>
    {
        public Guid PersonId { get; } = personId;
    }

    internal sealed class GetCatsQueryHandler(ApplicationDbContext db)
        : IRequestHandler<GetCatsQuery, ICollection<CatResponse>>
    {
        public async Task<ICollection<CatResponse>> Handle(GetCatsQuery request, CancellationToken cancellationToken)
        {
            List<CatResponse> cats = await db.Persons
                .AsNoTracking()
                .Where(x=>x.Id == request.PersonId)
                .SelectMany(x=>x.Cats)
                .ProjectToDto()
                .ToListAsync(cancellationToken);
            return cats;
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("persons/{personId:guid}/cats", async (
            Guid personId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            GetCatsQuery query = new(personId);
            ICollection<CatResponse> cats = await sender.Send(query, cancellationToken);
            return Results.Ok(cats);
        });
    }
}
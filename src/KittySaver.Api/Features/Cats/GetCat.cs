using KittySaver.Api.Features.Cats.SharedContracts;
using KittySaver.Api.Shared.Domain.Entites;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Cats;

public class GetCat : IEndpoint
{
    public sealed record GetCatQuery(Guid Id) : IQuery<CatResponse>;

    internal sealed class GetCatQueryHandler(ApplicationDbContext db)
        : IRequestHandler<GetCatQuery, CatResponse>
    {
        public async Task<CatResponse> Handle(GetCatQuery request, CancellationToken cancellationToken)
        {
            CatResponse cat = await db.Cats
                .AsNoTracking()
                .Where(x=>x.Id == request.Id)
                .ProjectToDto()
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new Cat.CatNotFoundException(request.Id);
            return cat;
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("cats/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            GetCatQuery query = new(id);
            CatResponse cat = await sender.Send(query, cancellationToken);
            return Results.Ok(cat);
        });
    }
}
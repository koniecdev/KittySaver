using KittySaver.Api.Features.Cats.SharedContracts;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Cats;

public sealed class GetCat : IEndpoint
{
    public sealed record GetCatQuery(Guid PersonId, Guid Id) : IQuery<CatResponse>;

    internal sealed class GetCatQueryHandler(ApplicationDbContext db)
        : IRequestHandler<GetCatQuery, CatResponse>
    {
        public async Task<CatResponse> Handle(GetCatQuery request, CancellationToken cancellationToken)
        {
            CatResponse cat =
                await db.Persons
                    .AsNoTracking()
                    .Where(x => x.Id == request.PersonId)
                    .SelectMany(x => x.Cats)
                    .Where(x => x.Id == request.Id)
                    .ProjectToDto()
                    .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundExceptions.CatNotFoundException(request.Id);
            return cat;
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("/persons/{personId:guid}/cats/{id:guid}", async (
            Guid personId,
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            GetCatQuery query = new(personId, id);
            CatResponse cat = await sender.Send(query, cancellationToken);
            return Results.Ok(cat);
        });
    }
}
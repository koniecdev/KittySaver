using KittySaver.Api.Features.Cats.SharedContracts;
using KittySaver.Api.Infrastructure.Endpoints;
using KittySaver.Api.Persistence.ReadRelated;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Shared.Responses;
using KittySaver.Shared.TypedIds;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Cats;

public sealed class GetCat : IEndpoint
{
    public sealed record GetCatQuery(PersonId PersonId, CatId Id) : IQuery<CatResponse>, IAuthorizedRequest, ICatRequest;

    internal sealed class GetCatQueryHandler(
        ApplicationReadDbContext db)
        : IRequestHandler<GetCatQuery, CatResponse>
    {
        public async Task<CatResponse> Handle(GetCatQuery request, CancellationToken cancellationToken)
        {
            CatResponse cat =
                await db.Cats
                    .Where(x => x.Id == request.Id && x.PersonId == request.PersonId)
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
            GetCatQuery query = new(new PersonId(personId), new CatId(id));
            CatResponse cat = await sender.Send(query, cancellationToken);
            return Results.Ok(cat);
        }).RequireAuthorization()
        .WithName(EndpointNames.Cats.GetById.EndpointName)
        .WithTags(EndpointNames.Cats.Group);
    }
}
using KittySaver.Api.Features.Cats.SharedContracts;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Endpoints;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Shared.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Cats;

public sealed class GetCat : IEndpoint
{
    public sealed record GetCatQuery(Guid PersonId, Guid Id) : IQuery<CatResponse>, IAuthorizedRequest, ICatRequest;

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
            GetCatQuery query = new(personId, id);
            CatResponse cat = await sender.Send(query, cancellationToken);
            return Results.Ok(cat);
        }).RequireAuthorization()
        .WithName(EndpointNames.GetCat.EndpointName)
        .WithTags(EndpointNames.GroupNames.CatGroup);
    }
}
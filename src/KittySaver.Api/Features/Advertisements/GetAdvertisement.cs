using KittySaver.Api.Features.Advertisements.SharedContracts;
using KittySaver.Api.Infrastructure.Endpoints;
using KittySaver.Api.Persistence.ReadRelated;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Shared.Responses;
using KittySaver.Shared.TypedIds;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Advertisements;

public class GetAdvertisement : IEndpoint
{
    public sealed record GetAdvertisementQuery(PersonId PersonId, AdvertisementId Id) 
        : IQuery<AdvertisementResponse>, IAuthorizedRequest, IAdvertisementRequest;

    internal sealed class GetAdvertisementQueryHandler(
        ApplicationReadDbContext db)
        : IRequestHandler<GetAdvertisementQuery, AdvertisementResponse>
    {
        public async Task<AdvertisementResponse> Handle(GetAdvertisementQuery request, CancellationToken cancellationToken)
        {
            AdvertisementResponse advertisement = await db.Advertisements
                .Where(x => x.PersonId == request.PersonId && x.Id == request.Id)
                .ProjectToDto()
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundExceptions.AdvertisementNotFoundException(request.Id);

            return advertisement;
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("persons/{personId:guid}/advertisements/{id:guid}", async(
            Guid personId,
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            GetAdvertisementQuery query = new(new PersonId(personId), new AdvertisementId(id));
            AdvertisementResponse advertisement = await sender.Send(query, cancellationToken);
            return Results.Ok(advertisement);
        }).RequireAuthorization()
        .WithName(EndpointNames.GetAdvertisement.EndpointName)
        .WithTags(EndpointNames.GroupNames.AdvertisementGroup);
    }
}
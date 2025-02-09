using KittySaver.Api.Features.Advertisements.SharedContracts;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Endpoints;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Shared.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Advertisements;

public class GetPublicAdvertisement : IEndpoint
{
    public sealed record GetPublicAdvertisementQuery(Guid Id) : IQuery<AdvertisementResponse>;

    internal sealed class GetPublicAdvertisementQueryHandler(
        ApplicationReadDbContext db)
        : IRequestHandler<GetPublicAdvertisementQuery, AdvertisementResponse>
    {
        public async Task<AdvertisementResponse> Handle(GetPublicAdvertisementQuery request, CancellationToken cancellationToken)
        {
            AdvertisementResponse advertisement = await db.Advertisements
                .Where(x => 
                    x.Id == request.Id 
                    && x.Status == AdvertisementStatus.Active)
                .ProjectToDto()
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundExceptions.AdvertisementNotFoundException(request.Id);

            return advertisement;
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("advertisements/{id:guid}", async(
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            GetPublicAdvertisementQuery query = new(id);
            AdvertisementResponse advertisement = await sender.Send(query, cancellationToken);
            return Results.Ok(advertisement);
        }).AllowAnonymous()
        .WithName(EndpointNames.GetPublicAdvertisement.EndpointName)
        .WithTags(EndpointNames.GroupNames.AdvertisementGroup);
    }
}
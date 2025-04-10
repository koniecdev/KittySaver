﻿using KittySaver.Api.Features.Advertisements.SharedContracts;
using KittySaver.Api.Infrastructure.Endpoints;
using KittySaver.Api.Persistence.ReadRelated;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Shared.Common.Enums;
using KittySaver.Shared.Responses;
using KittySaver.Shared.TypedIds;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Advertisements;

public class GetPublicAdvertisement : IEndpoint
{
    public sealed record GetPublicAdvertisementQuery(AdvertisementId Id) : IQuery<AdvertisementResponse>;

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
            GetPublicAdvertisementQuery query = new(new AdvertisementId(id));
            AdvertisementResponse advertisement = await sender.Send(query, cancellationToken);
            return Results.Ok(advertisement);
        }).AllowAnonymous()
        .WithName(EndpointNames.Advertisements.GetPublicById.EndpointName)
        .WithTags(EndpointNames.Advertisements.Group);
    }
}
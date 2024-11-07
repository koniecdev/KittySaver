using System.Diagnostics.CodeAnalysis;
using KittySaver.Api.Features.Advertisements.SharedContracts;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Advertisements;

public class GetAdvertisement : IEndpoint
{
    public sealed record GetAdvertisementQuery(Guid Id) : IQuery<AdvertisementResponse>;

    internal sealed class GetAdvertisementQueryHandler(ApplicationDbContext db)
        : IRequestHandler<GetAdvertisementQuery, AdvertisementResponse>
    {
        public async Task<AdvertisementResponse> Handle(GetAdvertisementQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("advertisements/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            GetAdvertisementQuery query = new(id);
            AdvertisementResponse person = await sender.Send(query, cancellationToken);
            return Results.Ok(person);
        });
    }
}
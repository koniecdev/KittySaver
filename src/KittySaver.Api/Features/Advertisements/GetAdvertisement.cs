using KittySaver.Api.Features.Advertisements.SharedContracts;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Persons;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Advertisements;

public class GetAdvertisement : IEndpoint
{
    public sealed record GetAdvertisementQuery(Guid Id) : IAdvertisementQuery<AdvertisementResponse>;

    internal sealed class GetAdvertisementQueryHandler(
        ApplicationReadDbContext db,
        ILinkService linkService,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetAdvertisementQuery, AdvertisementResponse>
    {
        public async Task<AdvertisementResponse> Handle(GetAdvertisementQuery request, CancellationToken cancellationToken)
        {
            CurrentlyLoggedInPerson? loggedInPerson = await currentUserService.GetCurrentlyLoggedInPersonAsync(cancellationToken);
            AdvertisementResponse advertisement = await db.Advertisements
                .Where(x => x.Id == request.Id)
                .ProjectToDto(linkService, loggedInPerson)
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
            GetAdvertisementQuery query = new(id);
            AdvertisementResponse advertisement = await sender.Send(query, cancellationToken);
            return Results.Ok(advertisement);
        }).AllowAnonymous()
        .WithName(EndpointNames.GetAdvertisement.EndpointName)
        .WithTags(EndpointNames.GroupNames.AdvertisementGroup);
    }
}
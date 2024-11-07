using KittySaver.Api.Features.Advertisements.SharedContracts;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Advertisements;

public sealed class GetAdvertisements : IEndpoint
{
    public sealed class GetAdvertisementsQuery : IQuery<ICollection<GetAdvertisementsQuery.Response>>
    {
        public sealed class Response
        {
            public required Guid Id { get; init; }
            public required Guid PersonId { get; init; }
            public required string PersonName { get; init; }
            public required string Title { get; init; }
            public required double PriorityScore { get; init; }
            public required string? Description { get; init; }
            public required PickupAddressDto PickupAddress { get; init; }
            public required ContactInfoDto ContactInfo { get; init; }
    
            public sealed class PickupAddressDto
            {
                public required string Country { get; init; }
                public required string? State { get; init; }
                public required string ZipCode { get; init; }
                public required string City { get; init; }
                public required string? Street { get; init; }
                public required string? BuildingNumber { get; init; }
            }

            public sealed class ContactInfoDto
            {
                public required string Email { get; init; }
                public required string PhoneNumber { get; init; }
            }
        }
    }

    internal sealed class GetAdvertisementsQueryHandler(ApplicationDbContext db)
        : IRequestHandler<GetAdvertisementsQuery, ICollection<GetAdvertisementsQuery.Response>>
    {
        public async Task<ICollection<GetAdvertisementsQuery.Response>> Handle(GetAdvertisementsQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("advertisements", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            GetAdvertisementsQuery query = new();
            ICollection<GetAdvertisementsQuery.Response> advertisements = await sender.Send(query, cancellationToken);
            return Results.Ok(advertisements);
        });
    }
}
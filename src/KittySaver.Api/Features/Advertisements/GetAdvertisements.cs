using KittySaver.Api.Features.Advertisements.SharedContracts;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Persons;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Advertisements;

public sealed class GetAdvertisements : IEndpoint
{
    public sealed record GetAdvertisementsQuery : IQuery<ICollection<AdvertisementResponse>>;

    internal sealed class GetAdvertisementsQueryHandler(ApplicationDbContext db)
        : IRequestHandler<GetAdvertisementsQuery, ICollection<AdvertisementResponse>>
    {
        public async Task<ICollection<AdvertisementResponse>> Handle(GetAdvertisementsQuery request, CancellationToken cancellationToken)
        {
            List<AdvertisementResponse> advertisements = await db.Advertisements
                .AsNoTracking()
                .Select(x => new AdvertisementResponse
                {
                    Id = x.Id,
                    ContactInfoEmail = x.ContactInfoEmail,
                    ContactInfoPhoneNumber = x.ContactInfoPhoneNumber,
                    PriorityScore = x.PriorityScore,
                    Description = x.Description,
                    PersonId = x.PersonId,
                    PickupAddress = new AdvertisementResponse.PickupAddressDto()
                    {
                        BuildingNumber = x.PickupAddress.BuildingNumber,
                        City = x.PickupAddress.City,
                        Country = x.PickupAddress.Country,
                        State = x.PickupAddress.State,
                        Street = x.PickupAddress.Street,
                        ZipCode = x.PickupAddress.ZipCode
                    }
                }).ToListAsync(cancellationToken);
            
            foreach (AdvertisementResponse advertisement in advertisements)
            {
                Person person = await db.Persons
                    .AsNoTracking()
                    .Where(x => x.Id == advertisement.PersonId)
                    .Include(x => x.Cats.Where(c => c.AdvertisementId == advertisement.Id))
                    .FirstAsync(cancellationToken);
                List<Cat> cats = person.Cats.ToList();
                advertisement.PersonName = person.FirstName;
                advertisement.Cats = cats.Select(x=> new AdvertisementResponse.CatDto
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToList();
            }

            return advertisements;
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("advertisements", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            GetAdvertisementsQuery query = new();
            ICollection<AdvertisementResponse> advertisements = await sender.Send(query, cancellationToken);
            return Results.Ok(advertisements);
        });
    }
}
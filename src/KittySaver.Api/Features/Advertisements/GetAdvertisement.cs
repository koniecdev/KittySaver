using System.Diagnostics.CodeAnalysis;
using KittySaver.Api.Features.Advertisements.SharedContracts;
using KittySaver.Api.Shared.Domain.Persons;
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
            AdvertisementResponse advertisement = await db.Advertisements
                .AsNoTracking()
                .Where(x=>x.Id == request.Id)
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
                }).FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundExceptions.AdvertisementNotFoundException(request.Id);
            
            Person person = await db.Persons
                .AsNoTracking()
                .Where(x => x.Id == advertisement.PersonId)
                .Include(x => x.Cats.Where(c => c.AdvertisementId == advertisement.Id))
                .FirstAsync(cancellationToken);
            
            advertisement.PersonName = person.FirstName;
            List<Cat> cats = person.Cats.ToList();
            advertisement.Cats = cats.Select(x=> new AdvertisementResponse.CatDto
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();

            return advertisement;
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
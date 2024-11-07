using FluentValidation;
using KittySaver.Api.Shared.Domain.Entites;
using KittySaver.Api.Shared.Domain.Services;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Cats;

public sealed class AssignCatToAdvertisement : IEndpoint
{
    public sealed record AssignCatToAdvertisementCommand(
        Guid PersonId,
        Guid CatId,
        Guid AdvertisementId
        ) : ICommand;

    public sealed class AssignCatToAdvertisementCommandValidator : AbstractValidator<AssignCatToAdvertisementCommand>
    {
        public AssignCatToAdvertisementCommandValidator()
        {
            RuleFor(x => x.PersonId)
                .NotEmpty()
                .NotEqual(x => x.AdvertisementId)
                .NotEqual(x => x.CatId);
            
            RuleFor(x => x.CatId)
                .NotEmpty()
                .NotEqual(x => x.AdvertisementId)
                .NotEqual(x => x.PersonId);
            
            RuleFor(x => x.AdvertisementId)
                .NotEmpty()
                .NotEqual(x => x.PersonId)
                .NotEqual(x => x.CatId);
        }
    }
    
    public sealed class AssignCatToAdvertisementCommandHandler(ApplicationDbContext db) 
        : IRequestHandler<AssignCatToAdvertisementCommand>
    {
        public async Task Handle(AssignCatToAdvertisementCommand request, CancellationToken cancellationToken)
        {
            Person person = await db.Persons
                              .Include(x => x.Cats)
                              .FirstOrDefaultAsync(x => x.Id == request.PersonId, cancellationToken)
                          ?? throw new NotFoundExceptions.PersonNotFoundException(request.PersonId);

            Cat cat = person.Cats
                          .FirstOrDefault(x => x.Id == request.CatId)
                      ?? throw new NotFoundExceptions.CatNotFoundException(request.CatId);

            if (cat.AdvertisementId is not null)
            {
                throw new Exception();
            }

            Advertisement advertisement = 
                await db.Advertisements.FirstOrDefaultAsync(x => x.Id == request.AdvertisementId, cancellationToken)
                ?? throw new NotFoundExceptions.AdvertisementNotFoundException(request.AdvertisementId);
            
            AssignCatToAdvertisement(cat, person, advertisement);
            
            await db.SaveChangesAsync(cancellationToken);
        }
        
        private static void AssignCatToAdvertisement(Cat catToAssign, Person advertisementOwningPerson, Advertisement advertisement)
        {
            //TODO: First two if checks are redundant because Cat.AssignAdvertisement() checks for that as well
            //TODO: Advertisement re calculation of PriorityScore logic could subscribe to e.g. CatAssignedToAdvertisementDomainEvent
            if (advertisementOwningPerson.Cats.All(cat => cat.Id != catToAssign.Id))
            {
                throw new Exception();
            }
        
            if (catToAssign.AdvertisementId is not null)
            {
                throw new Exception();
            }
        
            catToAssign.AssignAdvertisement(advertisement.Id);

            List<Guid> advertisementCatsIds = advertisementOwningPerson.Cats
                .Where(x => x.AdvertisementId == advertisement.Id)
                .Select(x=>x.Id)
                .ToList();
            
            advertisement.PriorityScore = advertisementOwningPerson.GetHighestPriorityScoreFromGivenCats(advertisementCatsIds);
        }
    }
    
    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPut("persons/{personId:guid}/cats/{catId:guid}/advertisement/{advertisementId:guid}", async (
            Guid personId,
            Guid catId,
            Guid advertisementId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            AssignCatToAdvertisementCommand command = new(
                PersonId: personId,
                CatId: catId,
                AdvertisementId: advertisementId);
            
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).RequireAuthorization();
    }
}
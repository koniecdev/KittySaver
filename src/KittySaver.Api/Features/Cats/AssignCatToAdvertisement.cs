using FluentValidation;
using KittySaver.Api.Shared.Domain.Persons;
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
        Guid AdvertisementId) : ICommand;

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
            
            if (await db.Advertisements.AnyAsync(x => x.Id == request.AdvertisementId, cancellationToken))
            {
                throw new NotFoundExceptions.AdvertisementNotFoundException(request.AdvertisementId);
            }

            person.AssignCatToAdvertisement(request.AdvertisementId, request.CatId);
            
            await db.SaveChangesAsync(cancellationToken);
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
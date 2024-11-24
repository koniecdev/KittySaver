using FluentValidation;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Advertisements;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Persons;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Advertisements;

public sealed class ReassignCatsToAdvertisement : IEndpoint
{
    public sealed record ReassignCatsToAdvertisementRequest(IEnumerable<Guid> CatIds);
    public sealed record ReassignCatsToAdvertisementCommand(
        Guid AdvertisementId,
        IEnumerable<Guid> CatIds) : ICommand;

    public sealed class AssignCatToAdvertisementCommandValidator : AbstractValidator<ReassignCatsToAdvertisementCommand>
    {
        public AssignCatToAdvertisementCommandValidator()
        {
            RuleFor(x => x.AdvertisementId).NotEmpty();
            RuleFor(x => x.CatIds).NotEmpty();
        }
    }
    
    public sealed class ReassignCatsToAdvertisementCommandHandler(ApplicationDbContext db) 
        : IRequestHandler<ReassignCatsToAdvertisementCommand>
    {
        public async Task Handle(ReassignCatsToAdvertisementCommand request, CancellationToken cancellationToken)
        {
            Advertisement advertisement = await db.Advertisements
                                              .FirstOrDefaultAsync(x => x.Id == request.AdvertisementId, cancellationToken)
                                          ?? throw new NotFoundExceptions.AdvertisementNotFoundException(request.AdvertisementId);

            Person person = await db.Persons
                                .Include(x => x.Cats)
                                .FirstAsync(x => x.Id == advertisement.PersonId, cancellationToken);

            AdvertisementService advertisementService = new();
            advertisementService.ReplaceCatsOfAdvertisement(person, advertisement, request.CatIds);
            
            await db.SaveChangesAsync(cancellationToken);
        }
    }
    
    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPut("advertisements/{id:guid}/cats", async (
            Guid id,
            ReassignCatsToAdvertisementRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            ReassignCatsToAdvertisementCommand command = new(
                AdvertisementId: id,
                CatIds: request.CatIds);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).RequireAuthorization();
    }
}
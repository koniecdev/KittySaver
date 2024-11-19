using FluentValidation;
using KittySaver.Api.Shared.Domain.Advertisement;
using KittySaver.Api.Shared.Domain.Persons;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Advertisements;

public sealed class DeleteAdvertisement : IEndpoint
{
    public sealed record DeleteAdvertisementCommand(Guid Id) : ICommand;

    public sealed class DeleteAdvertisementCommandValidator
        : AbstractValidator<DeleteAdvertisementCommand>
    {
        public DeleteAdvertisementCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }

    internal sealed class DeleteAdvertisementCommandHandler(ApplicationDbContext db)
        : IRequestHandler<DeleteAdvertisementCommand>
    {
        public async Task Handle(DeleteAdvertisementCommand request, CancellationToken cancellationToken)
        {
            Advertisement advertisement = 
                await db.Advertisements
                    .FirstOrDefaultAsync(x=> x.Id == request.Id, cancellationToken)
                    ?? throw new NotFoundExceptions.AdvertisementNotFoundException(request.Id);
            Person person = 
                await db.Persons
                    .Include(x=>x.Cats)
                    .FirstOrDefaultAsync(x=> x.Id == advertisement.PersonId, cancellationToken)
                    ?? throw new NotFoundExceptions.AdvertisementNotFoundException(request.Id);
            IEnumerable<Cat> catsThatAreAssignedToAdvertisement = person.Cats.Where(x => x.AdvertisementId == advertisement.Id);
            foreach (Cat cat in catsThatAreAssignedToAdvertisement)
            {
                person.UnassignCatFromAdvertisement(cat.Id);
            }
            db.Remove(advertisement);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapDelete("advertisements/{id:guid}", async (Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            DeleteAdvertisementCommand command = new(id);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).RequireAuthorization();
    }
}
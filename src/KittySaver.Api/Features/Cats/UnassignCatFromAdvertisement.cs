using FluentValidation;
using KittySaver.Api.Shared.Domain.Persons;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Cats;

public sealed class UnassignCatFromAdvertisement : IEndpoint
{
    public sealed record UnassignCatFromAdvertisementCommand(
        Guid PersonId,
        Guid CatId) : ICommand;

    public sealed class
        UnassignCatFromAdvertisementCommandValidator : AbstractValidator<UnassignCatFromAdvertisementCommand>
    {
        public UnassignCatFromAdvertisementCommandValidator()
        {
            RuleFor(x => x.PersonId)
                .NotEmpty()
                .NotEqual(x => x.CatId);

            RuleFor(x => x.CatId)
                .NotEmpty()
                .NotEqual(x => x.PersonId);
        }
    }

    public sealed class UnassignCatFromAdvertisementCommandHandler(ApplicationDbContext db)
        : IRequestHandler<UnassignCatFromAdvertisementCommand>
    {
        public async Task Handle(UnassignCatFromAdvertisementCommand request, CancellationToken cancellationToken)
        {
            Person person =
                await db.Persons
                    .Include(x => x.Cats)
                    .FirstOrDefaultAsync(x => x.Id == request.PersonId, cancellationToken)
                ?? throw new NotFoundExceptions.PersonNotFoundException(request.PersonId);
            
            person.UnassignCatFromAdvertisement(request.CatId);

            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapDelete("persons/{personId:guid}/cats/{catId:guid}/advertisement", async (
            Guid personId,
            Guid catId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            UnassignCatFromAdvertisementCommand command = new(
                PersonId: personId,
                CatId: catId);

            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).RequireAuthorization();
    }
}
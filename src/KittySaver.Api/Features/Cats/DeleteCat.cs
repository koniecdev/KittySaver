using FluentValidation;
using KittySaver.Api.Shared.Domain.Entites;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Cats;

public sealed class DeleteCat : IEndpoint
{
    public sealed record DeleteCatCommand(Guid PersonId, Guid Id) : ICommand;

    public sealed class DeleteCatCommandValidator
        : AbstractValidator<DeleteCatCommand>
    {
        public DeleteCatCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }

    internal sealed class DeleteCatCommandHandler(ApplicationDbContext db)
        : IRequestHandler<DeleteCatCommand>
    {
        public async Task Handle(DeleteCatCommand request, CancellationToken cancellationToken)
        {
            //for now, there are no invariants in domain that would suggest choosing approach with fetching whole aggregate
            int numberOfDeletedCats = await db.Persons
                .Where(x => x.Id == request.PersonId)
                .SelectMany(x => x.Cats)
                .Where(x => x.Id == request.Id)
                .ExecuteDeleteAsync(cancellationToken);
            
            if (numberOfDeletedCats == 0)
            {
                throw new NotFoundExceptions.CatNotFoundException(request.Id);
            }
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapDelete("/persons/{personId:guid}/cats/{id:guid}", async (
            Guid personId,
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            DeleteCatCommand command = new(personId, id);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).RequireAuthorization();
    }
}
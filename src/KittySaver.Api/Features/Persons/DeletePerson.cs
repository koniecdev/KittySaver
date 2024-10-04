using FluentValidation;
using KittySaver.Api.Shared.Domain.Entites;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Persons;

public sealed class DeletePerson : IEndpoint
{
    public sealed record DeletePersonCommand(Guid IdOrUserIdentityId) : ICommand;

    public sealed class DeletePersonCommandValidator
        : AbstractValidator<DeletePersonCommand>
    {
        public DeletePersonCommandValidator()
        {
            RuleFor(x => x.IdOrUserIdentityId).NotEmpty();
        }
    }

    internal sealed class DeletePersonCommandHandler(ApplicationDbContext db)
        : IRequestHandler<DeletePersonCommand>
    {
        public async Task Handle(DeletePersonCommand request, CancellationToken cancellationToken)
        {
            int numberOfDeletedPersons = await db.Persons
                .Where(x => 
                    x.Id == request.IdOrUserIdentityId
                    || x.UserIdentityId == request.IdOrUserIdentityId)
                .ExecuteDeleteAsync(cancellationToken);
            if (numberOfDeletedPersons == 0)
            {
                throw new Person.PersonNotFoundException(request.IdOrUserIdentityId);
            }
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapDelete("persons/{id:guid}", async (Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            DeletePersonCommand command = new(id);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).RequireAuthorization();
    }
}
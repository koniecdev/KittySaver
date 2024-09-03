using FluentValidation;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Infrastructure.Endpoints;
using KittySaver.Api.Shared.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Persons;

public sealed class DeletePerson : IEndpoint
{
    public sealed record DeletePersonCommand(
        Guid Id) : ICommand;

    public sealed class DeletePersonCommandValidator
        : AbstractValidator<DeletePersonCommand>, IAsyncValidator
    {
        private readonly ApplicationDbContext _db;

        public DeletePersonCommandValidator(ApplicationDbContext db)
        {
            _db = db;
            RuleFor(x => x.Id)
                .NotEmpty();
            RuleFor(x => x.Id)
                .MustAsync(async (id, ct) => await IsUserExistingInDatabase(id, ct))
                .WithMessage("User must exist in database to delete it");
        }

        private async Task<bool> IsUserExistingInDatabase(Guid id, CancellationToken ct)
            => await _db.Persons.AnyAsync(x => x.Id == id, ct);
    }

    internal sealed class DeletePersonCommandHandler(ApplicationDbContext db)
        : IRequestHandler<DeletePersonCommand>
    {
        public async Task Handle(DeletePersonCommand request, CancellationToken cancellationToken)
        {
            _ = await db.Persons
                .Where(x => x.Id == request.Id)
                .ExecuteDeleteAsync(cancellationToken);
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
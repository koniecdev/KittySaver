using FluentValidation;
using KittySaver.Auth.Api.Shared.Domain.Entites;
using KittySaver.Auth.Api.Shared.Exceptions;
using KittySaver.Auth.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Auth.Api.Shared.Infrastructure.Clients;
using KittySaver.Auth.Api.Shared.Infrastructure.Endpoints;
using KittySaver.Auth.Api.Shared.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Auth.Api.Features.ApplicationUsers;

public sealed class DeleteApplicationUser : IEndpoint
{
    public sealed record DeleteApplicationUserCommand(
        Guid Id) : ICommand;

    public sealed class DeleteApplicationUserCommandValidator
        : AbstractValidator<DeleteApplicationUserCommand>, IAsyncValidator
    {
        private readonly ApplicationDbContext _db;

        public DeleteApplicationUserCommandValidator(ApplicationDbContext db)
        {
            _db = db;
            RuleFor(x => x.Id)
                .NotEmpty();
            RuleFor(x => x.Id)
                .MustAsync(async (id, ct) => await IsUserExistingInDatabase(id, ct))
                .WithMessage("User must exist in database to delete it");
        }
        
        private async Task<bool> IsUserExistingInDatabase(Guid id, CancellationToken ct) 
            => await _db.ApplicationUsers.AnyAsync(x=>x.Id == id, ct);
    }
    
    internal sealed class DeleteApplicationUserCommandHandler(ApplicationDbContext db)
        : IRequestHandler<DeleteApplicationUserCommand>
    {
        public async Task Handle(DeleteApplicationUserCommand request, CancellationToken cancellationToken)
        {
            _ = await db.ApplicationUsers
                .Where(x => x.Id == request.Id)
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
    
    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapDelete("application-users/{id:guid}", async
            (Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            DeleteApplicationUserCommand command = new(id);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).RequireAuthorization();;
    }
}

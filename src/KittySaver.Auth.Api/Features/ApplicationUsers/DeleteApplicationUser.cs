using FluentValidation;
using KittySaver.Auth.Api.Shared.Domain.Entites;
using KittySaver.Auth.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Auth.Api.Shared.Infrastructure.Clients;
using KittySaver.Auth.Api.Shared.Infrastructure.Endpoints;
using KittySaver.Auth.Api.Shared.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Auth.Api.Features.ApplicationUsers;

public sealed class DeleteApplicationUser : IEndpoint
{
    public sealed record DeleteApplicationUserCommand(Guid Id) : ICommand;

    public sealed class DeleteApplicationUserCommandValidator
        : AbstractValidator<DeleteApplicationUserCommand>
    {
        public DeleteApplicationUserCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
    
    internal sealed class DeleteApplicationUserCommandHandler(UserManager<ApplicationUser> userManager, IKittySaverApiClient client)
        : IRequestHandler<DeleteApplicationUserCommand>
    {
        public async Task Handle(DeleteApplicationUserCommand request, CancellationToken cancellationToken)
        {
            ApplicationUser user = await userManager.Users
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
                ?? throw new ApplicationUser.Exceptions.ApplicationUserNotFoundException();
            await client.DeletePerson(user.Id);
            await userManager.DeleteAsync(user);
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
        }).RequireAuthorization();
    }
}

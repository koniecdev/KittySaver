using FluentValidation;
using KittySaver.Auth.Api.Features.ApplicationUsers.SharedContracts;
using KittySaver.Auth.Api.Shared.Domain.Entites;
using KittySaver.Auth.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Auth.Api.Shared.Infrastructure.Clients;
using KittySaver.Auth.Api.Shared.Infrastructure.Endpoints;
using KittySaver.Auth.Api.Shared.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Auth.Api.Features.ApplicationUsers;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class UpdateApplicationUser : IEndpoint
{
    public sealed record UpdateApplicationUserRequest(
        string FirstName,
        string LastName,
        string Email,
        string PhoneNumber);
    
    public sealed record UpdateApplicationUserCommand(
        Guid Id,
        string FirstName,
        string LastName,
        string Email,
        string PhoneNumber) : ICommand;

    public sealed class UpdateApplicationUserCommandValidator
        : AbstractValidator<UpdateApplicationUserCommand>, IAsyncValidator
    {
        private readonly ApplicationDbContext _db;

        public UpdateApplicationUserCommandValidator(ApplicationDbContext db)
        {
            _db = db; 
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();
            RuleFor(x => x.PhoneNumber).NotEmpty();
            RuleFor(x => x.Email)
                .NotEmpty()
                .Matches(ValidationPatterns.EmailPattern);
            RuleFor(x => x.Email)
                .MustAsync(async (email, ct) => await IsEmailUniqueAsync(email, ct))
                .WithMessage("Email is already used by another user.");
        }

        private async Task<bool> IsEmailUniqueAsync(string email, CancellationToken ct) 
            => !await _db.ApplicationUsers
                .AsNoTracking()
                .AnyAsync(x=>x.Email == email, ct);
    }
    
    internal sealed class UpdateApplicationUserCommandHandler(ApplicationDbContext db, IKittySaverApiClient client)
        : IRequestHandler<UpdateApplicationUserCommand>
    {
        public async Task Handle(UpdateApplicationUserCommand request, CancellationToken cancellationToken)
        {
            ApplicationUser user = await db.ApplicationUsers
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
                ?? throw new ApplicationUser.Exceptions.ApplicationUserNotFoundException();
            UpdateApplicationUserMapper.UpdateEntityWithCommandData(request, user);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
    
    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPut("application-users/{id:guid}", async
            (Guid id, 
            UpdateApplicationUserRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            UpdateApplicationUserCommand command = request.ToUpdateCommand(id);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        });
    }
}

[Mapper]
public static partial class UpdateApplicationUserMapper
{
    public static partial UpdateApplicationUser.UpdateApplicationUserCommand ToUpdateCommand(this UpdateApplicationUser.UpdateApplicationUserRequest request, Guid id);
    public static partial void UpdateEntityWithCommandData(UpdateApplicationUser.UpdateApplicationUserCommand source,
        ApplicationUser target);
}

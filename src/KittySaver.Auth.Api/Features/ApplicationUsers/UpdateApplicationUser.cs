using FluentValidation;
using KittySaver.Auth.Api.Features.ApplicationUsers.SharedContracts;
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

    public sealed class RegisterCommandValidator
        : AbstractValidator<UpdateApplicationUserCommand>, IAsyncValidator
    {
        private readonly ApplicationDbContext _db;

        public RegisterCommandValidator(ApplicationDbContext db)
        {
            _db = db; 
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();
            RuleFor(x => x.PhoneNumber).NotEmpty();
            RuleFor(x => x.Id)
                .MustAsync(async (id, ct) => await IsUserExistingInDatabase(id, ct))
                .WithMessage("User must exist in database to delete it");
            RuleFor(x => x.Email)
                .NotEmpty()
                .Matches(ValidationPatterns.EmailPattern);
            RuleFor(x => x.Email)
                .MustAsync(async (command, email, ct) => !await IsEmailAlreadyUsedBySomeoneElseInDb(command, email, ct))
                .WithMessage("Email is already used by different account in database");
        }

        private async Task<bool> IsEmailAlreadyUsedBySomeoneElseInDb(UpdateApplicationUserCommand command, string email, CancellationToken ct)
        {
            return await _db.ApplicationUsers.AnyAsync(x => x.Email == email && x.Id != command.Id, ct);
        } 
        private async Task<bool> IsUserExistingInDatabase(Guid id, CancellationToken ct) 
            => await _db.ApplicationUsers.AnyAsync(x=>x.Id == id, ct);
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

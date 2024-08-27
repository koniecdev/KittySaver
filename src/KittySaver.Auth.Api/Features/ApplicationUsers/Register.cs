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

public class Register : IEndpoint
{
    public sealed record RegisterRequest(
        string FirstName,
        string LastName,
        string Email,
        string PhoneNumber,
        string Password);
    
    public sealed record RegisterCommand(
        string FirstName,
        string LastName,
        string Email,
        string PhoneNumber,
        string Password) : ICommand<Guid>;

    public sealed class RegisterCommandValidator
        : AbstractValidator<RegisterCommand>, IAsyncValidator
    {
        private readonly ApplicationDbContext _db;
        private const string EmailPattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";

        public RegisterCommandValidator(ApplicationDbContext db)
        {
            _db = db;
            RuleFor(x => x.Password)
                .NotEmpty();
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("'Password' is not in the correct format. Your password cannot be empty")
                .MinimumLength(8).WithMessage("'Password' is not in the correct format. Your password length must be at least 8.")
                .Matches("[A-Z]+").WithMessage("'Password' is not in the correct format. Your password must contain at least one uppercase letter.")
                .Matches("[a-z]+").WithMessage("'Password' is not in the correct format. Your password must contain at least one lowercase letter.")
                .Matches("[0-9]+").WithMessage("'Password' is not in the correct format. Your password must contain at least one number.");
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();
            RuleFor(x => x.PhoneNumber).NotEmpty();
            RuleFor(x => x.Email)
                .NotEmpty()
                .Matches(EmailPattern);
            RuleFor(x => x.Email)
                .MustAsync(async (email, ct) => !await IsEmailAlreadyRegisteredInDb(email, ct))
                .WithMessage("Email is already registered in database");
        }
        
        private async Task<bool> IsEmailAlreadyRegisteredInDb(string email, CancellationToken ct)
        {
            return await _db.ApplicationUsers.AnyAsync(x=>x.Email == email, ct);
        }
    }
    
    internal sealed class RegisterCommandHandler(UserManager<ApplicationUser> userManager, IKittySaverApiClient client)
        : IRequestHandler<RegisterCommand, Guid>
    {
        public async Task<Guid> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            ApplicationUser user = request.ToEntity();
            IdentityResult identityResult = await userManager.CreateAsync(user, request.Password);
            if (!identityResult.Succeeded)
            {
                throw new IdentityResultException(identityResult.Errors);
            }

            try
            {
                _ = await client.CreatePerson(new IKittySaverApiClient.CreatePersonDto(
                    user.FirstName, user.LastName, user.Email!, user.PhoneNumber!, user.Id));
                return user.Id;
            }
            catch (Exception)
            {
                _ = await userManager.DeleteAsync(user);
                throw;
            }
        }
    }
    
    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("application-users/register", async
            (RegisterRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            RegisterCommand command = request.ToRegisterCommand();
            Guid personId = await sender.Send(command, cancellationToken);
            return Results.Created($"/api/v1/application-users/{personId}", new { Id = personId });
        });
    }
}

[Mapper]
public static partial class RegisterMapper
{
    public static partial Register.RegisterCommand ToRegisterCommand(this Register.RegisterRequest request);
    public static partial ApplicationUser ToEntity(this Register.RegisterCommand command);
}

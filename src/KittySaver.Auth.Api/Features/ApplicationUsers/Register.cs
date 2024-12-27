using FluentValidation;
using KittySaver.Auth.Api.Features.ApplicationUsers.SharedContracts;
using KittySaver.Auth.Api.Shared.Domain.Entites;
using KittySaver.Auth.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Auth.Api.Shared.Infrastructure.Clients;
using KittySaver.Auth.Api.Shared.Infrastructure.Endpoints;
using KittySaver.Auth.Api.Shared.Infrastructure.Services;
using KittySaver.Auth.Api.Shared.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Auth.Api.Features.ApplicationUsers;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class Register : IEndpoint
{
    public sealed record RegisterRequest(
        string UserName,
        string Email,
        string PhoneNumber,
        string Password,
        string DefaultAdvertisementPickupAddressCountry,
        string DefaultAdvertisementPickupAddressState,
        string DefaultAdvertisementPickupAddressZipCode,
        string DefaultAdvertisementPickupAddressCity,
        string DefaultAdvertisementPickupAddressStreet,
        string DefaultAdvertisementPickupAddressBuildingNumber,
        string DefaultAdvertisementContactInfoEmail,
        string DefaultAdvertisementContactInfoPhoneNumber);

    public sealed record RegisterCommand(
        string UserName,
        string Email,
        string PhoneNumber,
        string Password,
        string DefaultAdvertisementPickupAddressCountry,
        string DefaultAdvertisementPickupAddressState,
        string DefaultAdvertisementPickupAddressZipCode,
        string DefaultAdvertisementPickupAddressCity,
        string DefaultAdvertisementPickupAddressStreet,
        string DefaultAdvertisementPickupAddressBuildingNumber,
        string DefaultAdvertisementContactInfoEmail,
        string DefaultAdvertisementContactInfoPhoneNumber) : ICommand<Guid>;

    public sealed class RegisterCommandValidator
        : AbstractValidator<RegisterCommand>, IAsyncValidator
    {
        private readonly ApplicationDbContext _db;

        public RegisterCommandValidator(ApplicationDbContext db)
        {
            _db = db;
            RuleFor(x => x.Password)
                .NotEmpty();
            RuleFor(x => x.Password)
                .MinimumLength(8).WithMessage("'Password' is not in the correct format. Your password length must be at least 8.")
                .Matches("[A-Z]+").WithMessage("'Password' is not in the correct format. Your password must contain at least one uppercase letter.")
                .Matches("[a-z]+").WithMessage("'Password' is not in the correct format. Your password must contain at least one lowercase letter.")
                .Matches("[0-9]+").WithMessage("'Password' is not in the correct format. Your password must contain at least one number.");
            RuleFor(x => x.UserName).NotEmpty();
            RuleFor(x => x.PhoneNumber).NotEmpty();
            RuleFor(x => x.Email)
                .NotEmpty()
                .Matches(ValidationPatterns.EmailPattern)
                .MustAsync(async (email, ct) => await IsEmailUniqueAsync(email, ct))
                .WithMessage("Email is already used by another user.");;
            RuleFor(x => x.DefaultAdvertisementPickupAddressCountry).NotEmpty();
            RuleFor(x => x.DefaultAdvertisementPickupAddressState).NotEmpty();
            RuleFor(x => x.DefaultAdvertisementPickupAddressZipCode).NotEmpty();
            RuleFor(x => x.DefaultAdvertisementPickupAddressCity).NotEmpty();
            RuleFor(x => x.DefaultAdvertisementPickupAddressStreet).NotEmpty();
            RuleFor(x => x.DefaultAdvertisementPickupAddressBuildingNumber).NotEmpty();
            RuleFor(x => x.DefaultAdvertisementContactInfoEmail).NotEmpty().EmailAddress();
            RuleFor(x => x.DefaultAdvertisementContactInfoPhoneNumber).NotEmpty();

        }
        
        private async Task<bool> IsEmailUniqueAsync(string email, CancellationToken ct) 
            => !await _db.ApplicationUsers
                .AsNoTracking()
                .AnyAsync(x=>x.Email == email, ct);
    }
    
    internal sealed class RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        IKittySaverApiClient client,
        IJwtTokenService jwtTokenService)
        : IRequestHandler<RegisterCommand, Guid>
    {
        public async Task<Guid> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            ApplicationUser user = request.ToEntity();
            await userManager.CreateAsync(user, request.Password);
            try
            {
                ApplicationUser? applicationUser = await userManager.FindByEmailAsync(request.Email);
                if (applicationUser is null)
                {
                    throw new Exception();
                }
                (string token, DateTimeOffset _) tokenResults = await jwtTokenService.GenerateTokenAsync(applicationUser);
                await client.CreatePerson(tokenResults.token, new IKittySaverApiClient.CreatePersonDto(
                    Nickname: user.UserName!, 
                    Email: user.Email!, 
                    PhoneNumber: user.PhoneNumber!, 
                    UserIdentityId: user.Id,
                    DefaultAdvertisementPickupAddressCountry: user.DefaultAdvertisementPickupAddressCountry,
                    DefaultAdvertisementPickupAddressState: user.DefaultAdvertisementPickupAddressState,
                    DefaultAdvertisementPickupAddressZipCode: user.DefaultAdvertisementPickupAddressZipCode,
                    DefaultAdvertisementPickupAddressCity: user.DefaultAdvertisementPickupAddressCity,
                    DefaultAdvertisementPickupAddressStreet: user.DefaultAdvertisementPickupAddressStreet,
                    DefaultAdvertisementPickupAddressBuildingNumber: user.DefaultAdvertisementPickupAddressBuildingNumber,
                    DefaultAdvertisementContactInfoEmail: user.DefaultAdvertisementContactInfoEmail,
                    DefaultAdvertisementContactInfoPhoneNumber: user.DefaultAdvertisementContactInfoPhoneNumber));
            }
            catch(Exception)
            {
                await userManager.DeleteAsync(user);
                throw;
            }
            return user.Id;
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

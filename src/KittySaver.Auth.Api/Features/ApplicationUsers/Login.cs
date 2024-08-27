using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using KittySaver.Auth.Api.Shared.Domain.Entites;
using KittySaver.Auth.Api.Shared.Exceptions;
using KittySaver.Auth.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Auth.Api.Shared.Infrastructure.Clients;
using KittySaver.Auth.Api.Shared.Infrastructure.Endpoints;
using KittySaver.Auth.Api.Shared.Infrastructure.Services;
using KittySaver.Auth.Api.Shared.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Auth.Api.Features.ApplicationUsers;

public class Login : IEndpoint
{
    public sealed class LoginResponse
    {
        public required string AccessToken { get; init; }
        public required DateTimeOffset AccessTokenExpiresAt { get; init; }
    }
    
    public sealed record LoginRequest(
        string Email,
        string Password);
    
    public sealed record LoginCommand(
        string Email,
        string Password) : ICommand<LoginResponse>;

    public sealed class LoginCommandValidator
        : AbstractValidator<LoginCommand>
    {
        private const string EmailPattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";

        public LoginCommandValidator(ApplicationDbContext db)
        {
            RuleFor(x => x.Password)
                .NotEmpty();
            RuleFor(x => x.Email)
                .NotEmpty()
                .Matches(EmailPattern);
        }
    }
    
    internal sealed class LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IDateTimeProvider dateTimeProvider)
        : IRequestHandler<LoginCommand, LoginResponse>
    {
        public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            ApplicationUser user = await userManager.Users
                .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken)
                ?? throw new ApplicationUser.Exceptions.ApplicationUserNotFoundException();

            bool areUserCredentialsValid = await userManager.CheckPasswordAsync(user, request.Password);

            if (!areUserCredentialsValid)
            {
                throw new UnauthorizedAccessException("Provided credentials are invalid");
            }
            
            IList<string> roles = await userManager.GetRolesAsync(user);
            
            (string token, DateTimeOffset expiresAt) tokenResults = CreateToken(user, roles);
            LoginResponse response = new()
            {
                AccessToken = tokenResults.token,
                AccessTokenExpiresAt = tokenResults.expiresAt
            };
            return response;
        }

        private (string token, DateTimeOffset expiresAt) CreateToken(ApplicationUser user, ICollection<string> roles)
        {
            List<Claim> claims = [
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            ];

            foreach (string role in roles)
            {
                Claim roleClaim = new Claim(ClaimTypes.Role, role);
                claims.Add(roleClaim);
            }

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["AppSettings:Token"]!));

            SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            DateTimeOffset expiresAt = dateTimeProvider.Now.AddMinutes(5);
            JwtSecurityToken token = new JwtSecurityToken(
                claims: claims,
                expires: expiresAt.DateTime,
                signingCredentials: credentials);
            string jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return (jwt, expiresAt);
        }
    }
    
    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("application-users/login", async 
            (LoginRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            LoginCommand command = request.ToLoginCommand();
            LoginResponse response = await sender.Send(command, cancellationToken);
            return Results.Ok(response);
        });
    }
}

[Mapper]
public static partial class LoginMapper
{
    public static partial Login.LoginCommand ToLoginCommand(this Login.LoginRequest request);
}

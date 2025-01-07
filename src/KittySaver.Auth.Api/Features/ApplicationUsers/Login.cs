using System.Security.Authentication;
using FluentValidation;
using KittySaver.Auth.Api.Features.ApplicationUsers.SharedContracts;
using KittySaver.Auth.Api.Shared.Domain.Entites;
using KittySaver.Auth.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Auth.Api.Shared.Infrastructure.Endpoints;
using KittySaver.Auth.Api.Shared.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Auth.Api.Features.ApplicationUsers;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class Login : IEndpoint
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
        public LoginCommandValidator()
        {
            RuleFor(x => x.Password)
                .NotEmpty();
            RuleFor(x => x.Email)
                .NotEmpty()
                .Matches(ValidationPatterns.EmailPattern);
        }
    }

    internal sealed class LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService)
        : IRequestHandler<LoginCommand, LoginResponse>
    {
        public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            ApplicationUser user = await userManager.Users
                                       .AsNoTracking()
                                       .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken)
                                   ?? throw new ApplicationUser.Exceptions.ApplicationUserNotFoundException();

            bool areUserCredentialsValid = await userManager.CheckPasswordAsync(user, request.Password);

            if (!areUserCredentialsValid)
            {
                throw new AuthenticationException();
            }
            
            (string token, DateTimeOffset expiresAt) = await jwtTokenService.GenerateTokenAsync(user);
            
            return new LoginResponse
            {
                AccessToken = token,
                AccessTokenExpiresAt = expiresAt
            };
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("application-users/login", async (
            LoginRequest request,
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
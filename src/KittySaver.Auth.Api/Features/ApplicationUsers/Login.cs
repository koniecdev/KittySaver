using System.Security.Authentication;
using FluentValidation;
using KittySaver.Auth.Api.Features.ApplicationUsers.SharedContracts;
using KittySaver.Auth.Api.Shared.Abstractions;
using KittySaver.Auth.Api.Shared.Domain.Entites;
using KittySaver.Auth.Api.Shared.Endpoints;
using KittySaver.Auth.Api.Shared.Infrastructure.Services;
using KittySaver.Shared.Requests;
using KittySaver.Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;
using UnauthorizedAccessException = System.UnauthorizedAccessException;

namespace KittySaver.Auth.Api.Features.ApplicationUsers;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class Login : IEndpoint
{
    public sealed record LoginCommand(
        string Email,
        string Password) : ICommand<LoginResponse>;

    public sealed class LoginCommandValidator
        : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("'Hasło' nie może być puste");
            RuleFor(x => x.Email)
                .NotEmpty()
                // .WithMessage("'Email' cannot be empty.")
                .WithMessage("'Email' nie może być pusty.")
                .MaximumLength(255)
                // .WithMessage("'Email' must not exceed {MaxLength} characters.")
                .WithMessage("'Email' nie może przekraczać {MaxLength} znaków.")
                .Matches(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$")
                // .WithMessage("'Email' is not in the correct format.")
                .WithMessage("'Email' ma niepoprawny format.");
        }
    }

    internal sealed class LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService)
        : IRequestHandler<LoginCommand, LoginResponse>
    {
        public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            ApplicationUser user = await userManager.Users
                                       .AsNoTracking()
                                       .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken)
                                   ?? throw new AuthenticationException();

            bool areUserCredentialsValid = await userManager.CheckPasswordAsync(user, request.Password);

            if (!areUserCredentialsValid)
            {
                throw new AuthenticationException();
            }
            
            bool isEmailConfirmed = await userManager.IsEmailConfirmedAsync(user);

            if (!isEmailConfirmed)
            {
                throw new InvalidOperationException("Musisz najpierw potwierdzić swój mail. Sprawdź także folder SPAM.");
            }
            
            (string token, DateTimeOffset expiresAt) = await jwtTokenService.GenerateTokenAsync(user);
            RefreshToken refreshToken = await refreshTokenService.GenerateRefreshTokenAsync(user.Id, cancellationToken);
            
            return new LoginResponse
            {
                AccessToken = token,
                AccessTokenExpiresAt = expiresAt,
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiresAt = refreshToken.ExpiresAt
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
    public static partial Login.LoginCommand ToLoginCommand(this LoginRequest request);
}
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentValidation;
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

namespace KittySaver.Auth.Api.Features.ApplicationUsers;

public sealed class GetRefreshToken : IEndpoint
{
    public sealed record RefreshTokenCommand(
        string AccessToken,
        string RefreshToken) : ICommand<LoginResponse>;

    public sealed class RefreshTokenCommandValidator
        : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.AccessToken).NotEmpty();
            RuleFor(x => x.RefreshToken).NotEmpty();
        }
    }

    internal sealed class RefreshTokenCommandHandler(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService)
        : IRequestHandler<RefreshTokenCommand, LoginResponse>
    {
        public async Task<LoginResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            RefreshToken refreshToken = await refreshTokenService
                .ValidateRefreshTokenAsync(request.RefreshToken, cancellationToken);

            ApplicationUser user = refreshToken.ApplicationUser ?? 
                await userManager.Users
                    .FirstOrDefaultAsync(u => u.Id == refreshToken.ApplicationUserId, cancellationToken)
                ?? throw new ApplicationUser.Exceptions.ApplicationUserNotFoundException();

            (string token, DateTimeOffset expiresAt) = await jwtTokenService.GenerateTokenAsync(user);
            
            await refreshTokenService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);
            
            RefreshToken newRefreshToken = await refreshTokenService
                .GenerateRefreshTokenAsync(user.Id, cancellationToken);
            
            return new LoginResponse
            {
                AccessToken = token,
                AccessTokenExpiresAt = expiresAt,
                RefreshToken = newRefreshToken.Token,
                RefreshTokenExpiresAt = newRefreshToken.ExpiresAt
            };
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("application-users/refresh-token", async (
            RefreshTokenRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            RefreshTokenCommand command = request.ToRefreshTokenCommand();
            LoginResponse response = await sender.Send(command, cancellationToken);
            return Results.Ok(response);
        });
    }
}

[Mapper]
public static partial class RefreshTokenMapper
{
    public static partial GetRefreshToken.RefreshTokenCommand ToRefreshTokenCommand(this RefreshTokenRequest request);
}
using FluentValidation;
using KittySaver.Auth.Api.Shared.Abstractions;
using KittySaver.Auth.Api.Shared.Domain.Entites;
using KittySaver.Auth.Api.Shared.Endpoints;
using KittySaver.Auth.Api.Shared.Infrastructure.Services;
using KittySaver.Shared.Requests;
using MediatR;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Auth.Api.Features.ApplicationUsers;

public sealed class Logout : IEndpoint
{
    public sealed record LogoutCommand(
        string RefreshToken) : ICommand;

    public sealed class LogoutCommandValidator
        : AbstractValidator<LogoutCommand>
    {
        public LogoutCommandValidator()
        {
            RuleFor(x => x.RefreshToken).NotEmpty();
        }
    }

    internal sealed class LogoutCommandHandler(
        IRefreshTokenService refreshTokenService,
        ICurrentUserService currentUserService)
        : IRequestHandler<LogoutCommand>
    {
        public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await refreshTokenService.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);
            }
            catch (RefreshToken.Exceptions.RefreshTokenNotFoundException)
            {
            }

            if (Guid.TryParse(currentUserService.UserId, out Guid userId))
            {
                await refreshTokenService.RevokeAllUserRefreshTokensAsync(userId, cancellationToken);
            }
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("application-users/logout", async (
            LogoutRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            LogoutCommand command = request.ToLogoutCommand();
            await sender.Send(command, cancellationToken);
            return Results.Ok(new { message = "Wylogowano pomyślnie" });
        }).RequireAuthorization();
    }
}

[Mapper]
public static partial class LogoutMapper
{
    public static partial Logout.LogoutCommand ToLogoutCommand(this LogoutRequest request);
}
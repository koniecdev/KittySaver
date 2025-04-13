using FluentValidation;
using KittySaver.Auth.Api.Shared.Abstractions;
using KittySaver.Auth.Api.Shared.Domain.Entites;
using KittySaver.Auth.Api.Shared.Endpoints;
using KittySaver.Auth.Api.Shared.Exceptions;
using KittySaver.Auth.Api.Shared.Infrastructure.Services;
using KittySaver.Shared.Requests;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Auth.Api.Features.ApplicationUsers;

public sealed class ChangePassword : IEndpoint
{
    public sealed record ChangePasswordCommand(
        string CurrentPassword,
        string NewPassword) : ICommand;

    public sealed class ChangePasswordCommandValidator
        : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(x => x.CurrentPassword).NotEmpty();
            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .MinimumLength(8).WithMessage("'New Password' is not in the correct format. Your password length must be at least 8.")
                .Matches("[A-Z]+").WithMessage("'New Password' is not in the correct format. Your password must contain at least one uppercase letter.")
                .Matches("[a-z]+").WithMessage("'New Password' is not in the correct format. Your password must contain at least one lowercase letter.")
                .Matches("[0-9]+").WithMessage("'New Password' is not in the correct format. Your password must contain at least one number.");
        }
    }

    internal sealed class ChangePasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUserService,
        IRefreshTokenService refreshTokenService)
        : IRequestHandler<ChangePasswordCommand>
    {
        public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(currentUserService.UserId, out Guid userId))
            {
                throw new UnauthorizedAccessException();
            }

            ApplicationUser user = await userManager.FindByIdAsync(userId.ToString())
                ?? throw new NotFoundExceptions.ApplicationUserNotFoundException();

            IdentityResult result = await userManager.ChangePasswordAsync(
                user, 
                request.CurrentPassword, 
                request.NewPassword);
            
            if (!result.Succeeded)
            {
                string errorMessage = result.Errors.FirstOrDefault()?.Description ?? "Password change failed";
                throw new InvalidOperationException(errorMessage);
            }
            
            await refreshTokenService.RevokeAllUserRefreshTokensAsync(userId, cancellationToken);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("application-users/change-password", async (
            ChangePasswordRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            ChangePasswordCommand command = request.ToChangePasswordCommand();
            await sender.Send(command, cancellationToken);
            return Results.Ok(new { message = "Hasło zostało zmienione pomyślnie" });
        }).RequireAuthorization();
    }
}

[Mapper]
public static partial class ChangePasswordMapper
{
    public static partial ChangePassword.ChangePasswordCommand ToChangePasswordCommand(this ChangePasswordRequest request);
}
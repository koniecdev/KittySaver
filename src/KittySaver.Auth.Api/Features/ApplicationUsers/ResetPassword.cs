using FluentValidation;
using KittySaver.Auth.Api.Shared.Abstractions;
using KittySaver.Auth.Api.Shared.Domain.Entites;
using KittySaver.Auth.Api.Shared.Endpoints;
using KittySaver.Auth.Api.Shared.Exceptions;
using KittySaver.Shared.Requests;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Riok.Mapperly.Abstractions;
using System.Text;

namespace KittySaver.Auth.Api.Features.ApplicationUsers;

public sealed class ResetPassword : IEndpoint
{
    public sealed record ResetPasswordCommand(
        string Email,
        string Token,
        string Password) : ICommand;

    public sealed class ResetPasswordCommandValidator
        : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator()
        {
            RuleFor(x => x.Email).NotEmpty();
            RuleFor(x => x.Token).NotEmpty();
            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8).WithMessage("'Password' is not in the correct format. Your password length must be at least 8.")
                .Matches("[A-Z]+").WithMessage("'Password' is not in the correct format. Your password must contain at least one uppercase letter.")
                .Matches("[a-z]+").WithMessage("'Password' is not in the correct format. Your password must contain at least one lowercase letter.")
                .Matches("[0-9]+").WithMessage("'Password' is not in the correct format. Your password must contain at least one number.");
        }
    }

    internal sealed class ResetPasswordCommandHandler(
        UserManager<ApplicationUser> userManager)
        : IRequestHandler<ResetPasswordCommand>
    {
        public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            ApplicationUser? user = await userManager.FindByEmailAsync(request.Email)
                ?? throw new ApplicationUser.Exceptions.ApplicationUserNotFoundException();

            // Dekodujemy token
            string decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
            
            // Resetujemy hasło
            IdentityResult result = await userManager.ResetPasswordAsync(user, decodedToken, request.Password);
            
            if (!result.Succeeded)
            {
                string errorMessage = result.Errors.FirstOrDefault()?.Description ?? "Password reset failed";
                throw new BadRequestException("ApplicationUser.PasswordReset.Failed", errorMessage);
            }
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("application-users/reset-password", async (
            ResetPasswordRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            ResetPasswordCommand command = request.ToResetPasswordCommand();
            await sender.Send(command, cancellationToken);
            return Results.Ok(new { message = "Hasło zostało zresetowane pomyślnie" });
        });
    }
}

[Mapper]
public static partial class ResetPasswordMapper
{
    public static partial ResetPassword.ResetPasswordCommand ToResetPasswordCommand(this ResetPasswordRequest request);
}
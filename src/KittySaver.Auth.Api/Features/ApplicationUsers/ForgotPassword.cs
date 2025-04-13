using System.Net;
using FluentValidation;
using KittySaver.Auth.Api.Features.ApplicationUsers.SharedContracts;
using KittySaver.Auth.Api.Shared.Abstractions;
using KittySaver.Auth.Api.Shared.Domain.Entites;
using KittySaver.Auth.Api.Shared.Endpoints;
using KittySaver.Auth.Api.Shared.Infrastructure.Services.Email;
using KittySaver.Shared.Requests;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Riok.Mapperly.Abstractions;
using System.Text;
using KittySaver.Auth.Api.Shared.Exceptions;

namespace KittySaver.Auth.Api.Features.ApplicationUsers;

public sealed class ForgotPassword : IEndpoint
{
    public sealed record ForgotPasswordCommand(string Email) : ICommand;

    public sealed class ForgotPasswordCommandValidator
        : AbstractValidator<ForgotPasswordCommand>
    {
        public ForgotPasswordCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .Matches(ValidationPatterns.EmailPattern);
        }
    }

    internal sealed class ForgotPasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        IOptions<EmailSettings> emailSettings)
        : IRequestHandler<ForgotPasswordCommand>
    {
        public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            ApplicationUser? user = await userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                throw new NotFoundExceptions.ApplicationUserNotFoundException();
            }

            if (!await userManager.IsEmailConfirmedAsync(user))
            {
                throw new InvalidOperationException("Email nie jest potwierdzony.");
            }

            string token = await userManager.GeneratePasswordResetTokenAsync(user);
            string encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            
            string resetLink = $"{emailSettings.Value.WebsiteBaseUrl}/reset-password?email={WebUtility.UrlEncode(user.Email!)}&token={encodedToken}";
            
            await emailService.SendPasswordResetAsync(user.Email!, resetLink);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("application-users/forgot-password", async (
            ForgotPasswordRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            ForgotPasswordCommand command = request.ToForgotPasswordCommand();
            await sender.Send(command, cancellationToken);
            return Results.Ok(new { message = "Jeśli podany adres email istnieje w naszej bazie, wyślemy na niego instrukcje resetowania hasła." });
        });
    }
}

[Mapper]
public static partial class ForgotPasswordMapper
{
    public static partial ForgotPassword.ForgotPasswordCommand ToForgotPasswordCommand(this ForgotPasswordRequest request);
}
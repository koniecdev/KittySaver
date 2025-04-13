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

public sealed class ResendEmailConfirmation : IEndpoint
{
    public sealed record ResendEmailConfirmationCommand(
        string Email) : ICommand;

    public sealed class ResendEmailConfirmationCommandValidator
        : AbstractValidator<ResendEmailConfirmationCommand>
    {
        public ResendEmailConfirmationCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .Matches(ValidationPatterns.EmailPattern);
        }
    }

    internal sealed class ResendEmailConfirmationCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        IOptions<EmailSettings> emailSettings)
        : IRequestHandler<ResendEmailConfirmationCommand>
    {
        public async Task Handle(ResendEmailConfirmationCommand request, CancellationToken cancellationToken)
        {
            ApplicationUser? user = await userManager.FindByEmailAsync(request.Email);

            if (user is null)
            {
                throw new NotFoundExceptions.ApplicationUserNotFoundException();
            }

            if (await userManager.IsEmailConfirmedAsync(user))
            {
                return;
            }

            string token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            string encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            
            string confirmationLink = $"{emailSettings.Value.WebsiteBaseUrl}/confirm-email?userId={user.Id}&token={encodedToken}";
            
            await emailService.SendEmailConfirmationAsync(user.Email!, confirmationLink);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("application-users/resend-email-confirmation", async (
            ResendEmailConfirmationRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            ResendEmailConfirmationCommand command = request.ToResendEmailConfirmationCommand();
            await sender.Send(command, cancellationToken);
            return Results.Ok(new { message = "Jeśli podany adres email istnieje w naszej bazie i nie został jeszcze potwierdzony, wyślemy na niego nowy link potwierdzający." });
        });
    }
}

[Mapper]
public static partial class ResendEmailConfirmationMapper
{
    public static partial ResendEmailConfirmation.ResendEmailConfirmationCommand ToResendEmailConfirmationCommand(this ResendEmailConfirmationRequest request);
}
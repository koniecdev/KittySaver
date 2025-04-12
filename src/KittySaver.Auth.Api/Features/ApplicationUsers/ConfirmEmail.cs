using FluentValidation;
using KittySaver.Auth.Api.Shared.Abstractions;
using KittySaver.Auth.Api.Shared.Domain.Entites;
using KittySaver.Auth.Api.Shared.Endpoints;
using KittySaver.Shared.Requests;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Auth.Api.Features.ApplicationUsers;

public sealed class ConfirmEmail : IEndpoint
{
    public sealed record ConfirmEmailCommand(
        string UserId,
        string Token) : ICommand;

    public sealed class ConfirmEmailCommandValidator
        : AbstractValidator<ConfirmEmailCommand>
    {
        public ConfirmEmailCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.Token).NotEmpty();
        }
    }

    internal sealed class ConfirmEmailCommandHandler(
        UserManager<ApplicationUser> userManager)
        : IRequestHandler<ConfirmEmailCommand>
    {
        public async Task Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(request.UserId, out Guid userId))
            {
                throw new ApplicationUser.Exceptions.ApplicationUserNotFoundException();
            }

            ApplicationUser user = await userManager.FindByIdAsync(userId.ToString())
                ?? throw new ApplicationUser.Exceptions.ApplicationUserNotFoundException();

            string decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
            
            IdentityResult result = await userManager.ConfirmEmailAsync(user, decodedToken);
            
            if (!result.Succeeded)
            {
                string errorDescription = result.Errors.FirstOrDefault()?.Description ?? "Email confirmation failed";
                throw new InvalidOperationException(errorDescription);
            }
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("application-users/confirm-email", async (
            [FromQuery] string userId,
            [FromQuery] string token,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            ConfirmEmailCommand command = new(userId, token);
            await sender.Send(command, cancellationToken);
            return Results.Ok();
        });
    }
}

[Mapper]
public static partial class ConfirmEmailMapper
{
    public static partial ConfirmEmail.ConfirmEmailCommand ToConfirmEmailCommand(this ConfirmEmailRequest request);
}
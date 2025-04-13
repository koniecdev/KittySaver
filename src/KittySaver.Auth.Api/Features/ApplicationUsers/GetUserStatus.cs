using KittySaver.Auth.Api.Shared.Abstractions;
using KittySaver.Auth.Api.Shared.Domain.Entites;
using KittySaver.Auth.Api.Shared.Endpoints;
using KittySaver.Auth.Api.Shared.Exceptions;
using KittySaver.Auth.Api.Shared.Infrastructure.Services;
using KittySaver.Shared.Responses;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace KittySaver.Auth.Api.Features.ApplicationUsers;

public sealed class GetUserStatus : IEndpoint
{
    public sealed record GetUserStatusQuery : IQuery<UserStatusResponse>;

    internal sealed class GetUserStatusQueryHandler(
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetUserStatusQuery, UserStatusResponse>
    {
        public async Task<UserStatusResponse> Handle(GetUserStatusQuery request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(currentUserService.UserId, out Guid userId))
            {
                throw new UnauthorizedAccessException();
            }

            ApplicationUser user = await userManager.FindByIdAsync(userId.ToString())
                ?? throw new NotFoundExceptions.ApplicationUserNotFoundException();

            bool isEmailConfirmed = await userManager.IsEmailConfirmedAsync(user);
            IList<string> roles = await userManager.GetRolesAsync(user);

            return new UserStatusResponse
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber!,
                EmailConfirmed = isEmailConfirmed,
                Roles = roles.ToList()
            };
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("application-users/me", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            GetUserStatusQuery query = new();
            UserStatusResponse response = await sender.Send(query, cancellationToken);
            return Results.Ok(response);
        }).RequireAuthorization();
    }
}
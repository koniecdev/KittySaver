using KittySaver.Auth.Api.Features.ApplicationUsers.SharedContracts;
using KittySaver.Auth.Api.Shared.Domain.Entites;
using KittySaver.Auth.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Auth.Api.Shared.Infrastructure.Endpoints;
using KittySaver.Auth.Api.Shared.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Auth.Api.Features.ApplicationUsers;

public sealed class GetApplicationUser : IEndpoint
{
    public sealed record GetApplicationUserQuery(Guid Id) : IQuery<ApplicationUserResponse>;
    
    internal sealed class GetApplicationUserQueryHandler(ApplicationDbContext db)
        : IRequestHandler<GetApplicationUserQuery, ApplicationUserResponse>
    {
        public async Task<ApplicationUserResponse> Handle(GetApplicationUserQuery request, CancellationToken cancellationToken)
        {
            ApplicationUserResponse applicationUser = await db.ApplicationUsers
                .AsNoTracking()
                .Where(x=>x.Id == request.Id)
                .ProjectToDto()
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new ApplicationUser.Exceptions.ApplicationUserNotFoundException();
            return applicationUser;
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        // endpointRouteBuilder.MapGet("application-users/{id:guid}", async
        //     (Guid id,
        //      ISender sender,
        //      CancellationToken cancellationToken) =>
        // {
        //     GetApplicationUserQuery query = new(id);
        //     ApplicationUserResponse applicationUsers = await sender.Send(query, cancellationToken);
        //     return Results.Ok(applicationUsers);
        // }).RequireAuthorization();
    }
}
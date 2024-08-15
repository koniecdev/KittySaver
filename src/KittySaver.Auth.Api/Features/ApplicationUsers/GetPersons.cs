using KittySaver.Auth.Api.Features.ApplicationUsers.Contracts;
using KittySaver.Auth.Api.Shared.Domain.Entites;
using KittySaver.Auth.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Auth.Api.Shared.Infrastructure.Endpoints;
using KittySaver.Auth.Api.Shared.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Auth.Api.Features.ApplicationUsers;

public sealed class GetApplicationUsers : IEndpoint
{
    public sealed class GetApplicationUsersQuery : IQuery<ICollection<ApplicationUserResponse>>
    {
    }

    internal sealed class GetApplicationUsersQueryHandler(ApplicationDbContext db)
        : IRequestHandler<GetApplicationUsersQuery, ICollection<ApplicationUserResponse>>
    {
        public async Task<ICollection<ApplicationUserResponse>> Handle(GetApplicationUsersQuery request, CancellationToken cancellationToken)
        {
            List<ApplicationUserResponse> applicationUsers = await db.ApplicationUsers
                .AsNoTracking()
                .ProjectToDto()
                .ToListAsync(cancellationToken);
            return applicationUsers;
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("application-users", async
            (ISender sender,
            CancellationToken cancellationToken) =>
        {
            GetApplicationUsersQuery query = new();
            ICollection<ApplicationUserResponse> applicationUsers = await sender.Send(query, cancellationToken);
            return Results.Ok(applicationUsers);
        });
    }
}

[Mapper]
public static partial class GetApplicationUsersMapper
{
    public static partial IQueryable<ApplicationUserResponse> ProjectToDto(
        this IQueryable<ApplicationUser> applicationUsers);
}
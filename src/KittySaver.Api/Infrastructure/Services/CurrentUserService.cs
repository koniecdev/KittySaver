using System.Security.Authentication;
using System.Security.Claims;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Persons;
using KittySaver.Domain.Persons.Entities;
using KittySaver.Shared.Common.Enums;
using KittySaver.Shared.TypedIds;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Shared.Infrastructure.Services;

public interface ICurrentUserService
{
    public Guid GetCurrentUserIdentityId();
    public Task<CurrentlyLoggedInPerson?> GetCurrentlyLoggedInPersonAsync(CancellationToken cancellationToken);
    public Task EnsureUserIsAuthorizedAsync(PersonId personId, CancellationToken cancellationToken);
    public Task EnsureUserIsAdminAsync(CancellationToken cancellationToken);
}

public sealed class CurrentlyLoggedInPerson
{
    public required PersonId PersonId { get; init; }
    public required PersonRole Role { get; init; }
}

public sealed class CurrentUserService(
    IHttpContextAccessor httpContextAccessor,
    ICurrentEnvironmentService currentEnvironmentService,
    ApplicationReadDbContext dbContext)
    : ICurrentUserService
{
    private CurrentlyLoggedInPerson? _cachedUser;

    public async Task EnsureUserIsAuthorizedAsync(PersonId personId, CancellationToken cancellationToken)
    {
        CurrentlyLoggedInPerson? issuingUser = await GetCurrentlyLoggedInPersonAsync(cancellationToken);
        if (issuingUser is null || (issuingUser.PersonId != personId && issuingUser.Role is not PersonRole.Admin))
        {
            throw new UnauthorizedAccessException();
        }
    }

    public async Task EnsureUserIsAdminAsync(CancellationToken cancellationToken)
    {
        CurrentlyLoggedInPerson? loggedInUser = await GetCurrentlyLoggedInPersonAsync(cancellationToken);
        if (loggedInUser?.Role is not PersonRole.Admin)
        {
            throw new UnauthorizedAccessException();
        }
    }

    public Guid GetCurrentUserIdentityId()
    {
        bool isRegistrationEndpoint = httpContextAccessor.HttpContext?.Request.Path
                                          .ToString().EndsWith("/api/v1/persons", StringComparison.OrdinalIgnoreCase) == true
                                      && httpContextAccessor.HttpContext?.Request.Method == "POST";
    
        if (isRegistrationEndpoint)
        {
            return Guid.Empty;
        }
    
        return Guid.TryParse(httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier),
            out Guid userId)
            ? userId
            : currentEnvironmentService.IsDevelopmentTheCurrentEnvironment() 
                ? Guid.Empty 
                : throw new AuthenticationException($"User with UserIdentityId {userId} not found in database.");
    }
    
    public async Task<CurrentlyLoggedInPerson?> GetCurrentlyLoggedInPersonAsync(CancellationToken cancellationToken)
    {
        if (_cachedUser is not null)
        {
            return _cachedUser;
        }
        
        if (currentEnvironmentService.IsTestingTheCurrentEnvironment())
        {
            return new CurrentlyLoggedInPerson
            {
                PersonId = new PersonId(),
                Role = PersonRole.Admin
            };
        }
        
        bool success = Guid.TryParse(
            httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier),
            out Guid userId);
        
        if (!success)
        {
            return null;
        }

        _cachedUser = await dbContext.Persons
            .Where(x => x.UserIdentityId == userId)
            .Select(x => new CurrentlyLoggedInPerson
            {
                PersonId = x.Id,
                Role = (PersonRole)x.CurrentRole
            }).FirstOrDefaultAsync(cancellationToken);

        return _cachedUser;
    }
}

public sealed class DesignTimeMigrationsCurrentUserService : ICurrentUserService
{
    public Guid GetCurrentUserIdentityId() => Guid.Empty;
    public async Task<CurrentlyLoggedInPerson?> GetCurrentlyLoggedInPersonAsync(CancellationToken cancellationToken)
    {
        return await Task.FromResult(new CurrentlyLoggedInPerson { PersonId = PersonId.Empty, Role = PersonRole.Admin });
    }
    public async Task EnsureUserIsAuthorizedAsync(PersonId personId, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
    public async Task EnsureUserIsAdminAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}
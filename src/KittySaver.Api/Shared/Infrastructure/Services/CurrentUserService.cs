using System.Security.Authentication;
using System.Security.Claims;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Persons;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Shared.Infrastructure.Services;

public interface ICurrentUserService
{
    public Guid GetCurrentUserIdentityId();
    public Task EnsureUserIsAuthorizedAsync(Guid personThatIsBeingModifiedIdOrUserIdentityId);
    public Task EnsureUserIsAdminAsync();
}

public sealed class CurrentlyLoggedUser
{
    public required Guid Id { get; init; }
    public required Person.Role Role { get; init; }
}

public sealed class CurrentUserService(
    IHttpContextAccessor httpContextAccessor,
    ICurrentEnvironmentService currentEnvironmentService,
    ApplicationReadDbContext dbContext)
    : ICurrentUserService
{
    private CurrentlyLoggedUser? _cachedUser;

    public async Task EnsureUserIsAuthorizedAsync(Guid personThatIsBeingModifiedIdOrUserIdentityId)
    {
        if (GetCurrentUserIdentityId() == personThatIsBeingModifiedIdOrUserIdentityId)
        {
            return;
        }
        
        CurrentlyLoggedUser issuingUser = await GetCurrentlyLoggedUserAsync();
        if (issuingUser.Id != personThatIsBeingModifiedIdOrUserIdentityId && issuingUser.Role is not Person.Role.Admin)
        {
            throw new UnauthorizedAccessException();
        }
    }

    public async Task EnsureUserIsAdminAsync()
    {
        CurrentlyLoggedUser loggedInUser = await GetCurrentlyLoggedUserAsync();
        if (loggedInUser.Role is not Person.Role.Admin)
        {
            throw new UnauthorizedAccessException();
        }
    }

    public Guid GetCurrentUserIdentityId()
    {
        return Guid.TryParse(httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier),
            out Guid userId)
            ? userId
            : currentEnvironmentService.IsDevelopmentTheCurrentEnvironment() 
                ? Guid.Empty 
                : throw new AuthenticationException($"User with UserIdentityId {userId} not found in database.");
    }
    
    private async Task<CurrentlyLoggedUser> GetCurrentlyLoggedUserAsync()
    {
        if (_cachedUser is not null)
        {
            return _cachedUser;
        }
        
        if (currentEnvironmentService.IsDevelopmentTheCurrentEnvironment())
        {
            return new CurrentlyLoggedUser
            {
                Id = Guid.NewGuid(),
                Role = Person.Role.Admin
            };
        }
        
        bool success = Guid.TryParse(
            httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier),
            out Guid userId);
        
        if (!success)
        {
            throw new AuthenticationException();
        }

        _cachedUser = await dbContext.Persons
            .Where(x => x.UserIdentityId == userId)
            .Select(x => new CurrentlyLoggedUser
            {
                Id = x.Id,
                Role = (Person.Role)x.CurrentRole
            }).FirstOrDefaultAsync() ?? throw new AuthenticationException();

        return _cachedUser;
    }
}

public sealed class DesignTimeMigrationsCurrentUserService : ICurrentUserService
{
    public Guid GetCurrentUserIdentityId() => Guid.Empty;
    public async Task EnsureUserIsAuthorizedAsync(Guid personThatIsBeingModifiedIdOrUserIdentityId)
    {
        await Task.CompletedTask;
    }
    public async Task EnsureUserIsAdminAsync()
    {
        await Task.CompletedTask;
    }
}
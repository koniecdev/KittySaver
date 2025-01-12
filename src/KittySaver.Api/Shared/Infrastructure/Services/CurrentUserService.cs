using System.Security.Authentication;
using System.Security.Claims;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Persons;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Shared.Infrastructure.Services;

public interface ICurrentUserService
{
    public Guid GetCurrentUserIdentityId();
    public Task<CurrentlyLoggedInPerson?> GetCurrentlyLoggedInPersonAsync(CancellationToken cancellationToken);
    public Task EnsureUserIsAuthorizedAsync(Guid personThatIsBeingModifiedIdOrUserIdentityId, CancellationToken cancellationToken);
    public Task EnsureUserIsAdminAsync(CancellationToken cancellationToken);
}

public sealed class CurrentlyLoggedInPerson
{
    public required Guid PersonId { get; init; }
    public required Person.Role Role { get; init; }
}

public sealed class CurrentUserService(
    IHttpContextAccessor httpContextAccessor,
    ICurrentEnvironmentService currentEnvironmentService,
    ApplicationReadDbContext dbContext)
    : ICurrentUserService
{
    private CurrentlyLoggedInPerson? _cachedUser;

    public async Task EnsureUserIsAuthorizedAsync(Guid personThatIsBeingModifiedIdOrUserIdentityId, CancellationToken cancellationToken)
    {
        if (GetCurrentUserIdentityId() == personThatIsBeingModifiedIdOrUserIdentityId)
        {
            return;
        }
        
        CurrentlyLoggedInPerson? issuingUser = await GetCurrentlyLoggedInPersonAsync(cancellationToken);
        if (issuingUser is null || (issuingUser.PersonId != personThatIsBeingModifiedIdOrUserIdentityId && issuingUser.Role is not Person.Role.Admin))
        {
            throw new UnauthorizedAccessException();
        }
    }

    public async Task EnsureUserIsAdminAsync(CancellationToken cancellationToken)
    {
        CurrentlyLoggedInPerson? loggedInUser = await GetCurrentlyLoggedInPersonAsync(cancellationToken);
        if (loggedInUser?.Role is not Person.Role.Admin)
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
                PersonId = Guid.NewGuid(),
                Role = Person.Role.Admin
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
                Role = (Person.Role)x.CurrentRole
            }).FirstOrDefaultAsync(cancellationToken);
                      // ?? throw new AuthenticationException();

        return _cachedUser;
    }
}

public sealed class DesignTimeMigrationsCurrentUserService : ICurrentUserService
{
    public Guid GetCurrentUserIdentityId() => Guid.Empty;
    public async Task<CurrentlyLoggedInPerson?> GetCurrentlyLoggedInPersonAsync(CancellationToken cancellationToken)
    {
        return await Task.FromResult(new CurrentlyLoggedInPerson { PersonId = Guid.Empty, Role = Person.Role.Admin });
    }
    public async Task EnsureUserIsAuthorizedAsync(Guid personThatIsBeingModifiedIdOrUserIdentityId, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
    public async Task EnsureUserIsAdminAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}
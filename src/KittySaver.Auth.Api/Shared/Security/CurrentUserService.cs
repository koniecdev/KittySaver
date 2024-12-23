using System.Security.Authentication;
using System.Security.Claims;
using KittySaver.Auth.Api.Shared.Infrastructure.Services;

namespace KittySaver.Auth.Api.Shared.Security;

public interface ICurrentUserService
{
    public string UserId { get; }
    public ICollection<string> UserRoles { get; }
}

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor, ICurrentEnvironmentService currentEnvironmentService)
    : ICurrentUserService
{
    private const string NotLoggedInValueForDevelopment = "NotLoggedInWhileDev";

    public string UserId
    {
        get
        {
            string? userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                return userId;
            }
            
            if (!currentEnvironmentService.IsDevelopmentTheCurrentEnvironment())
            {
                throw new AuthenticationException();
            }
            
            userId = NotLoggedInValueForDevelopment;
            return userId;
        }
    }
    
    public ICollection<string> UserRoles
    {
        get
        {
            ICollection<string> userRoles =
                httpContextAccessor.HttpContext?
                    .User
                    .Claims
                    .Where(x=>x.Type == ClaimTypes.Role)
                    .Select(x=>x.Value)
                    .ToList() ?? [];

            return userRoles;
        }
    }
}

public sealed class DesignTimeMigrationsCurrentUserService : ICurrentUserService
{
    private const string DesignTimeMigrationValue = "Migration";

    public string UserId => DesignTimeMigrationValue;
    public ICollection<string> UserRoles => new List<string>();
}
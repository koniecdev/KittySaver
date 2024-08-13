using System.Security.Claims;
using KittySaver.Api.Shared.Infrastructure.Services;

namespace KittySaver.Api.Shared.Security;

public interface ICurrentUserService
{
    public string UserId { get; }
}

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor, ICurrentEnvironmentService currentEnvironmentService) : ICurrentUserService
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
                throw new UnauthorizedAccessException();
            }
            
            userId = NotLoggedInValueForDevelopment;
            return userId;
        }
    }
}

public sealed class DesignTimeMigrationsCurrentUserService : ICurrentUserService
{
    private const string DesignTimeMigrationValue = "Migration";

    public string UserId => DesignTimeMigrationValue;
}
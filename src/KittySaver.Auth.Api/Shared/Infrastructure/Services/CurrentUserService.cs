﻿using System.Security.Claims;

namespace KittySaver.Auth.Api.Shared.Infrastructure.Services;

public interface ICurrentUserService
{
    public string UserId { get; }
    public ICollection<string> UserRoles { get; }
}

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
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
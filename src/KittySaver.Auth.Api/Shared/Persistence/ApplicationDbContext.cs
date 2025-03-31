using KittySaver.Auth.Api.Shared.Domain.Entites;
using KittySaver.Auth.Api.Shared.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KittySaver.Auth.Api.Shared.Persistence;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService) 
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(Program).Assembly);
    }
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    foreach (EntityEntry<AuditableEntity> entity in ChangeTracker.Entries<AuditableEntity>())
    {
        // Sprawdź, czy to jest przypadek tworzenia nowego RefreshToken
        bool isRefreshTokenCreation = entity.Entity is RefreshToken refreshToken && 
                                     entity.State == EntityState.Added;

        switch (entity.State)
        {
            case EntityState.Added:
                entity.Entity.CreatedOn = dateTimeProvider.Now;
                
                // Obsługa przypadku tworzenia tokenu podczas logowania
                if (isRefreshTokenCreation)
                {
                    // Bezpośrednio używamy ID użytkownika z RefreshToken zamiast pobierać z kontekstu
                    RefreshToken token = (RefreshToken)entity.Entity;
                    entity.Entity.CreatedBy = token.ApplicationUserId.ToString();
                }
                else
                {
                    try
                    {
                        // Standardowa obsługa dla innych encji
                        entity.Entity.CreatedBy = currentUserService.UserId;
                    }
                    catch (System.Security.Authentication.AuthenticationException)
                    {
                        // Awaryjnie, jeśli nie ma zalogowanego użytkownika
                        entity.Entity.CreatedBy = "System";
                    }
                }
                break;
                
            case EntityState.Modified:
                entity.Entity.LastModificationOn = dateTimeProvider.Now;
                
                try
                {
                    entity.Entity.LastModificationBy = currentUserService.UserId;
                }
                catch (System.Security.Authentication.AuthenticationException)
                {
                    // Awaryjnie, jeśli nie ma zalogowanego użytkownika
                    entity.Entity.LastModificationBy = "System";
                }
                break;
                
            case EntityState.Detached:
            case EntityState.Unchanged:
            case EntityState.Deleted:
            default:
                break;
        }
    }
    
    return base.SaveChangesAsync(cancellationToken);
}
}
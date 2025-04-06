using KittySaver.Auth.Api.Shared.Domain.Entites;
using KittySaver.Auth.Api.Shared.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KittySaver.Auth.Api.Shared.Persistence;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IDateTimeProvider dateTimeProvider) 
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
        switch (entity.State)
        {
            case EntityState.Added:
                entity.Entity.CreatedAt = dateTimeProvider.Now;
                break;
                
            case EntityState.Modified:
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
using KittySaver.Api.Shared.Domain.Entites;
using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Api.Shared.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KittySaver.Api.Shared.Persistence;

internal sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService) 
    : IdentityDbContext<Person, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Person> Persons => Set<Person>();
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
                    entity.Entity.CreatedOn = dateTimeProvider.Now;
                    entity.Entity.CreatedBy = currentUserService.UserId;
                    break;
                case EntityState.Detached:
                case EntityState.Unchanged:
                case EntityState.Deleted:
                case EntityState.Modified:
                default:
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
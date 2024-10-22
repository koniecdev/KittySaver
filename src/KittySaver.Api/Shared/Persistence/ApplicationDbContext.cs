using KittySaver.Api.Shared.Domain.Entites;
using KittySaver.Api.Shared.Domain.Entites.Common;
using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Api.Shared.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SmartEnum.EFCore;

namespace KittySaver.Api.Shared.Persistence;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IDateTimeService dateTimeProvider,
    ICurrentUserService currentUserService) 
    : DbContext(options)
{
    public DbSet<Person> Persons => Set<Person>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(Program).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.ConfigureSmartEnum();
        base.ConfigureConventions(configurationBuilder);
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
                case EntityState.Modified:
                    entity.Entity.LastModificationOn = dateTimeProvider.Now;
                    entity.Entity.LastModificationBy = currentUserService.UserId;
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
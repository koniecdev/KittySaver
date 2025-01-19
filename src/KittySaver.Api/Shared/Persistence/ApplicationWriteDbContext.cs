using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Api.Shared.Persistence.ReadModels;
using KittySaver.Domain;
using KittySaver.Domain.Common.Primitives;
using KittySaver.Domain.Persons;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SmartEnum.EFCore;

namespace KittySaver.Api.Shared.Persistence;

public sealed class ApplicationWriteDbContext(
    DbContextOptions<ApplicationWriteDbContext> options,
    IDateTimeService dateTimeProvider,
    ICurrentUserService currentUserService,
    IPublisher? publisher = null)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Person> Persons => Set<Person>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(IDomainMarker).Assembly);
    }
    
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.ConfigureSmartEnum();
        base.ConfigureConventions(configurationBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (EntityEntry<AuditableEntity> entity in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entity.State)
            {
                case EntityState.Added:
                    entity.Entity.CreatedOn = dateTimeProvider.Now;
                    entity.Entity.CreatedBy = currentUserService.GetCurrentUserIdentityId().ToString();
                    break;
                case EntityState.Modified:
                    entity.Entity.LastModificationOn = dateTimeProvider.Now;
                    entity.Entity.LastModificationBy = currentUserService.GetCurrentUserIdentityId().ToString();
                    break;
                case EntityState.Detached:
                case EntityState.Unchanged:
                case EntityState.Deleted:
                default:
                    break;
            }
        }

        if (publisher is null)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        await PublishDomainEvents(cancellationToken);
        
        return await base.SaveChangesAsync(cancellationToken);
    }
    
    private async Task PublishDomainEvents(CancellationToken cancellationToken)
    {
        if (publisher is null)
        {
            return;
        }
        
        List<EntityEntry<AggregateRoot>> aggregateRootsEntryListQuery = ChangeTracker
            .Entries<AggregateRoot>()
            .Where(entityEntry => entityEntry.Entity.GetDomainEvents().Count != 0)
            .ToList();

        List<DomainEvent> domainEvents = aggregateRootsEntryListQuery.SelectMany(entityEntry => entityEntry.Entity.GetDomainEvents()).ToList();

        foreach (EntityEntry<AggregateRoot> aggregateRootEntry in aggregateRootsEntryListQuery)
        {
            aggregateRootEntry.Entity.ClearDomainEvents();
        }

        foreach (DomainEvent domainEvent in domainEvents)
        {
            await publisher.Publish(domainEvent, cancellationToken);
        }
    }
}
using KittySaver.Api.Infrastructure.Services;
using KittySaver.Domain;
using KittySaver.Domain.Common.Primitives;
using KittySaver.Domain.Persons.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SmartEnum.EFCore;

namespace KittySaver.Api.Persistence.WriteRelated;

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
        configurationBuilder.RegisterAllStronglyTypedIdConverters();
        configurationBuilder.ConfigureSmartEnum();
        base.ConfigureConventions(configurationBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (EntityEntry<IAuditableEntity> entity in ChangeTracker.Entries<IAuditableEntity>())
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
        
        List<EntityEntry<IAggregateRoot>> aggregateRootsEntryListQuery = ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(entityEntry => entityEntry.Entity.GetDomainEvents().Count != 0)
            .ToList();

        List<DomainEvent> domainEvents = aggregateRootsEntryListQuery.SelectMany(entityEntry => entityEntry.Entity.GetDomainEvents()).ToList();

        foreach (EntityEntry<IAggregateRoot> aggregateRootEntry in aggregateRootsEntryListQuery)
        {
            aggregateRootEntry.Entity.ClearDomainEvents();
        }

        foreach (DomainEvent domainEvent in domainEvents)
        {
            await publisher.Publish(domainEvent, cancellationToken);
        }
    }
}
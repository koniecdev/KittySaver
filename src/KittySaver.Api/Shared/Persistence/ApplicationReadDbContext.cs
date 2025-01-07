using KittySaver.Api.Shared.Persistence.ReadModels;
using KittySaver.Domain;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Shared.Persistence;

public sealed class ApplicationReadDbContext(
    DbContextOptions<ApplicationReadDbContext> options)
    : DbContext(options)
{
    public DbSet<PersonReadModel> Persons => Set<PersonReadModel>();
    public DbSet<CatReadModel> Cats => Set<CatReadModel>();
    public DbSet<AdvertisementReadModel> Advertisements => Set<AdvertisementReadModel>();
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(IDomainMarker).Assembly, ReadConfigurationFilter);
    }

    private static bool ReadConfigurationFilter(Type type) => type.IsAssignableTo(typeof(IReadConfiguration));
}
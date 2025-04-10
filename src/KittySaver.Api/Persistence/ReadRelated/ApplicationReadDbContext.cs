using KittySaver.ReadModels.EntityFramework;
using KittySaver.ReadModels.PersonAggregate;
using Microsoft.EntityFrameworkCore;
using SmartEnum.EFCore;

namespace KittySaver.Api.Persistence.ReadRelated;

public sealed class ApplicationReadDbContext(
    DbContextOptions<ApplicationReadDbContext> options)
    : DbContext(options)
{
    public DbSet<PersonReadModel> Persons => Set<PersonReadModel>();
    public DbSet<CatReadModel> Cats => Set<CatReadModel>();
    public DbSet<AdvertisementReadModel> Advertisements => Set<AdvertisementReadModel>();
    
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.RegisterAllStronglyTypedIdConverters();
        configurationBuilder.ConfigureSmartEnum();
        base.ConfigureConventions(configurationBuilder);
    }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(IReadConfiguration).Assembly);
    }
}
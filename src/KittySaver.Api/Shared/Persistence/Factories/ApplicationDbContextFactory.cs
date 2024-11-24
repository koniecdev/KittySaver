using KittySaver.Api.Shared.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Shared.Persistence.Factories;

internal sealed class ApplicationDbContextFactoryFactory : DesignTimeDbContextFactoryBase<ApplicationDbContext>
{
    protected override ApplicationDbContext CreateNewInstance(DbContextOptions<ApplicationDbContext> options)
    {
        ApplicationDbContext db = new ApplicationDbContext(options, new DefaultDateTimeService(), new DesignTimeMigrationsCurrentUserService());
        return db;
    }
}

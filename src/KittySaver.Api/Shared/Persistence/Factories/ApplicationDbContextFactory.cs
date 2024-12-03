using KittySaver.Api.Shared.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Shared.Persistence.Factories;

internal sealed class ApplicationDbContextFactoryFactory : DesignTimeDbContextFactoryBase<ApplicationWriteDbContext>
{
    protected override ApplicationWriteDbContext CreateNewInstance(DbContextOptions<ApplicationWriteDbContext> options)
    {
        ApplicationWriteDbContext writeDb = new ApplicationWriteDbContext(options, new DefaultDateTimeService(), new DesignTimeMigrationsCurrentUserService());
        return writeDb;
    }
}

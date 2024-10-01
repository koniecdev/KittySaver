using KittySaver.Auth.Api.Shared.Infrastructure.Services;
using KittySaver.Auth.Api.Shared.Security;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Auth.Api.Shared.Persistence.Factories;

internal sealed class ApplicationDbContextFactoryFactory : DesignTimeDbContextFactoryBase<ApplicationDbContext>
{
    protected override ApplicationDbContext CreateNewInstance(DbContextOptions<ApplicationDbContext> options)
    {
        ApplicationDbContext db = new ApplicationDbContext(options, new DefaultDateTimeProvider(), new DesignTimeMigrationsCurrentUserService());
        return db;
    }
}

using KittySaver.Auth.Api.Shared.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Auth.Api.Shared.Persistence.Factories;

internal sealed class ApplicationDbContextFactoryFactory : DesignTimeDbContextFactoryBase<ApplicationDbContext>
{
    protected override ApplicationDbContext CreateNewInstance(DbContextOptions<ApplicationDbContext> options)
    {
        ApplicationDbContext db = new ApplicationDbContext(options, new DefaultDateTimeProvider());
        return db;
    }
}

using KittySaver.Api.Shared.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Shared.Persistence.Factories;

internal sealed class ApplicationDbContextFactoryFactory : DesignTimeDbContextFactoryBase<ApplicationDbContext>
{
    protected override ApplicationDbContext CreateNewInstance(DbContextOptions<ApplicationDbContext> options)
    {
        var db = new ApplicationDbContext(options, new DefaultDateTimeProvider());
        return db;
    }
}

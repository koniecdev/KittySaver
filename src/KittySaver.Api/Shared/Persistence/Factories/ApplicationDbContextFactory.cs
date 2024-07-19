using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Shared.Persistence.Factories;

internal sealed class ApplicationDbContextFactoryFactory : DesignTimeDbContextFactoryBase<ApplicationDbContext>
{
    protected override ApplicationDbContext CreateNewInstance(DbContextOptions<ApplicationDbContext> options)
    {
        var db = new ApplicationDbContext(options);
        return db;
    }
}

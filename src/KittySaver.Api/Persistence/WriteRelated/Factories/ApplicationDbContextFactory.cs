using KittySaver.Api.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Persistence.WriteRelated.Factories;

internal sealed class ApplicationDbContextFactoryFactory : DesignTimeDbContextFactoryBase<ApplicationWriteDbContext>
{
    protected override ApplicationWriteDbContext CreateNewInstance(DbContextOptions<ApplicationWriteDbContext> options)
    {
        ApplicationWriteDbContext writeDb = new ApplicationWriteDbContext(options, new DefaultDateTimeService());
        return writeDb;
    }
}

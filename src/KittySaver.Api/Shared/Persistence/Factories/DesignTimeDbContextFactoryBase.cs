using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KittySaver.Api.Shared.Persistence.Factories;

internal abstract class DesignTimeDbContextFactoryBase<TContext> :
    IDesignTimeDbContextFactory<TContext> where TContext : DbContext

{
    private const string ConnectionStringName = "Database";

    public TContext CreateDbContext(string[] args)
    {
        string? environmentName =
            Environment.GetEnvironmentVariable(
                "ASPNETCORE_ENVIRONMENT");

        DirectoryInfo? dir = Directory.GetParent(AppContext.BaseDirectory);

        FileInfo? appsettingsDir = dir?.GetFiles("appsettings.json", SearchOption.AllDirectories).First();

        string? basePath = appsettingsDir?.Directory?.FullName;

        ArgumentNullException.ThrowIfNull(basePath);

        return Create(basePath, environmentName ?? string.Empty);
    }

    protected abstract TContext CreateNewInstance(DbContextOptions<TContext> options);

    private TContext Create(string basePath, string environmentName)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Local.json", optional: true)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        
        string connectionString = configuration.GetConnectionString(ConnectionStringName)
                                  ?? throw new Exception($"ConnectionString {ConnectionStringName} could not be found");

        return Create(connectionString);
    }

    private TContext Create(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException($"Connection string '{ConnectionStringName}' is null or empty.", nameof(connectionString));
        }

        Console.WriteLine($"DesignTimeDbContextFactoryBase.Create(string): Connection string: '{connectionString}'.");

        DbContextOptionsBuilder<TContext> optionsBuilder = new DbContextOptionsBuilder<TContext>();

        optionsBuilder.UseSqlServer(connectionString);

        return CreateNewInstance(optionsBuilder.Options);
    }

}
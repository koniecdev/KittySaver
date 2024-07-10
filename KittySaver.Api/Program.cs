using KittySaver.Api.Extensions;
using Serilog;

IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom
    .Configuration(configuration)
    .CreateLogger();

try
{
    Log.Information("Application is starting up");
    
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();
    
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerServices();

    WebApplication app = builder.Build();
    
    if (app.Environment.IsDevelopment())
    {
        app.AddSwagger();
    }

    app.UseHttpsRedirection();
    app.UseSerilogRequestLogging();

    app.Run();
}
catch(Exception exception)
{
    Log.Fatal(exception, "Could not start application");
}
finally
{
    Log.CloseAndFlush();
}

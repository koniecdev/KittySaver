using KittySaver.Api.Exceptions;
using KittySaver.Api.Extensions;
using KittySaver.Api.Middlewares;
using KittySaver.Api.Middlewares.ExceptionHandlers;
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

    builder.Services.AddEveryExceptionHandler();
    builder.Services.AddProblemDetails();
    
    WebApplication app = builder.Build();
    app.UseExceptionHandler();
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

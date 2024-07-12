using Asp.Versioning;
using Asp.Versioning.Builder;
using KittySaver.Api.Exceptions;
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

    builder.Services.AddEveryExceptionHandler();

    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1);
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    }).AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'V";
        options.SubstituteApiVersionInUrl = true;
    });
    
    WebApplication app = builder.Build();
    app.UseExceptionHandler();
    app.UseHttpsRedirection();
    app.UseSerilogRequestLogging();

    ApiVersionSet apiVersionSet = app.NewApiVersionSet()
        .HasApiVersion(new ApiVersion(1))
        .ReportApiVersions()
        .Build();
    RouteGroupBuilder versionedGroup = app
        .MapGroup("api/v{apiVersion:apiVersion}")
        .WithApiVersionSet(apiVersionSet);

    versionedGroup.MapGet("hello", () => "Hello!");
    
    if (app.Environment.IsDevelopment())
    {
        app.AddSwagger();
    }
    
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

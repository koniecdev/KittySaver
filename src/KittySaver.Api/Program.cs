using System.Reflection;
using Asp.Versioning;
using Asp.Versioning.Builder;
using KittySaver.Api.Shared.Endpoints;
using KittySaver.Api.Shared.Exceptions;
using KittySaver.SharedForApi.Swagger;
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
    builder.Services.RegisterInfrastructureServices(builder.Configuration, builder.Environment);

    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1);
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    }).AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'V";
        options.SubstituteApiVersionInUrl = true;
    });
    
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DevelopmentPolicy",
            corsBuilder =>
            {
                corsBuilder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            });
        options.AddPolicy("ProductionPolicy",
            corsBuilder =>
            {
                corsBuilder.WithOrigins("https://uratujkota.koniec.dev").AllowAnyHeader().AllowAnyMethod();
                corsBuilder.WithOrigins("https://uratujkota.pl").AllowAnyHeader().AllowAnyMethod();
                corsBuilder.WithOrigins("https://auth.uratujkota.pl").AllowAnyHeader().AllowAnyMethod();
            });
    });

    builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());
    builder.AddServiceDefaults();

    WebApplication app = builder.Build();
    app.UseExceptionHandler();
    app.UseHttpsRedirection();
    app.UseSerilogRequestLogging();
    string corsPolicy = app.Environment.EnvironmentName == "Development" ? "DevelopmentPolicy" : "ProductionPolicy";
    app.UseCors(corsPolicy);
    app.UseAuthentication();
    app.UseAuthorization();

    ApiVersionSet apiVersionSet = app.NewApiVersionSet()
        .HasApiVersion(new ApiVersion(1))
        .ReportApiVersions()
        .Build();
    RouteGroupBuilder versionedGroup = app
        .MapGroup("api/v{apiVersion:apiVersion}")
        .WithApiVersionSet(apiVersionSet);

    app.MapEndpoints(versionedGroup);

    if (app.Environment.IsDevelopment())
    {
        app.MapDefaultEndpoints();
        app.AddSwagger();
    }


    app.Run();
}
catch (Exception exception)
{
    if (exception is HostAbortedException)
    {
        return;
    }

    Log.Fatal(exception, "Could not start application");
}
finally
{
    Log.CloseAndFlush();
}

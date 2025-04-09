namespace KittySaver.Api.Infrastructure.Services;

public interface ICurrentEnvironmentService
{
    public bool IsDevelopmentTheCurrentEnvironment();
    public bool IsTestingTheCurrentEnvironment();
}

public sealed class CurrentEnvironmentService(IConfiguration configuration) : ICurrentEnvironmentService
{
    public bool IsDevelopmentTheCurrentEnvironment() => configuration["ASPNETCORE_ENVIRONMENT"] == "Development";
    public bool IsTestingTheCurrentEnvironment() => configuration["ASPNETCORE_ENVIRONMENT"] == "Testing";
}
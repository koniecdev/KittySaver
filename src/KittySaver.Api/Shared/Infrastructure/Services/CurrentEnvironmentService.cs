namespace KittySaver.Api.Shared.Infrastructure.Services;

public interface ICurrentEnvironmentService
{
    public bool IsDevelopmentTheCurrentEnvironment();
}

public sealed class CurrentEnvironmentService(IConfiguration configuration) : ICurrentEnvironmentService
{
    public bool IsDevelopmentTheCurrentEnvironment() => configuration["ASPNETCORE_ENVIRONMENT"] == "Development";
}
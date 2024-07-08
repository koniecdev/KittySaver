namespace KittySaver.Api.Extensions;

internal static class SwaggerExtensions
{
    internal static IServiceCollection AddSwaggerServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSwaggerGen();
        return serviceCollection;
    }

    internal static WebApplication AddSwagger(this WebApplication webApplication)
    {
        webApplication.UseSwagger();
        webApplication.UseSwaggerUI();
        return webApplication;
    }
}
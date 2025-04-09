using Asp.Versioning.ApiExplorer;

namespace KittySaver.Auth.Api.Swagger.Swagger;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSwaggerGen();
        serviceCollection.ConfigureOptions<ConfigureSwaggerGenOptions>();
        return serviceCollection;
    }

    public static WebApplication AddSwagger(this WebApplication webApplication)
    {
        webApplication.UseSwagger();
        webApplication.UseSwaggerUI(options =>
        {
            IReadOnlyList<ApiVersionDescription> descriptions = webApplication.DescribeApiVersions();

            foreach (ApiVersionDescription description in descriptions)
            {
                string url = $"/swagger/{description.GroupName}/swagger.json";
                string name = description.GroupName.ToUpperInvariant();
                
                options.SwaggerEndpoint(url, name);
            }
        });
        return webApplication;
    }
}
using Asp.Versioning.ApiExplorer;
using KittySaver.Api.Shared.OpenApi;

namespace KittySaver.Api.Shared.Extensions;

internal static class SwaggerExtensions
{
    internal static IServiceCollection AddSwaggerServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSwaggerGen();
        serviceCollection.ConfigureOptions<ConfigureSwaggerGenOptions>();
        return serviceCollection;
    }

    internal static WebApplication AddSwagger(this WebApplication webApplication)
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
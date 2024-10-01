using System.Reflection;
using KittySaver.Auth.Api.Shared.Infrastructure.Endpoints;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace KittySaver.Auth.Api.Shared.Extensions;

public static class EndpointExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        ServiceDescriptor[] endpointServiceDescriptors = assembly
            .DefinedTypes
            .Where(type =>
                type is { IsAbstract: false, IsInterface: false }
                && type.IsAssignableTo(typeof(IEndpoint)))
            .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))
            .ToArray();
        
        services.TryAddEnumerable(endpointServiceDescriptors);
        
        return services;
    }

    public static IApplicationBuilder MapEndpoints(this WebApplication app, RouteGroupBuilder? routeGroupBuilder = null)
    {
        IEnumerable<IEndpoint> endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();

        IEndpointRouteBuilder builder = routeGroupBuilder is null ? app : routeGroupBuilder;
        
        foreach (IEndpoint endpoint in endpoints)
        {
            endpoint.MapEndpoint(builder);
        }
        return app;
    }
}
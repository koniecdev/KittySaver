using KittySaver.Api.Middlewares.ExceptionHandlers;

namespace KittySaver.Api.Extensions;

internal static class ExceptionHandlersExtensions
{
    internal static IServiceCollection AddEveryExceptionHandler(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddExceptionHandler<NotFoundExceptionHandler>();
        serviceCollection.AddExceptionHandler<BadRequestExceptionHandler>();
        serviceCollection.AddExceptionHandler<GlobalExceptionHandler>();
        return serviceCollection;
    }
}


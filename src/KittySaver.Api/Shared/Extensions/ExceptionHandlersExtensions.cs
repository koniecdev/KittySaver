using KittySaver.Api.Shared.Infrastructure.ExceptionHandlers;

namespace KittySaver.Api.Shared.Extensions;

internal static class ExceptionHandlersExtensions
{
    internal static IServiceCollection AddEveryExceptionHandler(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddExceptionHandler<NotFoundExceptionHandler>();
        serviceCollection.AddExceptionHandler<BadRequestExceptionHandler>();
        serviceCollection.AddExceptionHandler<GlobalExceptionHandler>();
        serviceCollection.AddProblemDetails();

        return serviceCollection;
    }
}


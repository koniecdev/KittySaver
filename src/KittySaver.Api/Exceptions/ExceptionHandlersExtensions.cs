using KittySaver.Api.Shared.Exceptions.Handlers;

namespace KittySaver.Api.Shared.Exceptions;

internal static class ExceptionHandlersExtensions
{
    internal static IServiceCollection AddEveryExceptionHandler(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddExceptionHandler<UnauthenticatedAccessExceptionHandler>();
        serviceCollection.AddExceptionHandler<UnauthorizedAccessExceptionHandler>();
        serviceCollection.AddExceptionHandler<ValidationExceptionHandler>();
        serviceCollection.AddExceptionHandler<NotFoundExceptionHandler>();
        serviceCollection.AddExceptionHandler<DomainExceptionHandler>();
        serviceCollection.AddExceptionHandler<GlobalExceptionHandler>();
        serviceCollection.AddProblemDetails();

        return serviceCollection;
    }
}


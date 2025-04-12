using KittySaver.Auth.Api.Shared.Exceptions.Handlers;

namespace KittySaver.Auth.Api.Shared.Exceptions;

internal static class ExceptionHandlersExtensions
{
    internal static IServiceCollection AddEveryExceptionHandler(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddExceptionHandler<UnauthenticatedExceptionHandler>();
        serviceCollection.AddExceptionHandler<UnauthorizedAccessExceptionHandler>();
        serviceCollection.AddExceptionHandler<DomainExceptionHandler>();
        serviceCollection.AddExceptionHandler<ValidationExceptionHandler>();
        serviceCollection.AddExceptionHandler<NotFoundExceptionHandler>();
        serviceCollection.AddExceptionHandler<GlobalExceptionHandler>();
        serviceCollection.AddProblemDetails();

        return serviceCollection;
    }
}


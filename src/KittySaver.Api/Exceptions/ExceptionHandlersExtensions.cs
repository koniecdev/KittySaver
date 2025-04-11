using KittySaver.Api.Exceptions.Handlers;

namespace KittySaver.Api.Exceptions;

internal static class ExceptionHandlersExtensions
{
    internal static IServiceCollection AddEveryExceptionHandler(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddExceptionHandler<UnauthenticatedAccessExceptionHandler>();
        serviceCollection.AddExceptionHandler<UnauthorizedAccessExceptionHandler>();
        serviceCollection.AddExceptionHandler<ApiValidationExceptionHandler>();
        serviceCollection.AddExceptionHandler<ValidationExceptionHandler>();
        serviceCollection.AddExceptionHandler<NotFoundExceptionHandler>();
        serviceCollection.AddExceptionHandler<ApiExceptionHandler>();
        serviceCollection.AddExceptionHandler<DomainExceptionHandler>();
        serviceCollection.AddExceptionHandler<GlobalExceptionHandler>();
        serviceCollection.AddProblemDetails();

        return serviceCollection;
    }
}


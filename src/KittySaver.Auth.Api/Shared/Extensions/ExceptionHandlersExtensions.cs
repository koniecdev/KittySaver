using KittySaver.Auth.Api.Shared.Exceptions;
using KittySaver.Auth.Api.Shared.Infrastructure.ExceptionHandlers;

namespace KittySaver.Auth.Api.Shared.Extensions;

internal static class ExceptionHandlersExtensions
{
    internal static IServiceCollection AddEveryExceptionHandler(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddExceptionHandler<ValidationExceptionHandler>();
        serviceCollection.AddExceptionHandler<IdentityResultExceptionHandler>();
        serviceCollection.AddExceptionHandler<NotFoundExceptionHandler>();
        serviceCollection.AddExceptionHandler<BadRequestExceptionHandler>();
        serviceCollection.AddExceptionHandler<GlobalExceptionHandler>();
        serviceCollection.AddProblemDetails();

        return serviceCollection;
    }
}


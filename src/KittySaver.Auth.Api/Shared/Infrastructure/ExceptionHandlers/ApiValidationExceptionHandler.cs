using KittySaver.Auth.Api.Shared.Exceptions;
using KittySaver.Auth.Api.Shared.Infrastructure.Clients;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace KittySaver.Auth.Api.Shared.Infrastructure.ExceptionHandlers;

internal sealed class ApiValidationExceptionHandler(ILogger<ApiValidationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ApiValidationException apiValidationException)
        {
            return false;
        }
        logger.LogError(
            apiValidationException,
            "Following errors occurred: {type} | {code} | {description}",
            apiValidationException.ValidationProblemDetails.Status!.Value,
            apiValidationException.ValidationProblemDetails.Title,
            apiValidationException.ValidationProblemDetails.Detail);
        
        httpContext.Response.StatusCode = apiValidationException.ValidationProblemDetails.Status.Value;
        await httpContext.Response
            .WriteAsJsonAsync(apiValidationException.ValidationProblemDetails, cancellationToken);
        return true;
    }
}
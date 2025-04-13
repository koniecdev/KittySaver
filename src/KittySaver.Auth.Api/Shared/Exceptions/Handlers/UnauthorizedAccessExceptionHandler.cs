using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace KittySaver.Auth.Api.Shared.Exceptions.Handlers;

internal sealed class UnauthorizedAccessExceptionHandler(ILogger<UnauthorizedAccessExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not UnauthorizedAccessException)
        {
            return false;
        }
        
        ProblemDetails problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Type = "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/403",
            Title = "Błąd autentykacji",
            Detail = "Nie masz uprawnień do tego zasobu."
        };

        logger.LogError(
            exception, "Unauthorized Exception occurred: {status} | {type} | {message}",
            problemDetails.Status.Value,
            exception.GetType().Name,
            exception.Message);

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
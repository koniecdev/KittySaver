using System.Security.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace KittySaver.Api.Exceptions.Handlers;

internal sealed class UnauthenticatedAccessExceptionHandler(ILogger<UnauthenticatedAccessExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not AuthenticationException)
        {
            return false;
        }
        
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Type = "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/401",
            // Title = "You have to log in first."
            Title = "Musisz najpierw się zalogować."
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
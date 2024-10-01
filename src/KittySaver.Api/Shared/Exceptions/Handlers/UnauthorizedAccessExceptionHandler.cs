using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace KittySaver.Api.Shared.Exceptions.Handlers;

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
        
        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Provided credentials are not valid"
        };

        logger.LogError(
            exception, "Unauthorized Exception occurred: {type} | {message}",
            problemDetails.Status.Value,
            exception.Message);

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}

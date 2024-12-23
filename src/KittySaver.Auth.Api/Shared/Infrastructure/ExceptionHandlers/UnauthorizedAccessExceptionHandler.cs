using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace KittySaver.Auth.Api.Shared.Infrastructure.ExceptionHandlers;

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
            Status = StatusCodes.Status403Forbidden,
            Title = "You do not have permission to modify resource that do not belong to You."
        };

        logger.LogError(
            exception, "Bad Login Exception occurred: {type} | {message}",
            problemDetails.Status.Value,
            exception.Message);

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}

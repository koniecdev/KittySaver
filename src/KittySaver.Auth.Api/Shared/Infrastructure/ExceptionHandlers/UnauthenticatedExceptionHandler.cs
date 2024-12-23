using System.Security.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace KittySaver.Auth.Api.Shared.Infrastructure.ExceptionHandlers;

internal sealed class UnauthenticatedExceptionHandler(ILogger<UnauthenticatedExceptionHandler> logger) : IExceptionHandler
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
        
        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Provided credentials are not valid"
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

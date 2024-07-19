using KittySaver.Api.Shared.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace KittySaver.Api.Shared.Infrastructure.ExceptionHandlers;

internal sealed class BadRequestExceptionHandler(ILogger<BadRequestExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not BadRequestException badRequestException)
        {
            return false;
        }

        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/400",
            Title = "One or more request processing validation errors occurred",
            Extensions = new Dictionary<string, object?>
            {
                {
                    "errors", 
                    new IApplicationError[]
                    {
                        badRequestException
                    }
                }
            }
        };

        logger.LogError(
            badRequestException,
            "Following errors occurred: {type} | {code} | {description}",
            problemDetails.Status.Value,
            badRequestException.ApplicationCode,
            badRequestException.Description);

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
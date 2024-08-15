using KittySaver.Auth.Api.Shared.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace KittySaver.Auth.Api.Shared.Infrastructure.ExceptionHandlers;

internal sealed class NotFoundExceptionHandler(ILogger<NotFoundExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not NotFoundException notFoundException)
        {
            return false;
        }

        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status404NotFound,
            Type = "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/404",
            Title = "Resource could not be found",
            Extensions = new Dictionary<string, object?>
            {
                {
                    "errors",
                    new IApplicationError[]
                    {
                        notFoundException
                    }
                }
            }
        };

        logger.LogError(
            notFoundException,
            "Following errors occurred: {type} | {code} | {description}",
            problemDetails.Status.Value,
            notFoundException.ApplicationCode,
            notFoundException.Description);

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}

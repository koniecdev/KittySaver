using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace KittySaver.Api.Shared.Exceptions.Handlers;

internal sealed class DomainExceptionHandler(ILogger<DomainExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not DomainException domainException)
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
                    new Dictionary<string, string[]>
                    {
                        {
                            domainException.ApplicationCode,
                            [domainException.Message]
                        }
                    }
                }
            }
        };

        logger.LogError(
            domainException,
            "Following errors occurred: {type} | {code} | {message}",
            problemDetails.Status.Value,
            domainException.ApplicationCode,
            domainException.Message);

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
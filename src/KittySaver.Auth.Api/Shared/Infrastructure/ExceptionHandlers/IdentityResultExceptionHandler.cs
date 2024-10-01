using System.Text.Json;
using KittySaver.Auth.Api.Shared.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace KittySaver.Auth.Api.Shared.Infrastructure.ExceptionHandlers;

internal sealed class IdentityResultExceptionHandler(ILogger<IdentityResultExceptionHandler> logger) : IExceptionHandler
{
    private class IdentityResultError : IApplicationError
    {
        public required string ApplicationCode { get; init; }
        public required string Description { get; init; }
    }
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not IdentityResultException identityResultException)
        {
            return false;
        }
        
        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status500InternalServerError,
            Type = "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/500",
            Title = "Something went wrong with managing user.",
            Extensions = new Dictionary<string, object?>
            {
                {
                    "errors",
                    identityResultException.IdentityErrors.Select(x=> new IdentityResultError
                    {
                        ApplicationCode = x.Code,
                        Description = x.Description
                    }).ToList()
                }
            }
        };
        
        logger.LogError(
            identityResultException,
            "Following identityresult errors occurred: {type} | {code} | {description}",
            problemDetails.Status.Value,
            "IdentityError",
            JsonSerializer.Serialize(identityResultException.IdentityErrors)
            );

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
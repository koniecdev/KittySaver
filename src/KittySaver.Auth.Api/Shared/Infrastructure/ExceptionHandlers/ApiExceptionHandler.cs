using KittySaver.Auth.Api.Shared.Exceptions;
using KittySaver.Auth.Api.Shared.Infrastructure.Clients;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace KittySaver.Auth.Api.Shared.Infrastructure.ExceptionHandlers;

internal sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ApiException apiException)
        {
            return false;
        }
        logger.LogError(
            apiException,
            "Following errors occurred: {type} | {code} | {description}",
            apiException.ProblemDetails.Status!.Value,
            apiException.ProblemDetails.Title,
            apiException.ProblemDetails.Detail);
        
        httpContext.Response.StatusCode = apiException.ProblemDetails.Status.Value;
        await httpContext.Response
            .WriteAsJsonAsync(apiException.ProblemDetails, cancellationToken);
        return true;
    }
}
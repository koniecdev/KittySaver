using System.Text.Json;
using KittySaver.Api.Infrastructure.Clients;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace KittySaver.Api.Exceptions.Handlers;

internal sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ApiException apiValidationEx)
        {
            return false;
        }
        
        ProblemDetails problemDetails = apiValidationEx.ProblemDetails;
        
        logger.LogError(
            exception,
            "API Validation error occurred: {type} | {code} | {message}",
            problemDetails.Status,
            exception.GetType().Name,
            exception.Message);

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status400BadRequest;
        
        await httpContext.Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
using System.Text.Json;
using KittySaver.Api.Infrastructure.Clients;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace KittySaver.Api.Exceptions.Handlers;

internal sealed class ApiValidationExceptionHandler(ILogger<ApiValidationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ApiValidationException apiValidationEx)
        {
            return false;
        }
        
        ValidationProblemDetails problemDetails = apiValidationEx.ValidationProblemDetails;
        
        logger.LogError(
            exception,
            "API Validation error occurred: {type} | {code} | {message}",
            problemDetails.Status,
            exception.GetType().Name,
            JsonSerializer.Serialize(problemDetails.Errors));

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status400BadRequest;
        
        await httpContext.Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
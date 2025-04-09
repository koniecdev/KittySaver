using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace KittySaver.Api.Exceptions.Handlers;

internal sealed class ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException || !validationException.Errors.Any())
        {
            return false;
        }
        
        Dictionary<string, string[]> failedProperties = validationException.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        
        ValidationProblemDetails problemDetails = new(failedProperties)
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/400",
            Title = "One or more request processing validation errors occurred"
        };
        
        logger.LogError(
            validationException,
            "Following errors occurred: {type} | {code} | {message}",
            problemDetails.Status.Value,
            exception.GetType().Name,
            JsonSerializer.Serialize(validationException.Errors));

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
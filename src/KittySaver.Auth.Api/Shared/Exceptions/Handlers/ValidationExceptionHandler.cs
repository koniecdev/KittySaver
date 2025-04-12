using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace KittySaver.Auth.Api.Shared.Exceptions.Handlers;

internal sealed class ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger) : IExceptionHandler
{
    private class ValidationError : IApplicationError
    {
        public required string ApplicationCode { get; init; }
        public required string Description { get; init; }
    }
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException || !validationException.Errors.Any())
        {
            return false;
        }
        
        Dictionary<string, string[]> errors = new();
        
        List<string> failedProperties = validationException.Errors
            .DistinctBy(x => x.PropertyName)
            .Select(x=>x.PropertyName)
            .ToList();
        
        foreach (string propertyName in failedProperties)
        {
            string[] errorMessagesOfGivenProperty = validationException.Errors
                .Where(x => x.PropertyName == propertyName)
                .Select(x=>x.ErrorMessage)
                .ToArray();
            errors.Add(propertyName, errorMessagesOfGivenProperty);
        }
        
        ValidationProblemDetails problemDetails = new(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/400",
            Title = "One or more request processing validation errors occurred"
        };
        
        logger.LogError(
            validationException,
            "Following errors occurred: {type} | {code} | {description}",
            problemDetails.Status.Value,
            "FluentValidationException",
            JsonSerializer.Serialize(validationException.Errors)
            );

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        await httpContext.Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
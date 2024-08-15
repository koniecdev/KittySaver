using System.Text;
using System.Text.Json;
using FluentValidation;
using KittySaver.Api.Shared.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace KittySaver.Api.Shared.Infrastructure.ExceptionHandlers;

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
        if (exception is not ValidationException validationException)
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
                    validationException.Errors.Select(x=> new ValidationError
                    {
                        ApplicationCode = $"ValidationException.{x.PropertyName}",
                        Description = x.ErrorMessage
                    }).ToList()
                }
            }
        };
        
        logger.LogError(
            validationException,
            "Following errors occurred: {type} | {code} | {description}",
            problemDetails.Status.Value,
            "FluentValidationException",
            JsonSerializer.Serialize(validationException.Errors)
            );

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
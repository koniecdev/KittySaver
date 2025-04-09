using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;

namespace KittySaver.Wasm.Shared.Extensions;

public static class ResultExtensions
{
    private const StringComparison StringComparer = StringComparison.CurrentCultureIgnoreCase;
    public static ICollection<string> ExtractErrorMessagesForProperty(this IResult result, string propertyToSearchFor)
    {
        return result.ValidationErrors
            .Where(x => string.Equals(x.Identifier, propertyToSearchFor, StringComparer))
            .Select(x=>x.ErrorMessage.Replace("'", ""))
            .ToList();
    }
}

public static class ProblemDetailsExtensions
{
    public static IEnumerable<ValidationError> ToValidationErrors(this ProblemDetails problemDetails)
    {
        if (problemDetails is not ValidationProblemDetails validationProblemDetails || validationProblemDetails.Errors.Count == 0)
        {
            ValidationError validationError = new(problemDetails.Title, problemDetails.Detail);
            yield return validationError;
            yield break;
        }
        foreach ((string? propertyErrorName, string[]? propertyErrors) in validationProblemDetails.Errors)
        {
            foreach (string propertyError in propertyErrors)
            {
                ValidationError validationError = new(propertyErrorName, propertyError);
                yield return validationError;
            }
        }
    }
}
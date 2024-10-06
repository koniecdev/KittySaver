using FluentValidation;
using FluentValidation.Results;

namespace KittySaver.Api.Shared.Domain.Enums.Common;

public static class SmartEnumsExceptions
{
    public class InvalidValueException(IEnumerable<(string propertyName, string attemptedValue)> validationExceptions)
        : ValidationException(validationExceptions.Select(x =>
            new ValidationFailure(x.propertyName, "Provided invalid value.", x.attemptedValue)));
}
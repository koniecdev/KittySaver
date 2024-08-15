using FluentValidation;
using FluentValidation.Results;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using MediatR;

namespace KittySaver.Api.Shared.Behaviours;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommandBase
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ValidationContext<TRequest> context = new ValidationContext<TRequest>(request);

        List<ValidationResult> validationFailures = validators
            .Select(validator => validator.Validate(context))
            .ToList();

        var errors = validationFailures
            .Where(validationResult => !validationResult.IsValid)
            .SelectMany(validationResult => validationResult.Errors)
            .Where(m => m is not null)
            .ToList();

        if (errors.Count != 0)
        {
            throw new ValidationException(errors);
        }

        TResponse response = await next();

        return response;
    }
}

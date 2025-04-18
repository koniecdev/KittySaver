﻿using FluentValidation;
using FluentValidation.Results;
using KittySaver.Auth.Api.Shared.Abstractions;
using MediatR;

namespace KittySaver.Auth.Api.Shared.Behaviours;

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

        List<IValidator<TRequest>> syncValidators = [];
        List<IValidator<TRequest>> asyncValidators = [];
        
        foreach (IValidator<TRequest> validator in validators)
        {
            if (validator is IAsyncValidator)
            {
                asyncValidators.Add(validator);
                continue;
            }
            syncValidators.Add(validator);
        }
        
        List<ValidationResult> syncValidatorsFailures = syncValidators
            .Select(validator => validator.Validate(context))
            .ToList();
        ValidationResult[] asyncValidatorsFailures = await Task.WhenAll(
            asyncValidators
                .Select(validator => validator.ValidateAsync(context, cancellationToken)));

        List<ValidationResult> validationResults = syncValidatorsFailures.Concat(asyncValidatorsFailures).ToList();

        List<ValidationFailure> errors = validationResults
            .Where(validationResult => !validationResult.IsValid)
            .SelectMany(validationResult => validationResult.Errors)
            .Where(m => m is not null)
            .ToList();

        if (errors.Count != 0)
        {
            throw new ValidationException(errors);
        }

        TResponse response = await next(cancellationToken);

        return response;
    }
}

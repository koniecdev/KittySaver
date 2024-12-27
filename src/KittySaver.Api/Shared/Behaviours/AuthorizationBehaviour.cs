using FluentValidation;
using FluentValidation.Results;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Infrastructure.Services;
using MediatR;

namespace KittySaver.Api.Shared.Behaviours;

public sealed class AuthorizationBehaviour<TRequest, TResponse>(ICurrentUserService currentUserService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IPersonAggregateAuthorizationRequiredRequest
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IAdminOnlyQuery<TResponse>)
        {
            Guid personId = request switch
            {
                IPersonAggregatePersonIdBase x => x.PersonId,
                IIPersonAggregateUserIdentityIdBase x => x.UserIdentityId,
                IIPersonAggregateIdOrUserIdentityIdBase x => x.IdOrUserIdentityId,
                _ => throw new InvalidOperationException("Report it to admin, there is something wrong with behaviour.")
            };
            await currentUserService.EnsureUserIsAuthorizedAsync(personId);
        }
        else
        {
            await currentUserService.EnsureUserIsAdminAsync();
        }
        
        TResponse response = await next();

        return response;
    }
}

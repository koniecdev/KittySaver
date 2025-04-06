using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Domain.Persons;
using KittySaver.Domain.Persons.Entities;
using KittySaver.Shared.Common.Enums;
using KittySaver.Shared.TypedIds;
using MediatR;

namespace KittySaver.Api.Shared.Behaviours;

public sealed class AuthorizationBehaviour<TRequest, TResponse>(
    ICurrentUserService currentUserService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IAuthenticationBasedRequest
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is IJobOrAdminOnlyRequest)
        {
            CurrentlyLoggedInPerson? person = await currentUserService.GetCurrentlyLoggedInPersonAsync(cancellationToken);
            if (person is not { Role: PersonRole.Job or PersonRole.Admin })
            {
                throw new UnauthorizedAccessException();
            }
        }
        else if (request is not IAdminOnlyRequest && request is IAuthorizedRequest)
        {
            PersonId personId = request switch
            {
                ICatRequest x => x.PersonId,
                IPersonRequest x => x.Id,
                IAdvertisementRequest x => x.PersonId,
                _ => throw new InvalidOperationException("Report it to admin, there is something wrong with behaviour.")
            };
            await currentUserService.EnsureUserIsAuthorizedAsync(personId, cancellationToken);
        }
        else
        {
            await currentUserService.EnsureUserIsAdminAsync(cancellationToken);
        }
        
        TResponse response = await next();

        return response;
    }
}

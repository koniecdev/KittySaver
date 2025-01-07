using KittySaver.Api.Features.Persons.SharedContracts;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Endpoints;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Persons;

public sealed class GetPerson : IEndpoint
{
    public sealed record GetPersonQuery(Guid IdOrUserIdentityId) : IQuery<PersonResponse>, IAuthorizedRequest, IPersonRequest;

    internal sealed class GetPersonQueryHandler(
        ApplicationReadDbContext db)
        : IRequestHandler<GetPersonQuery, PersonResponse>
    {
        public async Task<PersonResponse> Handle(GetPersonQuery request, CancellationToken cancellationToken)
        {
            PersonResponse person =
                await db.Persons
                    .Where(x => x.Id == request.IdOrUserIdentityId || x.UserIdentityId == request.IdOrUserIdentityId)
                    .ProjectToDto()
                    .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundExceptions.PersonNotFoundException(request.IdOrUserIdentityId);
            
            return person;
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("persons/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            GetPersonQuery query = new(id);
            PersonResponse person = await sender.Send(query, cancellationToken);
            return Results.Ok(person);
        }).RequireAuthorization()
        .WithName(EndpointNames.GetPerson.EndpointName)
        .WithTags(EndpointNames.GroupNames.PersonGroup);
    }
}
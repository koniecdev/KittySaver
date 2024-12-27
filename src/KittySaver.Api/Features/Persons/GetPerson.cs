using KittySaver.Api.Features.Persons.SharedContracts;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Persons;

public sealed class GetPerson : IEndpoint
{
    public sealed record GetPersonQuery(Guid IdOrUserIdentityId) : IPersonQuery<PersonResponse>;

    internal sealed class GetPersonQueryHandler(
        ApplicationReadDbContext db,
        ILinkService linkService)
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
            
            AddLinks(person);
            
            return person;
        }

        private void AddLinks(PersonResponse personResponse)
        {
            personResponse.Links.Add(
                linkService.Generate(
                    endpointInfo: EndpointNames.GetPerson,
                    routeValues: new { id = personResponse.Id },
                    isSelf: true));
            
            personResponse.Links.Add(
                linkService.Generate(
                    endpointInfo: EndpointNames.UpdatePerson,
                    routeValues: new { id = personResponse.Id }));
    
            personResponse.Links.Add(
                linkService.Generate(
                    endpointInfo: EndpointNames.DeletePerson,
                    routeValues: new { id = personResponse.Id }));
    
            personResponse.Links.Add(
                linkService.Generate(
                    endpointInfo: EndpointNames.GetCats,
                    routeValues: new { personId = personResponse.Id }));
            
            personResponse.Links.Add(
                linkService.Generate(
                    endpointInfo: EndpointNames.CreateCat,
                    routeValues: new { personId = personResponse.Id }));

            personResponse.Links.Add(
                linkService.Generate(
                    endpointInfo: EndpointNames.GetAdvertisements));
            
            personResponse.Links.Add(
                linkService.Generate(
                    endpointInfo: EndpointNames.GetPersonAdvertisements,
                    routeValues: new { personId = personResponse.Id }));
            
            personResponse.Links.Add(
                linkService.Generate(
                    endpointInfo: EndpointNames.CreateAdvertisement,
                    routeValues: new { personId = personResponse.Id }));
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
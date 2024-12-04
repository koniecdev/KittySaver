using KittySaver.Api.Features.Persons.SharedContracts;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Persons;

public sealed class GetPersons : IEndpoint
{
    public sealed class GetPersonsQuery : IQuery<ICollection<PersonResponse>>;

    internal sealed class GetPersonsQueryHandler(ApplicationReadDbContext db)
        : IRequestHandler<GetPersonsQuery, ICollection<PersonResponse>>
    {
        public async Task<ICollection<PersonResponse>> Handle(GetPersonsQuery request, CancellationToken cancellationToken)
        {
            List<PersonResponse> persons = await db.Persons
                .AsNoTracking()
                .ProjectToDto()
                .ToListAsync(cancellationToken);
            return persons;
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("persons", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            GetPersonsQuery query = new();
            ICollection<PersonResponse> persons = await sender.Send(query, cancellationToken);
            return Results.Ok(persons);
        });
    }
}
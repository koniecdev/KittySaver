using KittySaver.Api.Features.Persons.Contracts;
using KittySaver.Api.Shared.Domain.Entites;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Infrastructure.Endpoints;
using KittySaver.Api.Shared.Persistence;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Api.Features.Persons;

public sealed class GetPersons : IEndpoint
{
    public sealed class GetPersonQuery : IRequest<ICollection<PersonResponse>>
    {
        public List<ApiFilter>? Filters { get; init; } = [];
        public List<ApiSort>? Sorts { get; init; } = [];
    }

    internal sealed class GetPersonQueryHandler(ApplicationDbContext db)
        : IRequestHandler<GetPersonQuery, ICollection<PersonResponse>>
    {
        public async Task<ICollection<PersonResponse>> Handle(GetPersonQuery request, CancellationToken cancellationToken)
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
        endpointRouteBuilder.MapGet("persons", async
            (string? filterBy,
            string? sortBy,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            GetPersonQuery query = new();
            var persons = await sender.Send(query, cancellationToken);
            return Results.Ok(persons);
        });
    }
}

[Mapper]
public static partial class GetPersonsMapper
{
    public static partial IQueryable<PersonResponse> ProjectToDto(
        this IQueryable<Person> persons);
}
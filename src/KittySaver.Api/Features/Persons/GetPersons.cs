using KittySaver.Api.Features.Persons.SharedContracts;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Api.Shared.Persistence.ReadModels;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Persons;

public sealed class GetPersons : IEndpoint
{
    public sealed class GetPersonsQuery(int? offset, int? limit) : IAdminOnlyQuery<PagedList<PersonResponse>>
    {
        public int? Offset { get; } = offset;
        public int? Limit { get; } = limit;
    }

    internal sealed class GetPersonsQueryHandler(
        ApplicationReadDbContext db,
        IPaginationLinksService paginationLinksService)
        : IRequestHandler<GetPersonsQuery, PagedList<PersonResponse>>
    {
        public async Task<PagedList<PersonResponse>> Handle(GetPersonsQuery request, CancellationToken cancellationToken)
        {
            IQueryable<PersonReadModel> query = db.Persons;
            int totalRecords = await query.CountAsync(cancellationToken);
            
            if (request.Offset.HasValue)
            {
                query = query.Skip(request.Offset.Value);
            }
            if (request.Limit.HasValue)
            {
                query = query.Take(request.Limit.Value);
            }
            
            List<PersonResponse> persons = 
                await query
                    .ProjectToDto()
                    .ToListAsync(cancellationToken);

            PagedList<PersonResponse> response = new()
            {
                Items = persons,
                Total = totalRecords,
                Links = paginationLinksService.GeneratePaginationLinks(
                    EndpointNames.GetPersons.EndpointName,
                    request.Offset,
                    request.Limit,
                    totalRecords)
            };
            
            return response;
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("persons", async (
            int? offset,
            int? limit,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            GetPersonsQuery query = new(offset, limit);
            PagedList<PersonResponse> persons = await sender.Send(query, cancellationToken);
            return Results.Ok(persons);
        }).RequireAuthorization()
        .WithName(EndpointNames.GetPersons.EndpointName)
        .WithTags(EndpointNames.GroupNames.PersonGroup);
    }
}
using System.Linq.Expressions;
using KittySaver.Api.Features.Persons.SharedContracts;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Contracts;
using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Api.Shared.Persistence.ReadModels;
using KittySaver.Domain.Persons;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Persons;

public sealed class GetPersons : IEndpoint
{
    public sealed class GetPersonsQuery(
        int? offset,
        int? limit,
        string? searchTerm,
        string? sortColumn,
        string? sortOrder) : IQuery<IPagedList<PersonResponse>>, IAdminOnlyRequest, IPagedQuery
    {
        public int? Offset { get; } = offset;
        public int? Limit { get; } = limit;
        public string? SearchTerm { get; } = searchTerm;
        public string? SortColumn { get; } = sortColumn;
        public string? SortOrder { get; } = sortOrder;
    }
    
    internal sealed class GetPersonsQueryHandler(
        ApplicationReadDbContext db,
        IPaginationLinksService paginationLinksService)
        : IRequestHandler<GetPersonsQuery, IPagedList<PersonResponse>>
    {
        public async Task<IPagedList<PersonResponse>> Handle(GetPersonsQuery request, CancellationToken cancellationToken)
        {
            IQueryable<PersonReadModel> query = db.Persons;
            int totalRecords = await query.CountAsync(cancellationToken);
            
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                IEnumerable<FilterCriteria> filters = request.SearchTerm
                    .Split(',')
                    .Select(FilterCriteria.Parse);
                
                query = query.ApplyFilters(filters);
            }
            
            query = request.SortOrder?.ToLower() == "desc" 
                ? query.OrderByDescending(GetSortProperty(request)) 
                : query.OrderBy(GetSortProperty(request));
            
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

        private static Expression<Func<PersonReadModel, object>> GetSortProperty(GetPersonsQuery request)
            => request.SortColumn?.ToLower() switch
            {
                "nickname" => person => person.Nickname,
                "currentrole" => person => person.CurrentRole,
                "email" => person => person.Email,
                "phonenumber" => person => person.PhoneNumber,
                _ => person => person.CurrentRole
            };
    }
    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("persons", async (
                int? offset,
                int? limit,
                string? searchTerm,
                string? sortColumn,
                string? sortOrder,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                GetPersonsQuery query = new(offset, limit, searchTerm, sortColumn, sortOrder);
                IPagedList<PersonResponse> persons = await sender.Send(query, cancellationToken);
                return Results.Ok(persons);
            }).RequireAuthorization()
            .WithName(EndpointNames.GetPersons.EndpointName)
            .WithTags(EndpointNames.GroupNames.PersonGroup);
    }
}



public static class PersonFilters
{
    private static readonly IPropertyFilter<PersonReadModel>[] Filters =
    [
        // String filters
        new StringPropertyFilter<PersonReadModel>(p => p.Nickname),
        new StringPropertyFilter<PersonReadModel>(p => p.Email),
        new StringPropertyFilter<PersonReadModel>(p => p.PhoneNumber),
        
        // Numeric filters (example with age - add your numeric properties)
        new NumericPropertyFilter<PersonReadModel, int>(p => p.CurrentRole)
    ];

    public static IQueryable<PersonReadModel> ApplyFilters(this IQueryable<PersonReadModel> query, IEnumerable<FilterCriteria> filterCriteria)
    {
        foreach (FilterCriteria criteria in filterCriteria)
        {
            IPropertyFilter<PersonReadModel>? filter = Filters.FirstOrDefault(f => f.MatchesProperty(criteria.PropertyName));
            if (filter == null)
            {
                continue;
            }

            query = filter.ApplyFilter(query, criteria.Operation, criteria.Value);
        }

        return query;
    }
}
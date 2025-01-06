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
                
                query = query.FilterPerson(filters);
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

public static partial class FilterService
{
    public static IQueryable<PersonReadModel> FilterPerson(this IQueryable<PersonReadModel> query, IEnumerable<FilterCriteria> filterCriterias)
    {
        foreach (FilterCriteria filterCriteria in filterCriterias)
        {
            if (string.Equals(filterCriteria.PropertyName, nameof(PersonReadModel.Nickname), StringComparison.CurrentCultureIgnoreCase))
            {
                query = filterCriteria.Operation switch
                {
                    FilterCriteria.FilterOperation.Eq => query.Where(x=>x.Nickname == filterCriteria.Value),
                    FilterCriteria.FilterOperation.Neq => query.Where(x=>x.Nickname != filterCriteria.Value),
                    FilterCriteria.FilterOperation.In => query.Where(x=>x.Nickname.Contains(filterCriteria.Value)),
                    FilterCriteria.FilterOperation.Nin => query.Where(x=>!x.Nickname.Contains(filterCriteria.Value)),
                    _ => query
                };
            }
            if (string.Equals(filterCriteria.PropertyName, nameof(PersonReadModel.Email), StringComparison.CurrentCultureIgnoreCase))
            {
                query = filterCriteria.Operation switch
                {
                    FilterCriteria.FilterOperation.Eq => query.Where(x=>x.Email == filterCriteria.Value),
                    FilterCriteria.FilterOperation.Neq => query.Where(x=>x.Email != filterCriteria.Value),
                    FilterCriteria.FilterOperation.In => query.Where(x=>x.Email.Contains(filterCriteria.Value)),
                    FilterCriteria.FilterOperation.Nin => query.Where(x=>!x.Email.Contains(filterCriteria.Value)),
                    _ => query
                };
            }
            if (string.Equals(filterCriteria.PropertyName, nameof(PersonReadModel.PhoneNumber), StringComparison.CurrentCultureIgnoreCase))
            {
                query = filterCriteria.Operation switch
                {
                    FilterCriteria.FilterOperation.Eq => query.Where(x=>x.PhoneNumber == filterCriteria.Value),
                    FilterCriteria.FilterOperation.Neq => query.Where(x=>x.PhoneNumber != filterCriteria.Value),
                    FilterCriteria.FilterOperation.In => query.Where(x=>x.PhoneNumber.Contains(filterCriteria.Value)),
                    FilterCriteria.FilterOperation.Nin => query.Where(x=>!x.PhoneNumber.Contains(filterCriteria.Value)),
                    _ => query
                };
            }
        }

        return query;
    }
}
﻿using System.Linq.Expressions;
using KittySaver.Api.Features.Persons.SharedContracts;
using KittySaver.Api.Hateoas;
using KittySaver.Api.Infrastructure.Endpoints;
using KittySaver.Api.Persistence.ReadRelated;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.CollectionsQueriesFiltering;
using KittySaver.ReadModels.PersonAggregate;
using KittySaver.Shared.Responses;
using MediatR;
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
                
                IPropertyFilter<PersonReadModel>[] propertyFilters = GetPropertyFilters();
                
                query = query.ApplyFilters(filters, propertyFilters);
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
                    EndpointNames.Persons.GetAll.EndpointName,
                    request.Offset,
                    request.Limit,
                    totalRecords)
            };
            
            return response;
        }

        private static IPropertyFilter<PersonReadModel>[] GetPropertyFilters() =>
        [
            new StringPropertyFilter<PersonReadModel>(p => p.Nickname),
            new StringPropertyFilter<PersonReadModel>(p => p.Email),
            new StringPropertyFilter<PersonReadModel>(p => p.PhoneNumber),
                
            new NumericPropertyFilter<PersonReadModel, int>(p => p.CurrentRole)
        ];

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
            .WithName(EndpointNames.Persons.GetAll.EndpointName)
            .WithTags(EndpointNames.Persons.Group);;
    }
}
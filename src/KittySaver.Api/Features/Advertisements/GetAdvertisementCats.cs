using System.Linq.Expressions;
using KittySaver.Api.Features.Cats.SharedContracts;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.CollectionsQueriesFiltering;
using KittySaver.Api.Shared.Endpoints;
using KittySaver.Api.Shared.Pagination;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Api.Shared.Persistence.ReadModels;
using KittySaver.Shared.Pagination;
using KittySaver.Shared.Responses;
using KittySaver.Shared.TypedIds;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Advertisements;

public sealed class GetAdvertisementCats : IEndpoint
{
    public sealed record GetAdvertisementCatsQuery(
        PersonId PersonId,
        AdvertisementId AdvertisementId,
        int? Offset,
        int? Limit,
        string? SearchTerm,
        string? SortColumn,
        string? SortOrder)
        : IQuery<IPagedList<CatResponse>>, IPagedQuery, IAdvertisementRequest;

    internal sealed class GetAdvertisementCatsQueryHandler(
        ApplicationReadDbContext db,
        IPaginationLinksService paginationLinksService)
        : IRequestHandler<GetAdvertisementCatsQuery, IPagedList<CatResponse>>
    {
        public async Task<IPagedList<CatResponse>> Handle(GetAdvertisementCatsQuery request, CancellationToken cancellationToken)
        {
            IQueryable<CatReadModel> query = db.Cats
                .Where(x=>x.PersonId == request.PersonId && x.AdvertisementId == request.AdvertisementId);
            
            int totalRecords = await query.CountAsync(cancellationToken);
            
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                IEnumerable<FilterCriteria> filters = request.SearchTerm
                    .Split(',')
                    .Select(FilterCriteria.Parse);
                
                IPropertyFilter<CatReadModel>[] propertyFilters = GetPropertyFilters();
                
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
            
            List<CatResponse> advertisements =
                await query
                    .ProjectToDto()
                    .ToListAsync(cancellationToken);

            PagedList<CatResponse> response = new()
            {
                Items = advertisements,
                Total = totalRecords,
                Links = paginationLinksService.GeneratePaginationLinks(
                    EndpointNames.GetAdvertisementCats.EndpointName,
                    request.Offset,
                    request.Limit,
                    totalRecords,
                    request.PersonId)
            };
            
            return response;
        }
        
        private static IPropertyFilter<CatReadModel>[] GetPropertyFilters() =>
        [
            new StringPropertyFilter<CatReadModel>(p => p.Name),
            new NumericPropertyFilter<CatReadModel, double>(p => p.PriorityScore)
        ];
        
        private static Expression<Func<CatReadModel, object>> GetSortProperty(GetAdvertisementCatsQuery request)
            => request.SortColumn?.ToLower() switch
            {
                "name" => cat => cat.Name,
                "priorityscore" => cat => cat.PriorityScore,
                _ => cat => cat.Name
            };
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("persons/{personId:guid}/advertisements/{id:guid}/cats", async (
            Guid personId,
            Guid id,
            int? offset,
            int? limit,
            string? searchTerm,
            string? sortColumn,
            string? sortOrder,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            GetAdvertisementCatsQuery query = new(
                new PersonId(personId),
                new AdvertisementId(id),
                Offset: offset,
                Limit: limit,
                SearchTerm: searchTerm,
                SortColumn: sortColumn,
                SortOrder: sortOrder);
            IPagedList<CatResponse> cats = await sender.Send(query, cancellationToken);
            return Results.Ok(cats);
        }).AllowAnonymous()
        .WithName(EndpointNames.GetAdvertisementCats.EndpointName)
        .WithTags(EndpointNames.GroupNames.AdvertisementGroup);
    }
}
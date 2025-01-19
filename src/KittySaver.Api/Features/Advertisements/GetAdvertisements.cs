using System.Linq.Expressions;
using KittySaver.Api.Features.Advertisements.SharedContracts;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.CollectionsQueriesFiltering;
using KittySaver.Api.Shared.Endpoints;
using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Api.Shared.Pagination;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Api.Shared.Persistence.ReadModels;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Advertisements;

public sealed class GetAdvertisements : IEndpoint
{
    public sealed record GetAdvertisementsQuery(
        Guid PersonId,
        int? Offset,
        int? Limit,
        string? SearchTerm,
        string? SortColumn,
        string? SortOrder)
        : IQuery<IPagedList<AdvertisementResponse>>, IAuthorizedRequest, IPagedQuery, IAdvertisementRequest;

    internal sealed class GetAdvertisementsQueryHandler(
        ApplicationReadDbContext db,
        IPaginationLinksService paginationLinksService)
        : IRequestHandler<GetAdvertisementsQuery, IPagedList<AdvertisementResponse>>
    {
        public async Task<IPagedList<AdvertisementResponse>> Handle(GetAdvertisementsQuery request, CancellationToken cancellationToken)
        {
            IQueryable<AdvertisementReadModel> query = db.Advertisements
                .Where(x=>x.PersonId == request.PersonId);
            
            int totalRecords = await query.CountAsync(cancellationToken);
            
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                IEnumerable<FilterCriteria> filters = request.SearchTerm
                    .Split(',')
                    .Select(FilterCriteria.Parse);
                
                IPropertyFilter<AdvertisementReadModel>[] propertyFilters = GetPropertyFilters();
                
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
            
            List<AdvertisementResponse> advertisements =
                await query
                    .ProjectToDto()
                    .ToListAsync(cancellationToken);

            PagedList<AdvertisementResponse> response = new()
            {
                Items = advertisements,
                Total = totalRecords,
                Links = paginationLinksService.GeneratePaginationLinks(
                    EndpointNames.GetAdvertisements.EndpointName,
                    request.Offset,
                    request.Limit,
                    totalRecords,
                    request.PersonId)
            };
            
            return response;
        }
        
        private static IPropertyFilter<AdvertisementReadModel>[] GetPropertyFilters() =>
        [
            new StringPropertyFilter<AdvertisementReadModel>(p => p.PickupAddressCountry),
            new StringPropertyFilter<AdvertisementReadModel>(p => p.PickupAddressState),
            new StringPropertyFilter<AdvertisementReadModel>(p => p.PickupAddressZipCode),
            new StringPropertyFilter<AdvertisementReadModel>(p => p.PickupAddressCity),
            new StringPropertyFilter<AdvertisementReadModel>(p => p.PickupAddressStreet),
            new StringPropertyFilter<AdvertisementReadModel>(p => p.PickupAddressBuildingNumber),
                
            new NumericPropertyFilter<AdvertisementReadModel, double>(p => p.PriorityScore)
        ];
        
        private static Expression<Func<AdvertisementReadModel, object>> GetSortProperty(GetAdvertisementsQuery request)
            => request.SortColumn?.ToLower() switch
            {
                "pickupaddresscountry" => advertisement => advertisement.PickupAddressCountry,
                "pickupaddresszipcode" => advertisement => advertisement.PickupAddressZipCode,
                "pickupaddresscity" => advertisement => advertisement.PickupAddressCity,
                "pickupaddressstreet" => advertisement => advertisement.PickupAddressStreet!,
                "pickupaddressbuildingnumber" => advertisement => advertisement.PickupAddressBuildingNumber!,
                "priorityscore" => advertisement => advertisement.PriorityScore,
                _ => advertisement => advertisement.PriorityScore
            };
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("persons/{personId:guid}/advertisements", async (
            Guid personId,
            int? offset,
            int? limit,
            string? searchTerm,
            string? sortColumn,
            string? sortOrder,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            GetAdvertisementsQuery query = new(personId, offset, limit, searchTerm, sortColumn, sortOrder);
            IPagedList<AdvertisementResponse> advertisements = await sender.Send(query, cancellationToken);
            return Results.Ok(advertisements);
        }).RequireAuthorization()
        .WithName(EndpointNames.GetAdvertisements.EndpointName)
        .WithTags(EndpointNames.GroupNames.AdvertisementGroup);
    }
}
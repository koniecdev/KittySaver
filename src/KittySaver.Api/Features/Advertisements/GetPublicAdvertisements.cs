using System.Linq.Expressions;
using KittySaver.Api.Features.Advertisements.SharedContracts;
using KittySaver.Api.Hateoas;
using KittySaver.Api.Infrastructure.Endpoints;
using KittySaver.Api.Persistence.ReadRelated;
using KittySaver.Api.Persistence.ReadRelated.ReadModels;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.CollectionsQueriesFiltering;
using KittySaver.Shared.Common.Enums;
using KittySaver.Shared.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Advertisements;

public sealed class GetPublicAdvertisements : IEndpoint
{
    public sealed class GetPublicAdvertisementsQuery(
        int? offset,
        int? limit,
        string? searchTerm,
        string? sortColumn,
        string? sortOrder)
        : IQuery<IPagedList<AdvertisementResponse>>, IPagedQuery
    {
        public int? Offset { get; } = offset;
        public int? Limit { get; } = limit;
        public string? SearchTerm { get; } = searchTerm;
        public string? SortColumn { get; } = sortColumn;
        public string? SortOrder { get; } = sortOrder;
    }

    internal sealed class GetPublicAdvertisementsQueryHandler(
        ApplicationReadDbContext db,
        IPaginationLinksService paginationLinksService)
        : IRequestHandler<GetPublicAdvertisementsQuery, IPagedList<AdvertisementResponse>>
    {
        public async Task<IPagedList<AdvertisementResponse>> Handle(GetPublicAdvertisementsQuery request, CancellationToken cancellationToken)
        {
            IQueryable<AdvertisementReadModel> query = db.Advertisements
                .Where(x=>x.Status == AdvertisementStatus.Active);
            
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
                    EndpointNames.GetPublicAdvertisements.EndpointName,
                    request.Offset,
                    request.Limit,
                    totalRecords)
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
                
            new NumericPropertyFilter<AdvertisementReadModel, double>(p => p.PriorityScore),
            
            new PersonIdPropertyFilter<AdvertisementReadModel>(p => p.PersonId)
        ];
        
        private static Expression<Func<AdvertisementReadModel, object>> GetSortProperty(GetPublicAdvertisementsQuery request)
            => request.SortColumn?.ToLower() switch
            {
                "pickupaddresscountry" => advertisement => advertisement.PickupAddressCountry,
                "pickupaddresszipcode" => advertisement => advertisement.PickupAddressZipCode,
                "pickupaddresscity" => advertisement => advertisement.PickupAddressCity,
                "pickupaddressstreet" => advertisement => advertisement.PickupAddressStreet!,
                "pickupaddressbuildingnumber" => advertisement => advertisement.PickupAddressBuildingNumber!,
                "priorityscore" => advertisement => advertisement.PriorityScore,
                "personid" => advertisement => advertisement.PersonId,
                _ => advertisement => advertisement.PriorityScore
            };
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("advertisements", async (
            int? offset,
            int? limit,
            string? searchTerm,
            string? sortColumn,
            string? sortOrder,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            GetPublicAdvertisementsQuery query = new(offset, limit, searchTerm, sortColumn, sortOrder);
            IPagedList<AdvertisementResponse> advertisements = await sender.Send(query, cancellationToken);
            return Results.Ok(advertisements);
        }).AllowAnonymous()
        .WithName(EndpointNames.GetPublicAdvertisements.EndpointName)
        .WithTags(EndpointNames.GroupNames.AdvertisementGroup);
    }
}
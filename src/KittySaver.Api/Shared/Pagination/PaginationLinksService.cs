using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Hateoas;

namespace KittySaver.Api.Shared.Pagination;
public interface IPaginationLinksService
{
    List<Link> GeneratePaginationLinks(
        string endpointName,
        int? currentOffset,
        int? currentLimit,
        int totalRecords,
        Guid? personId = null);
}
public class PaginationLinksService(ILinkService linkService) : IPaginationLinksService
{
    public List<Link> GeneratePaginationLinks(
        string endpointName,
        int? currentOffset,
        int? currentLimit,
        int totalRecords,
        Guid? personId = null)
    {
        List<Link> links =
        [
            linkService.Generate(
                endpointName,
                new { offset = currentOffset, limit = currentLimit, personId },
                rel: "self")
        ];
        
        if (currentOffset.HasValue && currentLimit.HasValue)
        {
            int offset = currentOffset.Value;
            int limit = currentLimit.Value;

            if (offset > 0)
            {
                links.Add(linkService.Generate(
                    endpointName,
                    (offset: 0, limit),
                    rel: "first"));

                int previousOffset = Math.Max(0, offset - limit);
                links.Add(linkService.Generate(
                    endpointName,
                    (offset: previousOffset, limit),
                    rel: "previous"));
            }

            if (offset + limit < totalRecords)
            {
                links.Add(linkService.Generate(
                    endpointName,
                    (offset: offset + limit, limit),
                    rel: "next"));
            }

            int lastPageOffset = (totalRecords - 1) / limit * limit;
            if (offset < lastPageOffset)
            {
                links.Add(linkService.Generate(
                    endpointName,
                    (offset: lastPageOffset, limit),
                    rel: "last"));
            }
        }
        else if (currentOffset is > 0)
        {
            links.Add(linkService.Generate(
                endpointName,
                new { offset = 0 },
                rel: "first"));
        }

        links.AddRange(linkService.GeneratePaginationLinks(endpointName, currentOffset, currentLimit, personId));
        
        return links;
    }
}
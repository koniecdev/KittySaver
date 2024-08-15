namespace KittySaver.Api.Shared.Infrastructure.Endpoints;

public interface IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app);
}
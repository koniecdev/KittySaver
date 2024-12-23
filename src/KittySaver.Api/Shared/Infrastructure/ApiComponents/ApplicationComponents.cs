namespace KittySaver.Api.Shared.Infrastructure.ApiComponents;

public interface IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app);
}
public interface IAsyncValidator;


public interface IPersonAggregateAuthorizationRequiredRequest;

public interface IPersonAggregatePersonIdBase
{
    public Guid PersonId { get; }
}
public interface IIPersonAggregateUserIdentityIdBase
{
    public Guid UserIdentityId { get; }
}
public interface IIPersonAggregateIdOrUserIdentityIdBase
{
    public Guid IdOrUserIdentityId { get; }
}
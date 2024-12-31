using KittySaver.Api.Features.Advertisements.SharedContracts;

namespace KittySaver.Api.Shared.Abstractions;

public interface IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app);
}

public interface IHateoasResponse
{
    public Guid Id { get; }
    public ICollection<Link> Links { get; set; }
}
public interface IHateoasPersonResponse : IHateoasResponse;

public interface IHateoasCatResponse : IHateoasResponse
{
    public Guid PersonId { get; }
    public Guid? AdvertisementId { get; }
}

public interface IHateoasAdvertisementResponse : IHateoasResponse
{
    public Guid PersonId { get; }
    public AdvertisementResponse.AdvertisementStatus Status { get; }
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
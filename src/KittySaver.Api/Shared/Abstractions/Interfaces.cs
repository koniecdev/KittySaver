using KittySaver.Api.Features.Advertisements.SharedContracts;
using MediatR;

namespace KittySaver.Api.Shared.Abstractions;

public interface IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app);
}

public interface IHateoasResponse
{
    public ICollection<Link> Links { get; set; }
}

public interface IHateoasPersonResponse : IHateoasResponse
{
    public Guid Id { get; }
}

public interface IHateoasCatResponse : IHateoasResponse
{
    public Guid Id { get; }
    public Guid PersonId { get; }
    public Guid? AdvertisementId { get; }
}

public interface IHateoasAdvertisementResponse : IHateoasResponse
{
    public Guid Id { get; }
    public Guid PersonId { get; }
    public AdvertisementResponse.AdvertisementStatus Status { get; }
}

public interface IAsyncValidator;


public interface ICommandBase;
public interface IQueryBase;

public interface ICommand : IRequest, ICommandBase;
public interface ICommand<out TResponse> : IRequest<TResponse>, ICommandBase;

public interface IQuery<out TResponse> : IRequest<TResponse>, IQueryBase;

public interface ICatRequest
{
    public Guid PersonId { get; }
}

public interface IAdvertisementRequest
{
    public Guid PersonId { get; }
}

public interface ICreatePersonRequest
{
    public Guid UserIdentityId { get; }
}
public interface IPersonRequest
{
    public Guid IdOrUserIdentityId { get; }
}

public interface IPagedQuery;

public interface IAuthenticationBasedRequest;
public interface IAuthorizedRequest : IAuthenticationBasedRequest;
public interface IAdminOnlyRequest : IAuthenticationBasedRequest;
public interface IJobOrAdminOnlyRequest : IAuthenticationBasedRequest;
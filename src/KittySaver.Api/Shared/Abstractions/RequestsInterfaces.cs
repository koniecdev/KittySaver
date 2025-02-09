using MediatR;

namespace KittySaver.Api.Shared.Abstractions;

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
}
public interface IPersonRequest
{
    public Guid Id { get; }
}

public interface IPagedQuery;

public interface IAuthenticationBasedRequest;
public interface IAuthorizedRequest : IAuthenticationBasedRequest;
public interface IAdminOnlyRequest : IAuthenticationBasedRequest;
public interface IJobOrAdminOnlyRequest : IAuthenticationBasedRequest;
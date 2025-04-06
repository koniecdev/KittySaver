using KittySaver.Shared.TypedIds;
using MediatR;

namespace KittySaver.Api.Shared.Abstractions;

public interface ICommandBase;
public interface IQueryBase;

public interface ICommand : IRequest, ICommandBase;
public interface ICommand<out TResponse> : IRequest<TResponse>, ICommandBase;

public interface IQuery<out TResponse> : IRequest<TResponse>, IQueryBase;

public interface ICatRequest
{
    public PersonId PersonId { get; }
}

public interface IAdvertisementRequest
{
    public PersonId PersonId { get; }
}

public interface ICreatePersonRequest
{
}
public interface IPersonRequest
{
    public PersonId Id { get; }
}

public interface IPagedQuery;

public interface IAuthenticationBasedRequest;
public interface IAuthorizedRequest : IAuthenticationBasedRequest;
public interface IAdminOnlyRequest : IAuthenticationBasedRequest;
public interface IJobOrAdminOnlyRequest : IAuthenticationBasedRequest;
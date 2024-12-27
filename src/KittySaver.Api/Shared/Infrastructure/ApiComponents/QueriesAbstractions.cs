using KittySaver.Api.Shared.Abstractions;
using MediatR;

namespace KittySaver.Api.Shared.Infrastructure.ApiComponents;

public interface IQueryBase;

public interface IQuery<out TResponse> : IRequest<TResponse>, IQueryBase;
public interface IPersonAggregateAuthorizationRequiredBase : IPersonAggregateAuthorizationRequiredRequest;
public interface IPersonAggregateAuthorizationRequiredQuery<out TResponse> : IQuery<TResponse>, IPersonAggregateAuthorizationRequiredBase;

public interface IPersonQuery<out TResponse> : IPersonAggregateAuthorizationRequiredQuery<TResponse>, IIPersonAggregateIdOrUserIdentityIdBase;
public interface ICatQuery<out TResponse> : IPersonAggregateAuthorizationRequiredQuery<TResponse>, IPersonAggregatePersonIdBase;

public interface IAdvertisementQuery<out TResponse> : IQuery<TResponse>;
public interface IAdminOnlyQuery<out TResponse> : IQuery<TResponse>, IPersonAggregateAuthorizationRequiredRequest;

using KittySaver.Api.Shared.Abstractions;
using MediatR;

namespace KittySaver.Api.Shared.Infrastructure.ApiComponents;

public interface ICommandBase;

public interface ICommand : IRequest, ICommandBase;

public interface ICommand<out TResponse> : IRequest<TResponse>, ICommandBase;

public interface IPersonAggregateCommandBase : IPersonAggregateAuthorizationRequiredRequest;
public interface IPersonAggregateCommand : ICommand, IPersonAggregateCommandBase;
public interface IPersonAggregateCommand<out TResponse> : ICommand<TResponse>, IPersonAggregateCommandBase;

public interface IPersonCommand : IPersonAggregateCommand, IIPersonAggregateIdOrUserIdentityIdBase;
public interface IPersonCommand<out TResponse> : IPersonAggregateCommand<TResponse>, IIPersonAggregateUserIdentityIdBase;

public interface ICatCommand : IPersonAggregateCommand, IPersonAggregatePersonIdBase;
public interface ICatCommand<out TResponse> : IPersonAggregateCommand<TResponse>, IPersonAggregatePersonIdBase;

public interface IAdvertisementCommand : IPersonAggregateCommand, IPersonAggregatePersonIdBase;
public interface IAdvertisementCommand<out TResponse> : IPersonAggregateCommand<TResponse>, IPersonAggregatePersonIdBase;


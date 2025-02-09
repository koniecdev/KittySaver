using MediatR;

namespace KittySaver.Auth.Api.Shared.Abstractions;

public interface ICommandBase;

public interface ICommand : IRequest, ICommandBase;

public interface ICommand<out TResponse> : IRequest<TResponse>, ICommandBase;

public interface IQueryBase;

public interface IQuery<out TResponse> : IRequest<TResponse>, IQueryBase;
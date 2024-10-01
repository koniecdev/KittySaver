using MediatR;

namespace KittySaver.Auth.Api.Shared.Infrastructure.ApiComponents;

public interface ICommandBase;

public interface ICommand : IRequest, ICommandBase;

public interface ICommand<out TResponse> : IRequest<TResponse>, ICommandBase;
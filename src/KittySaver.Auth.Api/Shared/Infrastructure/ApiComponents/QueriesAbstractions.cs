using MediatR;

namespace KittySaver.Auth.Api.Shared.Infrastructure.ApiComponents;

public interface IQueryBase;

public interface IQuery<out TResponse> : IRequest<TResponse>, IQueryBase;
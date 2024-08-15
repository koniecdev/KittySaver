using MediatR;

namespace KittySaver.Api.Shared.Infrastructure.ApiComponents;

public interface IQueryBase{}

public interface IQuery<out TResponse> : IRequest<TResponse>, IQueryBase{}
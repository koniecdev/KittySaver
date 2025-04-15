namespace KittySaver.Domain.Common;

public interface IRepository<TAggregateRoot, in TAggregateRootId> 
    where TAggregateRootId : struct
    where TAggregateRoot : AggregateRoot<TAggregateRootId>
{
    Task<TAggregateRoot> GetByIdAsync(TAggregateRootId id, CancellationToken cancellationToken);
    void Insert(TAggregateRoot entity);
    void Remove(TAggregateRoot entity);
}
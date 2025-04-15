using KittySaver.Api.Persistence.WriteRelated;
using KittySaver.Domain.Common;
using KittySaver.Domain.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Persistence;

public abstract class GenericRepository<TAggregateRoot, TAggregateRootId>(ApplicationWriteDbContext db)
    : IRepository<TAggregateRoot, TAggregateRootId>
    where TAggregateRootId : struct
    where TAggregateRoot : AggregateRoot<TAggregateRootId>
{
    protected readonly ApplicationWriteDbContext DbContext = db;
    
    public virtual async Task<TAggregateRoot> GetByIdAsync(TAggregateRootId id, CancellationToken cancellationToken)
    {
        TAggregateRoot toReturn = await DbContext.Set<TAggregateRoot>()
                                      .FirstOrDefaultAsync(x=>x.Id.Equals(id), cancellationToken)
                                  ?? throw new NotFoundException<TAggregateRootId>(typeof(TAggregateRoot).Name, id);
        return toReturn;
    }

    public void Insert(TAggregateRoot aggregate)
    {
        DbContext.Set<TAggregateRoot>().Add(aggregate);
    }

    public void Remove(TAggregateRoot aggregate)
    {
        DbContext.Set<TAggregateRoot>().Remove(aggregate);
    }
}
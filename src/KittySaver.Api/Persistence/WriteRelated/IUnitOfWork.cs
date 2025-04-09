namespace KittySaver.Api.Persistence.WriteRelated;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
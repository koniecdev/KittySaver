namespace KittySaver.Domain.Common.Primitives;

public interface IAggregateRoot
{
    IReadOnlyCollection<DomainEvent> GetDomainEvents();
    void ClearDomainEvents();
}

public abstract class AggregateRoot<TId> : AuditableEntity<TId>, IAggregateRoot where TId : struct
{
    protected AggregateRoot(TId id) : base(id)
    {
    }
    private AggregateRoot()
    {
    }
    private readonly List<DomainEvent> _domainEvents = [];
    public IReadOnlyCollection<DomainEvent> GetDomainEvents() => _domainEvents.ToList();
    public void ClearDomainEvents() => _domainEvents.Clear();
    protected void RaiseDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
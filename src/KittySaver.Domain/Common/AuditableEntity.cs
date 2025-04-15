namespace KittySaver.Domain.Common;

public interface IAuditableEntity
{
    DateTimeOffset CreatedAt { get; set; }
}

public abstract class AuditableEntity<TId>(TId id) : IAuditableEntity where TId : struct
{
    protected AuditableEntity() : this(default)
    {
    }
    public TId Id { get; } = id;
    public DateTimeOffset CreatedAt { get; set; }
}
namespace KittySaver.Domain.Common.Primitives;

public abstract class AuditableEntity
{
    public Guid Id { get; } = Guid.NewGuid();
    public string CreatedBy { get; set; } = "";
    public DateTimeOffset CreatedOn { get; set; }
    public string? LastModificationBy { get; set; }
    public DateTimeOffset? LastModificationOn { get; set; }
}
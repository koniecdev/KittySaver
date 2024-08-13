namespace KittySaver.Api.Shared.Domain.Entites;

public abstract class AuditableEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string CreatedBy { get; set; } = "";
    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.Now;
}
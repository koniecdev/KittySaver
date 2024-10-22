namespace KittySaver.Api.Shared.Domain.Entites.Common;

public abstract class AuditableEntity
{
    public Guid Id { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTimeOffset CreatedOn { get; set; }
    public string? LastModificationBy { get; set; }
    public DateTimeOffset? LastModificationOn { get; set; }
}
namespace KittySaver.Api.SharedKernel;

public abstract class AuditableEntity
{
    public required Guid Id { get; init; }
    public required string CreatedBy { get; init; }
    public required DateTimeOffset Created { get; init; }
}
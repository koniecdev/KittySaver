using KittySaver.Api.Shared.Domain.Entites.Common;

namespace KittySaver.Api.Shared.Domain.Entites;

public sealed class Cat : AuditableEntity
{

    public required Guid PersonId { get; init; }
    public Person Person { get; private set; } = null!;
}
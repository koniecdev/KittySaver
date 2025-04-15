using MediatR;

namespace KittySaver.Domain.Common;

public record DomainEvent : INotification
{
    protected DomainEvent()
    {
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
}
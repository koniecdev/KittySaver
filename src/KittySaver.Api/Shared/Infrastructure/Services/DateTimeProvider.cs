namespace KittySaver.Api.Shared.Infrastructure.Services;

public interface IDateTimeProvider
{
    public DateTimeOffset Now { get; }
}
public sealed class DefaultDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset Now => DateTimeOffset.Now;
}
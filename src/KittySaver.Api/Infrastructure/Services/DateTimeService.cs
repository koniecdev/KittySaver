﻿namespace KittySaver.Api.Infrastructure.Services;

public interface IDateTimeService
{
    public DateTimeOffset Now { get; }
}
public sealed class DefaultDateTimeService : IDateTimeService
{
    public DateTimeOffset Now => DateTimeOffset.Now;
}
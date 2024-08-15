namespace KittySaver.Api.Shared.Infrastructure.ApiComponents;

public class ApiFilter
{
    public enum FilteringMethod
    {
        In,
        Nin,
        Eq,
        Neq,
        Gt,
        Gte,
        Lt,
        Lte
    }
    public required string PropertyName { get; init; }
    public required FilteringMethod Method { get; init; }
    public required bool IsDescending { get; init; }
}

public class ApiSort
{
    public required string PropertyName { get; init; }
    public required bool IsDescending { get; init; }
}
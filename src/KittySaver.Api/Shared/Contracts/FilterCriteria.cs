namespace KittySaver.Api.Shared.Contracts;

public sealed class FilterCriteria
{
    private FilterCriteria(string propertyName, FilterOperation operation, string value)
    {
        PropertyName = propertyName;
        Operation = operation;
        Value = value;
    }

    public enum FilterOperation
    {
        Eq,
        Neq,
        In,
        Nin,
        Gt,
        Gte,
        Lt,
        Lte
    }
    public string PropertyName { get; }
    public FilterOperation Operation { get; }
    public string Value { get; }

    public static FilterCriteria Parse(string filter)
    {
        string[] parts = filter.Split('-', 3);
        
        return new FilterCriteria(parts[0].ToLowerInvariant(), ParseOperation(parts[1].ToLowerInvariant()), parts[2]);
    }
    
    private static FilterOperation ParseOperation(string operation) => operation.ToLower() switch
    {
        "eq" => FilterOperation.Eq,
        "neq" => FilterOperation.Neq,
        "in" => FilterOperation.In,
        "nin" => FilterOperation.Nin,
        "gt" => FilterOperation.Gt,
        "gte" => FilterOperation.Gte,
        "lt" => FilterOperation.Lt,
        "lte" => FilterOperation.Lte,
        _ => FilterOperation.Eq
    };
}
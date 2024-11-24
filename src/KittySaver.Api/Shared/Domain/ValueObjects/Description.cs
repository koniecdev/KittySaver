using KittySaver.Api.Shared.Domain.Common.Primitives;

namespace KittySaver.Api.Shared.Domain.ValueObjects;

public class Description : ValueObject
{
    public const int MaxLength = 1000;
    private Description(string value) => Value = value;
    public string Value { get; }
    public override string ToString() => Value;
    public static implicit operator string(Description description) => description.Value;

    public static Description Create(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            description = "";
        }
        switch (description.Length)
        {
            case < 2:
                return new Description(description);
            case > MaxLength:
                throw new ArgumentOutOfRangeException(nameof(description), description,
                    $"Maximum allowed length is: {MaxLength}");
        }

        string firstChar = description[0].ToString().ToUpper();
        string rest = description[1..];
        Description descriptionInstance = new Description($"{firstChar}{rest}");
        return descriptionInstance;
    }
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
using KittySaver.Domain.Common.Primitives;

namespace KittySaver.Domain.Persons;

public class FirstName : ValueObject
{
    public const int MaxLength = 100;
    private FirstName(string value) => Value = value;
    public string Value { get; }
    public override string ToString() => Value;
    public static implicit operator string(FirstName firstName) => firstName.Value;

    public static FirstName Create(string firstName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName, nameof(firstName));
        if (firstName.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(nameof(firstName), firstName,
                $"Maximum allowed length is: {MaxLength}");
        }

        string firstChar = firstName[0].ToString().ToUpper();
        string rest = firstName[1..];
        var firstNameInstance = new FirstName($"{firstChar}{rest}");
        return firstNameInstance;
    }
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
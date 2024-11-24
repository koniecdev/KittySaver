using KittySaver.Domain.Common.Primitives;

namespace KittySaver.Domain.Persons;

public class LastName : ValueObject
{
    public const int MaxLength = 100;
    private LastName(string value) => Value = value;
    public string Value { get; }
    public override string ToString() => Value;
    public static implicit operator string(LastName lastName) => lastName.Value;

    public static LastName Create(string lastName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName, nameof(lastName));
        if (lastName.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(nameof(lastName), lastName,
                $"Maximum allowed length is: {MaxLength}");
        }

        var lastNameInstance = new LastName(lastName);
        return lastNameInstance;
    }
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
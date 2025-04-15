using KittySaver.Domain.Common;

namespace KittySaver.Domain.Persons.ValueObjects;

public class Nickname : ValueObject
{
    public const int MaxLength = 100;
    private Nickname(string value) => Value = value;
    public string Value { get; }
    public override string ToString() => Value;
    public static implicit operator string(Nickname nickname) => nickname.Value;

    public static Nickname Create(string firstName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName, nameof(firstName));
        if (firstName.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(nameof(firstName), firstName,
                $"Maximum allowed length is: {MaxLength}");
        }

        string firstChar = firstName[0].ToString().ToUpper();
        string rest = firstName[1..];
        var firstNameInstance = new Nickname($"{firstChar}{rest}");
        return firstNameInstance;
    }
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
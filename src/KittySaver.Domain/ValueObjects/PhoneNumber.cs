using KittySaver.Domain.Common.Primitives;

namespace KittySaver.Domain.ValueObjects;

public class PhoneNumber : ValueObject
{
    public const int MaxLength = 31;
    private PhoneNumber(string value) => Value = value;
    public string Value { get; }

    public override string ToString() => Value;
    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;

    public static PhoneNumber Create(string phoneNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber, nameof(phoneNumber));
        if (phoneNumber.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(nameof(phoneNumber), phoneNumber,
                $"Maximum allowed length is: {MaxLength}");
        }
        
        PhoneNumber phoneNumberInstance = new(phoneNumber);
        return phoneNumberInstance;
    }
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
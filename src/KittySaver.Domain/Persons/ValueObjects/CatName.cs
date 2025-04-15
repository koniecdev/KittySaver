using KittySaver.Domain.Common;

namespace KittySaver.Domain.Persons.ValueObjects;

public class CatName : ValueObject
{
    public const int MaxLength = 30;
    private CatName(string value) => Value = value;
    public string Value { get; }
    public override string ToString() => Value;
    public static implicit operator string(CatName catName) => catName.Value;

    public static CatName Create(string catName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(catName, nameof(catName));
        if (catName.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(nameof(catName), catName,
                $"Maximum allowed length is: {MaxLength}");
        }

        string firstChar = catName[0].ToString().ToUpper();
        string rest = catName[1..];
        var catNameInstance = new CatName($"{firstChar}{rest}");
        return catNameInstance;
    }
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
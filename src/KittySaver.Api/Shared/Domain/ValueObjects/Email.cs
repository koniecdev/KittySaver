using System.Text.RegularExpressions;
using KittySaver.Api.Shared.Domain.Common.Primitives;

namespace KittySaver.Api.Shared.Domain.Persons;

public partial class Email : ValueObject
{
    public const int MaxLength = 256;
    public const string RegexPattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
    private static readonly Regex EmailRegex = MyRegex();
    private Email(string value) => Value = value;

    public string Value { get; }

    public override string ToString() => Value;
    public static implicit operator string(Email email) => email.Value;

    public static Email Create(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email, nameof(email));
        if (email.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(nameof(email), email,
                $"Maximum allowed length is: {MaxLength}");
        }
        
        if (!EmailRegex.IsMatch(email))
        {
            throw new ArgumentException("Invalid email format", nameof(email));
        }
        
        var emailInstance = new Email(email);
        return emailInstance;
    }
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    [GeneratedRegex(RegexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}
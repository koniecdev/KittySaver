using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using KittySaver.Api.Shared.Domain.Common.Interfaces;
using KittySaver.Api.Shared.Domain.ValueObjects.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.Api.Shared.Domain.ValueObjects;

public partial class ContactInfo : ValueObject, IContact
{
    private readonly string _email = null!;
    private readonly string _phoneNumber = null!;

    public required string Email
    {
        get => _email;
        init
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(Email));
            if (value.Length > IContact.Constraints.EmailMaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(Email), value,
                    $"Maximum allowed length is: {IContact.Constraints.EmailMaxLength}");
            }
            if (!EmailRegex().IsMatch(value))
            {
                throw new FormatException("Provided email is not in correct format.");
            }
            _email = value;
        }
    }

    public required string PhoneNumber
    {
        get => _phoneNumber;
        init
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(PhoneNumber));
            if (value.Length > IContact.Constraints.PhoneNumberMaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(PhoneNumber), value,
                    $"Maximum allowed length is: {IContact.Constraints.PhoneNumberMaxLength}");
            }
            _phoneNumber = value;
        }
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Email;
        yield return PhoneNumber;
    }
    
    [GeneratedRegex(IContact.Constraints.EmailPattern)]
    private static partial Regex EmailRegex();
}

internal sealed class ContactInfoConfiguration : IEntityTypeConfiguration<ContactInfo>
{
    public void Configure(EntityTypeBuilder<ContactInfo> builder)
    {
        builder.HasNoKey();
        builder
            .Property(x => x.Email)
            .HasMaxLength(IContact.Constraints.EmailMaxLength)
            .IsRequired();
        builder
            .Property(x => x.PhoneNumber)
            .HasMaxLength(IContact.Constraints.PhoneNumberMaxLength);
    }
}
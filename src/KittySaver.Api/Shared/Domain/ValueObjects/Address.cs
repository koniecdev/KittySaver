using KittySaver.Api.Shared.Domain.ValueObjects.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.Api.Shared.Domain.ValueObjects;

public sealed class Address : ValueObject
{
    private readonly string? _state;
    private readonly string _country = null!;
    private readonly string _zipCode = null!;
    private readonly string _city = null!;
    private readonly string _street = null!;
    private readonly string _buildingNumber = null!;

    public required string? State
    {
        get => _state;
        init
        {
            if (value is null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                _state = null;
                return;
            }
            
            if (value.Length > Constraints.StateMaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(State), value,
                    $"Maximum allowed length is: {Constraints.StateMaxLength}");
            }
            _state = value;
        }
    }

    public required string Country
    {
        get => _country;
        init
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(Country));
            if (value.Length > Constraints.CountryMaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(Country), value,
                    $"Maximum allowed length is: {Constraints.CountryMaxLength}");
            }
            _country = value;
        }
    }

    public required string ZipCode
    {
        get => _zipCode;
        init
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(ZipCode));
            if (value.Length > Constraints.ZipCodeMaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(ZipCode), value,
                    $"Maximum allowed length is: {Constraints.ZipCodeMaxLength}");
            }
            _zipCode = value;
        }
    }

    public required string City
    {
        get => _city;
        init
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(City));
            if (value.Length > Constraints.CityMaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(City), value,
                    $"Maximum allowed length is: {Constraints.CityMaxLength}");
            }
            _city = value;
        }
    }

    public required string Street
    {
        get => _street;
        init
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(Street));
            if (value.Length > Constraints.StreetMaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(Street), value,
                    $"Maximum allowed length is: {Constraints.StreetMaxLength}");
            }
            _street = value;
        }
    }

    public required string BuildingNumber
    {
        get => _buildingNumber;
        init
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(BuildingNumber));
            if (value.Length > Constraints.BuildingNumberMaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(BuildingNumber), value,
                    $"Maximum allowed length is: {Constraints.BuildingNumberMaxLength}");
            }
            _buildingNumber = value;
        }
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Country;
        yield return State ?? "";
        yield return ZipCode;
        yield return City;
        yield return Street;
        yield return BuildingNumber;
    }
    
    public static class Constraints
    {
        public const int StateMaxLength = 100;
        public const int CountryMaxLength = 60;
        public const int ZipCodeMaxLength = 10;
        public const int CityMaxLength = 100;
        public const int StreetMaxLength = 100;
        public const int BuildingNumberMaxLength = 20;
    }
}

internal sealed class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.HasNoKey();
        builder
            .Property(x => x.Country)
            .HasMaxLength(Address.Constraints.CountryMaxLength)
            .IsRequired();
        builder
            .Property(x => x.State)
            .HasMaxLength(Address.Constraints.StateMaxLength);
        builder
            .Property(x => x.ZipCode)
            .HasMaxLength(Address.Constraints.ZipCodeMaxLength)
            .IsRequired();
        builder
            .Property(x => x.City)
            .HasMaxLength(Address.Constraints.CityMaxLength)
            .IsRequired();
        builder
            .Property(x => x.Street)
            .HasMaxLength(Address.Constraints.StreetMaxLength)
            .IsRequired();
        builder
            .Property(x => x.BuildingNumber)
            .HasMaxLength(Address.Constraints.BuildingNumberMaxLength)
            .IsRequired();
    }
}
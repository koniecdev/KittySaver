using KittySaver.Domain.Common.Primitives;

// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength - String Length properties limits are always set in entity that contains value object. 

namespace KittySaver.Domain.ValueObjects;

public sealed class Address : ValueObject
{
    private readonly string? _state;
    private readonly string _country = null!;
    private readonly string _zipCode = null!;
    private readonly string _city = null!;
    private readonly string _street = null!;
    private readonly string _buildingNumber = null!;
    
    public const int StateMaxLength = 100;
    public const int CountryMaxLength = 60;
    public const int ZipCodeMaxLength = 10;
    public const int CityMaxLength = 100;
    public const int StreetMaxLength = 100;
    public const int BuildingNumberMaxLength = 20;

    public string? State
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
            
            if (value.Length > StateMaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(State), value,
                    $"Maximum allowed length is: {StateMaxLength}");
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
            if (value.Length > CountryMaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(Country), value,
                    $"Maximum allowed length is: {CountryMaxLength}");
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
            if (value.Length > ZipCodeMaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(ZipCode), value,
                    $"Maximum allowed length is: {ZipCodeMaxLength}");
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
            if (value.Length > CityMaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(City), value,
                    $"Maximum allowed length is: {CityMaxLength}");
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
            if (value.Length > StreetMaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(Street), value,
                    $"Maximum allowed length is: {StreetMaxLength}");
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
            if (value.Length > BuildingNumberMaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(BuildingNumber), value,
                    $"Maximum allowed length is: {BuildingNumberMaxLength}");
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
}

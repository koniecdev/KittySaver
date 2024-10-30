using KittySaver.Api.Shared.Domain.Common.Interfaces;
using KittySaver.Api.Shared.Domain.ValueObjects.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength - String Length properties limits are always set in entity that contains value object. 

namespace KittySaver.Api.Shared.Domain.ValueObjects;

public sealed class PickupAddress : ValueObject, IAddress
{
    private readonly string? _state;
    private readonly string _country = null!;
    private readonly string _zipCode = null!;
    private readonly string _city = null!;
    private readonly string? _street;
    private readonly string? _buildingNumber;

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
            
            if (value.Length > IAddress.Constraints.StateMaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(State), value,
                    $"Maximum allowed length is: {IAddress.Constraints.StateMaxLength}");
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
            if (value.Length > IAddress.Constraints.CountryMaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(Country), value,
                    $"Maximum allowed length is: {IAddress.Constraints.CountryMaxLength}");
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
            if (value.Length > IAddress.Constraints.ZipCodeMaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(ZipCode), value,
                    $"Maximum allowed length is: {IAddress.Constraints.ZipCodeMaxLength}");
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
            if (value.Length > IAddress.Constraints.CityMaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(City), value,
                    $"Maximum allowed length is: {IAddress.Constraints.CityMaxLength}");
            }
            _city = value;
        }
    }

    public string? Street
    {
        get => _street;
        init
        {
            if (value is null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                _street = null;
                return;
            }
            
            if (value.Length > IAddress.Constraints.StreetMaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(Street), value,
                    $"Maximum allowed length is: {IAddress.Constraints.StreetMaxLength}");
            }
            _street = value;
        }
    }

    public string? BuildingNumber
    {
        get => _buildingNumber;
        init
        {
            if (value is null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                _buildingNumber = null;
                return;
            }
            
            if (value.Length > IAddress.Constraints.BuildingNumberMaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(BuildingNumber), value,
                    $"Maximum allowed length is: {IAddress.Constraints.BuildingNumberMaxLength}");
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
        yield return Street ?? "";
        yield return BuildingNumber ?? "";
    }
}
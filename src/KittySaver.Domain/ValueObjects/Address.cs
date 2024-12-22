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

    private static readonly HashSet<string> ValidCountryCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "AF", "AL", "AX", "BL", "BQ", "CW", "GG", "IM", "JE", "ME", "MF", "RS", "SS", "SX", "DZ", "AS",
        "AD", "AO", "AI", "AQ", "AG", "AR", "AM", "AW", "AU", "AT", "AZ", "BS", "EH", "YE", "ZM", "ZW",
        "BH", "BD", "BB", "BY", "BE", "BZ", "BJ", "BM", "BT", "BO", "BA", "BW", "BV", "BR", "IO", "BN", 
        "BG", "BF", "BI", "KH", "CM", "CA", "CV", "KY", "CF", "TD", "CL", "CN", "CX", "CC", "CO", "KM", 
        "CG", "CD", "CK", "CR", "CI", "HR", "CU", "CY", "CZ", "DK", "DJ", "DM", "DO", "EC", "EG", "SV", 
        "GQ", "ER", "EE", "ET", "FK", "FO", "FJ", "FI", "FR", "GF", "PF", "TF", "GA", "GM", "GE", "DE", 
        "GH", "GI", "GR", "GL", "GD", "GP", "GU", "GT", "GN", "GW", "GY", "HT", "HM", "VA", "HN", "HK", 
        "HU", "IS", "IN", "ID", "IR", "IQ", "IE", "IL", "IT", "JM", "JP", "JO", "KZ", "KE", "KI", "KP", 
        "KR", "KW", "KG", "LA", "LV", "LB", "LS", "LR", "LY", "LI", "LT", "LU", "MO", "MK", "MG", "MW", 
        "MY", "MV", "ML", "MT", "MH", "MQ", "MR", "MU", "YT", "MX", "FM", "MD", "MC", "MN", "MS", "MA", 
        "MZ", "MM", "NA", "NR", "NP", "NL", "NC", "NZ", "NI", "NE", "NG", "NU", "NF", "MP", "NO", "OM", 
        "PK", "PW", "PS", "PA", "PG", "PY", "PE", "PH", "PN", "PL", "PT", "PR", "QA", "RE", "RO", "RU", 
        "RW", "SH", "KN", "LC", "PM", "VC", "WS", "SM", "ST", "SA", "SN", "SC", "SL", "SG", "SK", "SI", 
        "SB", "SO", "ZA", "GS", "ES", "LK", "SD", "SR", "SJ", "SZ", "SE", "CH", "SY", "TW", "TJ", "TZ", 
        "TH", "TL", "TG", "TK", "TO", "TT", "TN", "TR", "TM", "TC", "TV", "UG", "UA", "AE", "GB", "US", 
        "UM", "UY", "UZ", "VU", "VE", "VN", "VG", "VI", "WF"
    };

    private static bool IsValidCountryCode(string countryCode) => 
        ValidCountryCodes.Contains(countryCode);

    public static Address Create(string country, string? state, string zipCode, string city, string street, string buildingNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(country, nameof(country));
        
        country = country.Trim().ToUpperInvariant();
        
        if (!IsValidCountryCode(country))
        {
            throw new ArgumentException($"Invalid country code: {country}. Please use ISO 3166-1 alpha-2 code.", nameof(country));
        }

        return new Address(country, state, zipCode, city, street, buildingNumber);
    }

    private Address(string country, string? state, string zipCode, string city, string street, string buildingNumber)
    {
        Country = country;
        State = state;
        ZipCode = zipCode;
        City = city;
        Street = street;
        BuildingNumber = buildingNumber;
    }
    
    public string? State
    {
        get => _state;
        private init
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

    public string Country
    {
        get => _country;
        private init
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

    public string ZipCode
    {
        get => _zipCode;
        private init
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

    public string City
    {
        get => _city;
        private init
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

    public string Street
    {
        get => _street;
        private init
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

    public string BuildingNumber
    {
        get => _buildingNumber;
        private init
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

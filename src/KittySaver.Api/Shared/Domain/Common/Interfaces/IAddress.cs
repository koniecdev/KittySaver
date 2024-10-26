namespace KittySaver.Api.Shared.Domain.Common.Interfaces;

public interface IAddress
{
    public string? Country { get; }
    public string? State { get; }
    public string? ZipCode { get; }
    public string? City { get; }
    public string? Street { get; }
    public string? BuildingNumber { get; }
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
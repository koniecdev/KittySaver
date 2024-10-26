using KittySaver.Api.Shared.Domain.Common.Interfaces;
using KittySaver.Api.Shared.Domain.ValueObjects.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.Api.Shared.Domain.ValueObjects;


public sealed class PickupAddress(
    string country,
    string? state,
    string zipCode,
    string city,
    string? street,
    string? buildingNumber)
    : ValueObject, IAddress
{
    public string Country { get; private init; } = country;
    public string? State { get; private init; } = state;
    public string ZipCode { get; private init; } = zipCode;
    public string City { get; private init; } = city;
    public string? Street { get; private init; } = street;
    public string? BuildingNumber { get; private init; } = buildingNumber;

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

internal sealed class PickupAddressConfiguration : IEntityTypeConfiguration<PickupAddress>
{
    public void Configure(EntityTypeBuilder<PickupAddress> builder)
    {
        builder.HasNoKey();
        builder
            .Property(x => x.Country)
            .HasMaxLength(IAddress.Constraints.CountryMaxLength)
            .IsRequired();
        builder
            .Property(x => x.State)
            .HasMaxLength(IAddress.Constraints.StateMaxLength);
        builder
            .Property(x => x.ZipCode)
            .HasMaxLength(IAddress.Constraints.ZipCodeMaxLength)
            .IsRequired();
        builder
            .Property(x => x.City)
            .HasMaxLength(IAddress.Constraints.CityMaxLength)
            .IsRequired();
        builder
            .Property(x => x.Street)
            .HasMaxLength(IAddress.Constraints.StreetMaxLength);
        builder
            .Property(x => x.BuildingNumber)
            .HasMaxLength(IAddress.Constraints.BuildingNumberMaxLength);
    }
}
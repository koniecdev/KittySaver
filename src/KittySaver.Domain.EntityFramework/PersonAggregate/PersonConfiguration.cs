using KittySaver.Domain.Persons.ValueObjects;
using KittySaver.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.Domain.Persons.Entities;

public sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("Persons");

        builder.Property(person => person.Id).ValueGeneratedNever();

        builder.HasKey(person => person.Id);

        builder.HasMany(person => person.Cats)
            .WithOne()
            .HasForeignKey(cat => cat.PersonId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasMany(person => person.Advertisements)
            .WithOne()
            .HasForeignKey(advertisement => advertisement.PersonId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.ComplexProperty(x => x.DefaultAdvertisementsPickupAddress, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder.Property(x => x.Country)
                .HasColumnName(
                    $"{nameof(Person.DefaultAdvertisementsPickupAddress)}{nameof(Person.DefaultAdvertisementsPickupAddress.Country)}")
                .HasMaxLength(Address.CountryMaxLength)
                .IsRequired();

            complexPropertyBuilder.Property(x => x.State)
                .HasColumnName(
                    $"{nameof(Person.DefaultAdvertisementsPickupAddress)}{nameof(Person.DefaultAdvertisementsPickupAddress.State)}")
                .HasMaxLength(Address.StateMaxLength);

            complexPropertyBuilder.Property(x => x.ZipCode)
                .HasColumnName(
                    $"{nameof(Person.DefaultAdvertisementsPickupAddress)}{nameof(Person.DefaultAdvertisementsPickupAddress.ZipCode)}")
                .HasMaxLength(Address.ZipCodeMaxLength)
                .IsRequired();

            complexPropertyBuilder.Property(x => x.City)
                .HasColumnName(
                    $"{nameof(Person.DefaultAdvertisementsPickupAddress)}{nameof(Person.DefaultAdvertisementsPickupAddress.City)}")
                .HasMaxLength(Address.CityMaxLength)
                .IsRequired();

            complexPropertyBuilder.Property(x => x.Street)
                .HasColumnName(
                    $"{nameof(Person.DefaultAdvertisementsPickupAddress)}{nameof(Person.DefaultAdvertisementsPickupAddress.Street)}")
                .HasMaxLength(Address.StreetMaxLength);

            complexPropertyBuilder.Property(x => x.BuildingNumber)
                .HasColumnName(
                    $"{nameof(Person.DefaultAdvertisementsPickupAddress)}{nameof(Person.DefaultAdvertisementsPickupAddress.BuildingNumber)}")
                .HasMaxLength(Address.BuildingNumberMaxLength);
        });

        builder.ComplexProperty(x => x.DefaultAdvertisementsContactInfoEmail, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder.Property(x => x.Value)
                .HasColumnName($"{nameof(Person.DefaultAdvertisementsContactInfoEmail)}")
                .HasMaxLength(Email.MaxLength)
                .IsRequired();
        });

        builder.ComplexProperty(x => x.DefaultAdvertisementsContactInfoPhoneNumber, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder.Property(x => x.Value)
                .HasColumnName($"{nameof(Person.DefaultAdvertisementsContactInfoPhoneNumber)}")
                .HasMaxLength(PhoneNumber.MaxLength)
                .IsRequired();
        });

        builder.ComplexProperty(x => x.Nickname, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder.Property(x => x.Value)
                .HasColumnName($"{nameof(Person.Nickname)}")
                .HasMaxLength(Nickname.MaxLength)
                .IsRequired();
        });

        builder.ComplexProperty(x => x.Email, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder.Property(x => x.Value)
                .HasColumnName($"{nameof(Person.Email)}")
                .HasMaxLength(Email.MaxLength)
                .IsRequired();
        });

        builder.ComplexProperty(x => x.PhoneNumber, complexPropertyBuilder =>
        {
            complexPropertyBuilder.IsRequired();

            complexPropertyBuilder.Property(x => x.Value)
                .HasColumnName($"{nameof(Person.PhoneNumber)}")
                .HasMaxLength(PhoneNumber.MaxLength)
                .IsRequired();
        });

        builder.Property(x => x.UserIdentityId)
            .IsRequired();
    }
}
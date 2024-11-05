using System.Reflection;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Shared.Domain.Entites;
using KittySaver.Api.Shared.Domain.Enums;
using KittySaver.Api.Shared.Domain.Services;
using KittySaver.Api.Shared.Domain.ValueObjects;
using NSubstitute;

namespace KittySaver.Api.Tests.Unit.Advertisements;

using Person = Shared.Domain.Entites.Person;

public class AdvertisementTests
{
    private static readonly DateTimeOffset Date = new(2024, 10, 31, 11, 0, 0, TimeSpan.Zero);

    private static readonly Address Address = new Faker<Address>()
        .CustomInstantiator(faker =>
            new Address
            {
                Country = faker.Address.Country(),
                State = faker.Address.State(),
                ZipCode = faker.Address.ZipCode(),
                City = faker.Address.City(),
                Street = faker.Address.StreetName(),
                BuildingNumber = faker.Address.BuildingNumber()
            }).Generate();

    private static readonly Faker<PickupAddress> PickupAddress = new Faker<PickupAddress>()
        .CustomInstantiator(faker =>
            new PickupAddress
            {
                Country = faker.Address.Country(),
                State = faker.Address.State(),
                ZipCode = faker.Address.ZipCode(),
                City = faker.Address.City(),
                Street = faker.Address.StreetName(),
                BuildingNumber = faker.Address.BuildingNumber()
            });

    private static readonly Faker<ContactInfo> ContactInfo = new Faker<ContactInfo>()
        .CustomInstantiator(faker =>
            new ContactInfo
            {
                Email = faker.Person.Email,
                PhoneNumber = faker.Person.Phone
            });

    private static readonly Person Person = new Faker<Person>()
        .CustomInstantiator(faker =>
            Person.Create(
                userIdentityId: Guid.NewGuid(),
                firstName: faker.Person.FirstName,
                lastName: faker.Person.LastName,
                email: faker.Person.Email,
                phoneNumber: faker.Person.Phone,
                address: Address,
                defaultAdvertisementPickupAddress: PickupAddress.Generate(),
                defaultAdvertisementContactInfo: ContactInfo.Generate()
            )).Generate();

    private static readonly Faker<Cat> CatGenerator = new Faker<Cat>()
        .CustomInstantiator(faker =>
            Cat.Create(
                calculator: new DefaultCatPriorityCalculator(),
                person: Person,
                name: faker.Person.FirstName,
                medicalHelpUrgency: faker.PickRandomParam(MedicalHelpUrgency.NoNeed, MedicalHelpUrgency.ShouldSeeVet,
                    MedicalHelpUrgency.HaveToSeeVet),
                ageCategory: faker.PickRandomParam(AgeCategory.Baby, AgeCategory.Adult, AgeCategory.Senior),
                behavior: faker.PickRandomParam(Behavior.Unfriendly, Behavior.Friendly),
                healthStatus: faker.PickRandomParam(HealthStatus.Critical, HealthStatus.Poor, HealthStatus.Good),
                isCastrated: faker.PickRandomParam(true, false),
                isInNeedOfSeeingVet: faker.PickRandomParam(true, false)
            ));

    [Fact]
    public void CreateAdvertisement_ShouldCreateSuccessfully_WhenValidDataAreProvided()
    {
        //Arrange
        List<Cat> cats = [CatGenerator.Generate(), CatGenerator.Generate(), CatGenerator.Generate()];
        PickupAddress pickupAddress = PickupAddress.Generate();
        ContactInfo contactInfo = ContactInfo.Generate();

        //Act
        Advertisement advertisement = Advertisement.Create(
            currentDate: Date,
            person: Person,
            cats: cats,
            pickupAddress: pickupAddress,
            contactInfo: contactInfo,
            description: "lorem ipsum");

        //Assert
        advertisement.Should().NotBeNull();
        advertisement.Id.Should().NotBeEmpty();
        advertisement.PickupAddress.Should().BeEquivalentTo(pickupAddress);
        advertisement.ContactInfo.Should().BeEquivalentTo(contactInfo);
        advertisement.PriorityScore.Should().BeGreaterThan(0);
        advertisement.ExpiresOn.Should().Be(Date.AddDays(30));
        advertisement.Description.Should().Be("lorem ipsum");
        advertisement.Status.Should().Be(Advertisement.AdvertisementStatus.Active);

        advertisement.PersonId.Should().Be(Person.Id);
        advertisement.Person.Should().BeEquivalentTo(Person);
        advertisement.Person.Advertisements.Should().Contain(advertisement);

        advertisement.Cats.Should().BeEquivalentTo(cats);
        advertisement.Cats.All(x => x.AdvertisementId == advertisement.Id).Should().BeTrue();
        advertisement.Cats.All(x => x.Advertisement == advertisement).Should().BeTrue();
    }

    [Fact]
    public void CreateAdvertisement_ShouldThrowArgumentException_WhenEmptyCatsListIsProvided()
    {
        //Act
        Action creation = () =>
        {
            Advertisement.Create(
                currentDate: Date,
                person: Person,
                cats: [],
                pickupAddress: PickupAddress,
                contactInfo: ContactInfo);
        };
        
        //Assert
        creation.Should()
            .ThrowExactly<ArgumentException>()
            .WithMessage("Advertisement cats list must not be empty. (Parameter 'cats')");
    }

    [Fact]
    public void CreateAdvertisement_ShouldThrowArgumentException_WhenWrongPersonIsProvided()
    {
        //Arrange
        Person invalidPerson = new Faker<Person>()
            .CustomInstantiator(faker =>
                Person.Create(
                    userIdentityId: Guid.NewGuid(),
                    firstName: faker.Person.FirstName,
                    lastName: faker.Person.LastName,
                    email: faker.Person.Email,
                    phoneNumber: faker.Person.Phone,
                    address: Address,
                    defaultAdvertisementPickupAddress: PickupAddress.Generate(),
                    defaultAdvertisementContactInfo: ContactInfo.Generate()
                )).Generate();
        List<Cat> cats = [CatGenerator.Generate(), CatGenerator.Generate(), CatGenerator.Generate()];
        PickupAddress pickupAddress = PickupAddress.Generate();
        ContactInfo contactInfo = ContactInfo.Generate();

        //Act
        Action creation = () =>
        {
            Advertisement.Create(
                currentDate: Date,
                person: invalidPerson,
                cats: cats,
                pickupAddress: pickupAddress,
                contactInfo: contactInfo);
        };

        //Assert
        creation.Should()
            .ThrowExactly<ArgumentException>()
            .WithMessage("One or more provided cats do not belong to provided person. (Parameter 'cats')");
    }

    [Fact]
    public void CreateAdvertisement_ShouldThrowArgumentException_WhenWrongCatIsProvided()
    {
        //Arrange
        Person invalidPerson = new Faker<Person>()
            .CustomInstantiator(faker =>
                Person.Create(
                    userIdentityId: Guid.NewGuid(),
                    firstName: faker.Person.FirstName,
                    lastName: faker.Person.LastName,
                    email: faker.Person.Email,
                    phoneNumber: faker.Person.Phone,
                    address: Address,
                    defaultAdvertisementPickupAddress: PickupAddress.Generate(),
                    defaultAdvertisementContactInfo: ContactInfo.Generate()
                )).Generate();
        Cat invalidPersonCat = new Faker<Cat>()
            .CustomInstantiator(faker =>
                Cat.Create(
                    calculator: new DefaultCatPriorityCalculator(),
                    person: invalidPerson,
                    name: faker.Person.FirstName,
                    medicalHelpUrgency: faker.PickRandomParam(MedicalHelpUrgency.NoNeed,
                        MedicalHelpUrgency.ShouldSeeVet,
                        MedicalHelpUrgency.HaveToSeeVet),
                    ageCategory: faker.PickRandomParam(AgeCategory.Baby, AgeCategory.Adult, AgeCategory.Senior),
                    behavior: faker.PickRandomParam(Behavior.Unfriendly, Behavior.Friendly),
                    healthStatus: faker.PickRandomParam(HealthStatus.Critical, HealthStatus.Poor, HealthStatus.Good),
                    isCastrated: faker.PickRandomParam(true, false),
                    isInNeedOfSeeingVet: faker.PickRandomParam(true, false)
                )).Generate();
        List<Cat> cats = [CatGenerator.Generate(), CatGenerator.Generate(), CatGenerator.Generate()];
        SharedHelper.SetBackingFieldDirectly(Person, "_cats", cats);
        SharedHelper.SetBackingFieldDirectly(invalidPerson, "_cats", new List<Cat> { invalidPersonCat });
        PickupAddress pickupAddress = PickupAddress.Generate();
        ContactInfo contactInfo = ContactInfo.Generate();
        cats.Add(invalidPersonCat);

        //Act
        Action creation = () =>
        {
            Advertisement.Create(
                currentDate: Date,
                person: invalidPerson,
                cats: cats,
                pickupAddress: pickupAddress,
                contactInfo: contactInfo);
        };

        //Assert
        creation.Should()
            .ThrowExactly<ArgumentException>()
            .WithMessage("One or more provided cats do not belong to provided person. (Parameter 'cats')");
    }

    [Fact]
    public void CreateAdvertisement_ShouldThrowInvalidOperationException_WhenAlreadyAssignedCatsAreProvided()
    {
        //Arrange
        List<Cat> cats = [CatGenerator.Generate(), CatGenerator.Generate(), CatGenerator.Generate()];
        SharedHelper.SetBackingFieldDirectly(Person, "_cats", cats);
        PickupAddress pickupAddress = PickupAddress.Generate();
        ContactInfo contactInfo = ContactInfo.Generate();
        Advertisement.Create(
            currentDate: Date,
            person: Person,
            cats: cats,
            pickupAddress: pickupAddress,
            contactInfo: contactInfo);

        //Act
        Action creation = () =>
        {
            Advertisement.Create(
                currentDate: Date,
                person: Person,
                cats: cats,
                pickupAddress: pickupAddress,
                contactInfo: contactInfo);
        };

        //Assert
        creation.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void PersonId_ShouldThrowArgumentException_WhenProvidedEmptyValue()
    {
        //Arrange
        Person person = new Faker<Person>()
            .CustomInstantiator(faker =>
                Person.Create(
                    userIdentityId: Guid.NewGuid(),
                    firstName: faker.Person.FirstName,
                    lastName: faker.Person.LastName,
                    email: faker.Person.Email,
                    phoneNumber: faker.Person.Phone,
                    address: Address,
                    defaultAdvertisementPickupAddress: PickupAddress,
                    defaultAdvertisementContactInfo: ContactInfo
                )).Generate();
        Cat cat = Cat.Create(
            calculator: Substitute.For<ICatPriorityCalculator>(),
            person: person,
            name: "Whiskers",
            medicalHelpUrgency: MedicalHelpUrgency.HaveToSeeVet,
            ageCategory: AgeCategory.Senior,
            behavior: Behavior.Friendly,
            healthStatus: HealthStatus.Critical
        );
        SharedHelper.SetBackingField(person, nameof(Person.Id), Guid.Empty);

        //Act
        Action creation = () =>
        {
            Advertisement.Create(
                currentDate: Date,
                person: person,
                cats: [cat],
                pickupAddress: PickupAddress,
                contactInfo: ContactInfo);
        };
        //Assert
        creation.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Close_ShouldCloseSuccessfully_WhenValidDataAreProvided()
    {
        //Arrange
        Advertisement advertisement = Advertisement.Create(
                currentDate: Date,
                person: Person,
                cats: [CatGenerator.Generate()],
                pickupAddress: PickupAddress,
                contactInfo: ContactInfo);
        
        //Act
        DateTimeOffset closureDate = Date.AddDays(1);
        advertisement.Close(closureDate);

        //Assert
        advertisement.Status.Should().Be(Advertisement.AdvertisementStatus.Closed);
        advertisement.ClosedOn.Should().Be(closureDate);
        advertisement.Cats.Select(x => x.IsAdopted).All(x => x).Should().BeTrue();
    }
    
    [Fact]
    public void Expire_ShouldCloseSuccessfully_WhenValidDataAreProvided()
    {
        //Arrange
        Cat cat = CatGenerator.Generate();

        Advertisement advertisement = Advertisement.Create(
            currentDate: Date,
            person: Person,
            cats: [cat],
            pickupAddress: PickupAddress,
            contactInfo: ContactInfo);
        
        //Act
        DateTimeOffset expirationDate = advertisement.ExpiresOn.AddDays(1);
        advertisement.Expire(expirationDate);
        
        //Assert
        advertisement.Status.Should().Be(Advertisement.AdvertisementStatus.Expired);
    }
}
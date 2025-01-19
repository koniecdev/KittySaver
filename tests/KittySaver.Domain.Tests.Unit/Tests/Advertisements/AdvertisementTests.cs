using Bogus;
using FluentAssertions;
using KittySaver.Domain.Common.Primitives.Enums;
using KittySaver.Domain.Persons;
using KittySaver.Domain.ValueObjects;
using Address = KittySaver.Domain.ValueObjects.Address;
using Person = KittySaver.Domain.Persons.Person;

namespace KittySaver.Domain.Tests.Unit.Tests.Advertisements;

using Person = Person;

public class AdvertisementTests
{
    private static readonly DateTimeOffset Date = new(2024, 10, 31, 11, 0, 0, TimeSpan.Zero);

    private static readonly Address Address = new Faker<Address>()
        .CustomInstantiator(faker =>
            Address.Create(
                country: faker.Address.CountryCode(),
                state: faker.Address.State(),
                zipCode: faker.Address.ZipCode(),
                city: faker.Address.City(),
                street: faker.Address.StreetName(),
                buildingNumber: faker.Address.BuildingNumber()
            )).Generate();

    private static readonly Faker<Address> PickupAddressGenerator = new Faker<Address>()
        .CustomInstantiator(faker =>
            Address.Create(
                country: faker.Address.CountryCode(),
                state: faker.Address.State(),
                zipCode: faker.Address.ZipCode(),
                city: faker.Address.City(),
                street: faker.Address.StreetName(),
                buildingNumber: faker.Address.BuildingNumber()
            ));

    private static readonly Faker<Email> ContactInfoEmailGenerator = new Faker<Email>()
        .CustomInstantiator(faker => Email.Create(faker.Person.Email));
    
    private static readonly Faker<PhoneNumber> ContactInfoPhoneNumberGenerator = new Faker<PhoneNumber>()
        .CustomInstantiator(faker => PhoneNumber.Create(faker.Person.Phone));
    
    private static readonly Person Person = new Faker<Person>()
        .CustomInstantiator(faker =>
            Person.Create(
                userIdentityId: Guid.NewGuid(),
                nickname: Nickname.Create(faker.Person.FirstName),
                email: Email.Create(faker.Person.Email),
                phoneNumber: PhoneNumber.Create(faker.Person.Phone),
                defaultAdvertisementPickupAddress: PickupAddressGenerator.Generate(),
                defaultAdvertisementContactInfoEmail: ContactInfoEmailGenerator.Generate(),
                defaultAdvertisementContactInfoPhoneNumber: ContactInfoPhoneNumberGenerator.Generate()
            )).Generate();

    private static readonly Faker<Cat> CatGenerator = new Faker<Cat>()
        .CustomInstantiator(faker =>
            Person.AddCat(
                priorityScoreCalculator: new DefaultCatPriorityCalculatorService(),
                name: CatName.Create(faker.Person.FirstName),
                medicalHelpUrgency: faker.PickRandomParam(MedicalHelpUrgency.NoNeed, MedicalHelpUrgency.ShouldSeeVet,
                    MedicalHelpUrgency.HaveToSeeVet),
                ageCategory: faker.PickRandomParam(AgeCategory.Baby, AgeCategory.Adult, AgeCategory.Senior),
                behavior: faker.PickRandomParam(Behavior.Unfriendly, Behavior.Friendly),
                healthStatus: faker.PickRandomParam(HealthStatus.Critical, HealthStatus.Poor, HealthStatus.Good),
                isCastrated: faker.PickRandomParam(true, false),
                additionalRequirements: Description.Create(faker.Lorem.Lines(2))
            ));

    [Fact]
    public void CreateAdvertisement_ShouldCreateSuccessfully_WhenValidDataAreProvided()
    {
        //Arrange
        List<Cat> cats = [CatGenerator.Generate(), CatGenerator.Generate(), CatGenerator.Generate()];
        Address pickupAddress = PickupAddressGenerator.Generate();
        Email contactInfoEmail = ContactInfoEmailGenerator.Generate();
        PhoneNumber contactInfoPhoneNumber = ContactInfoPhoneNumberGenerator.Generate();

        //Act
        Advertisement advertisement = Person.AddAdvertisement(
            dateOfCreation: Date,
            catsIdsToAssign: cats.Select(x=>x.Id),
            pickupAddress: pickupAddress,
            contactInfoEmail: contactInfoEmail,
            contactInfoPhoneNumber: contactInfoPhoneNumber,
            description: Description.Create("lorem ipsum"));
        
        //Assert
        advertisement.Should().NotBeNull();
        advertisement.Id.Should().NotBeEmpty();
        advertisement.PickupAddress.Should().BeEquivalentTo(pickupAddress);
        advertisement.ContactInfoEmail.Should().BeEquivalentTo(contactInfoEmail);
        advertisement.ContactInfoPhoneNumber.Should().BeEquivalentTo(contactInfoPhoneNumber);
        advertisement.ExpiresOn.Should().Be(Date.AddDays(30));
        advertisement.Description.Value.Should().Be("Lorem ipsum");
        advertisement.Status.Should().Be(Advertisement.AdvertisementStatus.ThumbnailNotUploaded);
        advertisement.PersonId.Should().Be(Person.Id);
        advertisement.PriorityScore.Should().NotBe(0);
    }
    
    [Fact]
    public void CreateAdvertisement_ShouldThrowArgumentException_WhenEmptyCatsListIsProvided()
    {
        //Arrange
        Address pickupAddress = PickupAddressGenerator.Generate();
        Email contactInfoEmail = ContactInfoEmailGenerator.Generate();
        PhoneNumber contactInfoPhoneNumber = ContactInfoPhoneNumberGenerator.Generate();
        
        //Act
        Action creation = () =>
        {
            Person.AddAdvertisement(
                dateOfCreation: Date,
                catsIdsToAssign: [],
                pickupAddress: pickupAddress,
                contactInfoEmail: contactInfoEmail,
                contactInfoPhoneNumber: contactInfoPhoneNumber,
                description: Description.Create("lorem ipsum"));
        };
        
        //Assert
        creation.Should()
            .ThrowExactly<ArgumentException>()
            .WithMessage("Advertisement cats list must not be empty. (Parameter 'catsIdsToAssign')");
    }
    
    [Fact]
    public void Close_ShouldCloseSuccessfully_WhenValidDataAreProvided()
    {
        //Arrange
        Advertisement advertisement = Person.AddAdvertisement(
                dateOfCreation: Date,
                catsIdsToAssign: [CatGenerator.Generate().Id],
                pickupAddress: PickupAddressGenerator.Generate(),
                contactInfoEmail: ContactInfoEmailGenerator.Generate(),
                contactInfoPhoneNumber: ContactInfoPhoneNumberGenerator.Generate(),
                description: Description.Create("lorem ipsum"));
        
        Person.ActivateAdvertisementIfThumbnailIsUploadedForTheFirstTime(advertisement.Id);
        
        //Act
        DateTimeOffset closureDate = Date.AddDays(1);
        advertisement.Close(closureDate);

        //Assert
        advertisement.Status.Should().Be(Advertisement.AdvertisementStatus.Closed);
        advertisement.ClosedOn.Should().Be(closureDate);
    }
    
    [Fact]
    public void Close_ShouldThrowInvalidOperationException_WhenNotActiveAdvertisementIsProvided()
    {
        //Arrange
        Advertisement advertisement = Person.AddAdvertisement(
            dateOfCreation: Date,
            catsIdsToAssign: [CatGenerator.Generate().Id],
            pickupAddress: PickupAddressGenerator.Generate(),
            contactInfoEmail: ContactInfoEmailGenerator.Generate(),
            contactInfoPhoneNumber: ContactInfoPhoneNumberGenerator.Generate(),
            description: Description.Create("lorem ipsum"));
        Person.ActivateAdvertisementIfThumbnailIsUploadedForTheFirstTime(advertisement.Id);

        
        advertisement.Expire(Date.AddDays(60));
        
        //Act
        DateTimeOffset closureDate = Date.AddDays(62);
        Action action = () => advertisement.Close(closureDate);

        //Assert
        action.Should().ThrowExactly<InvalidOperationException>();
    }
    
    [Fact]
    public void Expire_ShouldExpireSuccessfully_WhenValidDataAreProvided()
    {
        //Arrange
        Cat cat = CatGenerator.Generate();

        Advertisement advertisement = Person.AddAdvertisement(
            dateOfCreation: Date,
            catsIdsToAssign: [cat.Id],
            pickupAddress: PickupAddressGenerator,
            contactInfoEmail: ContactInfoEmailGenerator.Generate(),
            contactInfoPhoneNumber: ContactInfoPhoneNumberGenerator.Generate(),
            description: Description.Create("lorem ipsum"));
        Person.ActivateAdvertisementIfThumbnailIsUploadedForTheFirstTime(advertisement.Id);
        
        //Act
        DateTimeOffset expirationDate = advertisement.ExpiresOn.AddDays(1);
        advertisement.Expire(expirationDate);
        
        //Assert
        advertisement.Status.Should().Be(Advertisement.AdvertisementStatus.Expired);
    }
    
    [Fact]
    public void Expire_ShouldThrowInvalidOperationException_WhenNotActiveAdvertisementIsProvided()
    {
        //Arrange
        Advertisement advertisement = Person.AddAdvertisement(
            dateOfCreation: Date,
            catsIdsToAssign: [CatGenerator.Generate().Id],
            pickupAddress: PickupAddressGenerator.Generate(),
            contactInfoEmail: ContactInfoEmailGenerator.Generate(),
            contactInfoPhoneNumber: ContactInfoPhoneNumberGenerator.Generate(),
            description: Description.Create("lorem ipsum"));
        Person.ActivateAdvertisementIfThumbnailIsUploadedForTheFirstTime(advertisement.Id);
        advertisement.Expire(Date.AddDays(60));
        
        //Act
        DateTimeOffset expireDate = Date.AddDays(62);
        Action action = () => advertisement.Expire(expireDate);

        //Assert
        action.Should().ThrowExactly<InvalidOperationException>();
    }
    
    [Fact]
    public void Refresh_ShouldExpireSuccessfully_WhenValidDataAreProvided()
    {
        //Arrange
        Cat cat = CatGenerator.Generate();

        Advertisement advertisement = Person.AddAdvertisement(
            dateOfCreation: Date,
            catsIdsToAssign: [cat.Id],
            pickupAddress: PickupAddressGenerator,
            contactInfoEmail: ContactInfoEmailGenerator.Generate(),
            contactInfoPhoneNumber: ContactInfoPhoneNumberGenerator.Generate(),
            description: Description.Create("lorem ipsum"));
        
        Person.ActivateAdvertisementIfThumbnailIsUploadedForTheFirstTime(advertisement.Id);
        
        DateTimeOffset expirationDate = advertisement.ExpiresOn.AddDays(1);
        advertisement.Expire(expirationDate);
        
        //Act
        DateTimeOffset refreshDate = expirationDate.AddDays(2);
        advertisement.Refresh(refreshDate);
        
        //Assert
        advertisement.Status.Should().Be(Advertisement.AdvertisementStatus.Active);
    }
    
    [Fact]
    public void Refresh_ShouldThrowInvalidOperationException_WhenNotActiveAdvertisementIsProvided()
    {
        //Arrange
        Advertisement advertisement = Person.AddAdvertisement(
            dateOfCreation: Date,
            catsIdsToAssign: [CatGenerator.Generate().Id],
            pickupAddress: PickupAddressGenerator.Generate(),
            contactInfoEmail: ContactInfoEmailGenerator.Generate(),
            contactInfoPhoneNumber: ContactInfoPhoneNumberGenerator.Generate(),
            description: Description.Create("lorem ipsum"));
        Person.ActivateAdvertisementIfThumbnailIsUploadedForTheFirstTime(advertisement.Id);
        DateTimeOffset closureDate = advertisement.ExpiresOn.AddDays(1);
        advertisement.Close(closureDate);
        
        //Act
        DateTimeOffset refreshDate = closureDate.AddDays(2);
        Action action = () => advertisement.Refresh(refreshDate);

        //Assert
        action.Should().ThrowExactly<InvalidOperationException>();
    }
    
    [Fact]
    public void PersonIdSet_ShouldThrowArgumentException_WhenProvidedEmptyValue()
    {
        Person person = new Faker<Person>()
        .CustomInstantiator(faker =>
            Person.Create(
                userIdentityId: Guid.NewGuid(),
                nickname: Nickname.Create(faker.Person.FirstName),
                email: Email.Create(faker.Person.Email),
                phoneNumber: PhoneNumber.Create(faker.Person.Phone),
                defaultAdvertisementPickupAddress: Address,
                defaultAdvertisementContactInfoEmail: Email.Create(faker.Person.Email),
                defaultAdvertisementContactInfoPhoneNumber: PhoneNumber.Create(faker.Person.Phone)
            )).Generate();

        Cat cat = new Faker<Cat>()
            .CustomInstantiator(faker =>
                person.AddCat(
                    priorityScoreCalculator: new DefaultCatPriorityCalculatorService(),
                    name: CatName.Create(faker.Person.FirstName),
                    medicalHelpUrgency: faker.PickRandomParam(MedicalHelpUrgency.NoNeed,
                        MedicalHelpUrgency.ShouldSeeVet,
                        MedicalHelpUrgency.HaveToSeeVet),
                    ageCategory: faker.PickRandomParam(AgeCategory.Baby, AgeCategory.Adult, AgeCategory.Senior),
                    behavior: faker.PickRandomParam(Behavior.Unfriendly, Behavior.Friendly),
                    healthStatus: faker.PickRandomParam(HealthStatus.Critical, HealthStatus.Poor, HealthStatus.Good),
                    isCastrated: faker.PickRandomParam(true, false),
                    additionalRequirements: Description.Create(faker.Lorem.Lines(2))
                )).Generate();
        
        SharedHelper.SetBackingField(person, nameof(Person.Id), Guid.Empty);
        
        //Act
        Action createAdvertisement = () =>
        {
            person.AddAdvertisement(
                dateOfCreation: new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
                catsIdsToAssign: [cat.Id],
                pickupAddress: person.DefaultAdvertisementsPickupAddress,
                contactInfoEmail: person.DefaultAdvertisementsContactInfoEmail,
                contactInfoPhoneNumber: person.DefaultAdvertisementsContactInfoPhoneNumber,
                description: Description.Create("Lorem ipsum"));
        };
        
        //Assert
        createAdvertisement.Should().Throw<ArgumentException>().WithMessage("Provided person id is empty. (Parameter 'PersonId')");
    }
    
    [Fact]
    public void Activate_ShouldThrowInvalidOperationException_WhenAdvertisementHasWrongStatus()
    {
        //Arrange
        List<Cat> cats = [CatGenerator.Generate(), CatGenerator.Generate(), CatGenerator.Generate()];
        Address pickupAddress = PickupAddressGenerator.Generate();
        Email contactInfoEmail = ContactInfoEmailGenerator.Generate();
        PhoneNumber contactInfoPhoneNumber = ContactInfoPhoneNumberGenerator.Generate();

        Advertisement advertisement = Person.AddAdvertisement(
        dateOfCreation: Date,
        catsIdsToAssign: cats.Select(x=>x.Id),
        pickupAddress: pickupAddress,
        contactInfoEmail: contactInfoEmail,
        contactInfoPhoneNumber: contactInfoPhoneNumber,
        description: Description.Create("lorem ipsum"));
        
        Person.ActivateAdvertisementIfThumbnailIsUploadedForTheFirstTime(advertisement.Id);
        
        //Act
        Action activation = advertisement.Activate;
        
        //Assert
        activation.Should().ThrowExactly<InvalidOperationException>();
    }
}
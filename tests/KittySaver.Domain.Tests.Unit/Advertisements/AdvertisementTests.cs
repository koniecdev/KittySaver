using Bogus;
using FluentAssertions;
using KittySaver.Domain.Advertisements;
using KittySaver.Domain.Common.Primitives.Enums;
using KittySaver.Domain.Persons;
using KittySaver.Domain.ValueObjects;
using Person = KittySaver.Domain.Persons.Person;

namespace KittySaver.Domain.Tests.Unit.Advertisements;

using Person = Person;

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

    private static readonly Faker<Address> PickupAddressGenerator = new Faker<Address>()
        .CustomInstantiator(faker =>
            new Address
            {
                Country = faker.Address.Country(),
                State = faker.Address.State(),
                ZipCode = faker.Address.ZipCode(),
                City = faker.Address.City(),
                Street = faker.Address.StreetName(),
                BuildingNumber = faker.Address.BuildingNumber()
            });

    private static readonly Faker<Email> ContactInfoEmailGenerator = new Faker<Email>()
        .CustomInstantiator(faker => Email.Create(faker.Person.Email));
    
    private static readonly Faker<PhoneNumber> ContactInfoPhoneNumberGenerator = new Faker<PhoneNumber>()
        .CustomInstantiator(faker => PhoneNumber.Create(faker.Person.Phone));
    
    private static readonly Person Person = new Faker<Person>()
        .CustomInstantiator(faker =>
            Person.Create(
                userIdentityId: Guid.NewGuid(),
                firstName: FirstName.Create(faker.Person.FirstName),
                lastName: LastName.Create(faker.Person.LastName),
                email: Email.Create(faker.Person.Email),
                phoneNumber: PhoneNumber.Create(faker.Person.Phone),
                residentalAddress: Address,
                defaultAdvertisementPickupAddress: PickupAddressGenerator.Generate(),
                defaultAdvertisementContactInfoEmail: ContactInfoEmailGenerator.Generate(),
                defaultAdvertisementContactInfoPhoneNumber: ContactInfoPhoneNumberGenerator.Generate()
            )).Generate();

    private static readonly Faker<Cat> CatGenerator = new Faker<Cat>()
        .CustomInstantiator(faker =>
            Cat.Create(
                priorityScoreCalculator: new DefaultCatPriorityCalculatorService(),
                person: Person,
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
        Advertisement advertisement = Advertisement.Create(
            currentDate: Date,
            person: Person,
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
        advertisement.Status.Should().Be(Advertisement.AdvertisementStatus.Active);
        advertisement.PersonId.Should().Be(Person.Id);
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
            Advertisement.Create(
                currentDate: Date,
                person: Person,
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
        Advertisement advertisement = Advertisement.Create(
                currentDate: Date,
                person: Person,
                catsIdsToAssign: [CatGenerator.Generate().Id],
                pickupAddress: PickupAddressGenerator.Generate(),
                contactInfoEmail: ContactInfoEmailGenerator.Generate(),
                contactInfoPhoneNumber: ContactInfoPhoneNumberGenerator.Generate(),
                description: Description.Create("lorem ipsum"));
        
        //Act
        DateTimeOffset closureDate = Date.AddDays(1);
        advertisement.Close(closureDate);

        //Assert
        advertisement.Status.Should().Be(Advertisement.AdvertisementStatus.Closed);
        advertisement.ClosedOn.Should().Be(closureDate);
    }
    
    [Fact]
    public void Expire_ShouldExpireSuccessfully_WhenValidDataAreProvided()
    {
        //Arrange
        Cat cat = CatGenerator.Generate();

        Advertisement advertisement = Advertisement.Create(
            currentDate: Date,
            person: Person,
            catsIdsToAssign: [cat.Id],
            pickupAddress: PickupAddressGenerator,
            contactInfoEmail: ContactInfoEmailGenerator.Generate(),
            contactInfoPhoneNumber: ContactInfoPhoneNumberGenerator.Generate(),
            description: Description.Create("lorem ipsum"));
        
        //Act
        DateTimeOffset expirationDate = advertisement.ExpiresOn.AddDays(1);
        advertisement.Expire(expirationDate);
        
        //Assert
        advertisement.Status.Should().Be(Advertisement.AdvertisementStatus.Expired);
    }
}
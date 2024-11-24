using Bogus;
using FluentAssertions;
using KittySaver.Api.Shared.Domain.Advertisements;
using KittySaver.Api.Shared.Domain.Common.Primitives.Enums;
using KittySaver.Api.Shared.Domain.Persons;
using KittySaver.Api.Shared.Domain.ValueObjects;
using Person = KittySaver.Api.Shared.Domain.Persons.Person;

namespace KittySaver.Api.Tests.Unit.Advertisements;

using Person = Person;

public class CatAdvertisementAssignmentServiceTests
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

        Advertisement advertisement = Advertisement.Create(
            currentDate: Date,
            person: Person,
            catsIdsToAssign: cats.Select(x=>x.Id),
            pickupAddress: pickupAddress,
            contactInfoEmail: contactInfoEmail,
            contactInfoPhoneNumber: contactInfoPhoneNumber,
            description: Description.Create("lorem ipsum"));
        
        //Act
        Cat catToAssign = CatGenerator.Generate();
        AdvertisementService service = new();
        service.ReplaceCatsOfAdvertisement(person: Person,
            advertisement: advertisement,
            catIdsToAssign: [catToAssign.Id]);

        //Assert
        advertisement.Should().NotBeNull();
        advertisement.Id.Should().NotBeEmpty();
        Person.Cats
            .Count(x => x.AdvertisementId == advertisement.Id)
            .Should()
            .Be(1);
    }
}
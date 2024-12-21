using Bogus;
using FluentAssertions;
using KittySaver.Domain.Common.Primitives.Enums;
using KittySaver.Domain.Persons;
using KittySaver.Domain.ValueObjects;
using Person = KittySaver.Domain.Persons.Person;

namespace KittySaver.Domain.Tests.Unit.Tests.Cats;

public class DefaultCatPriorityCalculatorServiceTests
{
    private static readonly Address Address = new Faker<Address>()
        .CustomInstantiator(faker =>
            Address.Create(
                country: faker.Address.Country(),
                state: faker.Address.State(),
                zipCode: faker.Address.ZipCode(),
                city: faker.Address.City(),
                street: faker.Address.StreetName(),
                buildingNumber: faker.Address.BuildingNumber()
            )).Generate();

    private static readonly Address PickupAddress = new Faker<Address>()
        .CustomInstantiator(faker =>
            Address.Create(
                country: faker.Address.Country(),
                state: faker.Address.State(),
                zipCode: faker.Address.ZipCode(),
                city: faker.Address.City(),
                street: faker.Address.StreetName(),
                buildingNumber: faker.Address.BuildingNumber()
            )).Generate();
    
    private static readonly Person Person = new Faker<Person>()
        .CustomInstantiator(faker =>
            Person.Create(
                userIdentityId: Guid.NewGuid(),
                nickname: Nickname.Create(faker.Person.FirstName),
                email: Email.Create(faker.Person.Email),
                phoneNumber: PhoneNumber.Create(faker.Person.Phone),
                defaultAdvertisementPickupAddress: PickupAddress,
                defaultAdvertisementContactInfoEmail: Email.Create(faker.Person.Email),
                defaultAdvertisementContactInfoPhoneNumber: PhoneNumber.Create(faker.Person.Phone)
            )).Generate();
    
    [Fact]
    public void DefaultCatPriorityCalculatorService_ShouldReturnExpectedResult_WhenSpecificCombinationIsGiven()
    {
        //Arrange
        DefaultCatPriorityCalculatorService calculator = new DefaultCatPriorityCalculatorService(); 
        
        //Act
        Cat catThatDontNeedThatMuchHelp = Cat.Create(
            priorityScoreCalculator: calculator,
            person: Person,
            name: CatName.Create("Whiskers"),
            medicalHelpUrgency: MedicalHelpUrgency.NoNeed,
            ageCategory: AgeCategory.Baby, 
            behavior: Behavior.Friendly,
            healthStatus: HealthStatus.Good,
            additionalRequirements: Description.Create("Lorem"),
            isCastrated: false
        );
        
        Cat catThatNeedLittleMoreHelp = Cat.Create(
            priorityScoreCalculator: calculator,
            person: Person,
            name: CatName.Create("Cutie"),
            medicalHelpUrgency: MedicalHelpUrgency.NoNeed,
            ageCategory: AgeCategory.Senior, 
            behavior: Behavior.Friendly,
            healthStatus: HealthStatus.Good,
            additionalRequirements: Description.Create("Lorem"),
            isCastrated: false
        );
        
        Cat catThatNeedMuchHelp = Cat.Create(
            priorityScoreCalculator: calculator,
            person: Person,
            name: CatName.Create("Patootie"),
            medicalHelpUrgency: MedicalHelpUrgency.HaveToSeeVet,
            ageCategory: AgeCategory.Senior, 
            behavior: Behavior.Unfriendly,
            healthStatus: HealthStatus.Critical,
            additionalRequirements: Description.Create("Lorem"),
            isCastrated: false
        );

        //Assert
        catThatNeedLittleMoreHelp.PriorityScore.Should().BeGreaterThan(catThatDontNeedThatMuchHelp.PriorityScore);
        catThatNeedMuchHelp.PriorityScore.Should().BeGreaterThan(catThatNeedLittleMoreHelp.PriorityScore);
    }
}
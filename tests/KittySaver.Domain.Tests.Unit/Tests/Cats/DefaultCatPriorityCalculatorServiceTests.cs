using Bogus;
using FluentAssertions;
using KittySaver.Domain.Persons;
using KittySaver.Domain.Persons.DomainServices;
using KittySaver.Domain.Persons.Entities;
using KittySaver.Domain.Persons.ValueObjects;
using KittySaver.Domain.ValueObjects;
using KittySaver.Shared.Common.Enums;
using Person = KittySaver.Domain.Persons.Entities.Person;

namespace KittySaver.Domain.Tests.Unit.Tests.Cats;

public class DefaultCatPriorityCalculatorServiceTests
{
    private static readonly Address PickupAddress = new Faker<Address>()
        .CustomInstantiator(faker =>
            Address.Create(
                country: faker.Address.CountryCode(),
                state: faker.Address.State(),
                zipCode: faker.Address.ZipCode(),
                city: faker.Address.City(),
                street: faker.Address.StreetName(),
                buildingNumber: faker.Address.BuildingNumber()
            )).Generate();
    
    private static readonly Person Person = new Faker<Person>()
        .CustomInstantiator(faker =>
            Person.Create(
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
        Cat catThatDontNeedThatMuchHelp = Person.AddCat(
            priorityScoreCalculator: calculator,
            name: CatName.Create("Whiskers"),
            medicalHelpUrgency: MedicalHelpUrgency.NoNeed,
            ageCategory: AgeCategory.Baby, 
            behavior: Behavior.Friendly,
            healthStatus: HealthStatus.Unknown,
            additionalRequirements: Description.Create("Lorem"),
            isCastrated: false
        );
        
        Cat catThatNeedLittleMoreHelp = Person.AddCat(
            priorityScoreCalculator: calculator,
            name: CatName.Create("Cutie"),
            medicalHelpUrgency: MedicalHelpUrgency.NoNeed,
            ageCategory: AgeCategory.Senior, 
            behavior: Behavior.Friendly,
            healthStatus: HealthStatus.Good,
            additionalRequirements: Description.Create("Lorem"),
            isCastrated: false
        );
        
        Cat catThatNeedMuchHelp = Person.AddCat(
            priorityScoreCalculator: calculator,
            name: CatName.Create("Patootie"),
            medicalHelpUrgency: MedicalHelpUrgency.HaveToSeeVet,
            ageCategory: AgeCategory.Senior, 
            behavior: Behavior.Unfriendly,
            healthStatus: HealthStatus.Terminal,
            additionalRequirements: Description.Create("Lorem"),
            isCastrated: false
        );

        //Assert
        catThatNeedLittleMoreHelp.PriorityScore.Should().BeGreaterThan(catThatDontNeedThatMuchHelp.PriorityScore);
        catThatNeedMuchHelp.PriorityScore.Should().BeGreaterThan(catThatNeedLittleMoreHelp.PriorityScore);
    }
}
using Bogus;
using FluentAssertions;
using KittySaver.Domain.Common.Primitives.Enums;
using KittySaver.Domain.Persons;
using KittySaver.Domain.ValueObjects;
using NSubstitute;
using Person = KittySaver.Domain.Persons.Person;

namespace KittySaver.Domain.Tests.Unit.Cats;

public class CatTests
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
                firstName: FirstName.Create(faker.Person.FirstName),
                lastName: LastName.Create(faker.Person.LastName),
                email: Email.Create(faker.Person.Email),
                phoneNumber: PhoneNumber.Create(faker.Person.Phone),
                residentalAddress: Address,
                defaultAdvertisementPickupAddress: PickupAddress,
                defaultAdvertisementContactInfoEmail: Email.Create(faker.Person.Email),
                defaultAdvertisementContactInfoPhoneNumber: PhoneNumber.Create(faker.Person.Phone)
            )).Generate();
    
    [Fact]
    public void CreateCat_ShouldReturnProperCat_WhenValidDataIsProvided()
    {
        //Arrange
        ICatPriorityCalculatorService? calculator = Substitute.For<ICatPriorityCalculatorService>();
        calculator.Calculate(Arg.Any<Cat>()).Returns(420);
        
        CatName name = CatName.Create("Whiskers");
        Description additionalRequirements = Description.Create("Lorem ipsum");
        MedicalHelpUrgency medicalHelpUrgency = MedicalHelpUrgency.NoNeed;
        AgeCategory ageCategory = AgeCategory.Adult;
        Behavior behavior = Behavior.Friendly;
        HealthStatus healthStatus = HealthStatus.Good;
        const bool isCastrated = true;
        
        //Act
        Cat cat = Cat.Create(
            priorityScoreCalculator: calculator,
            person: Person,
            name: name,
            medicalHelpUrgency: medicalHelpUrgency,
            ageCategory: ageCategory,
            behavior: behavior,
            healthStatus: healthStatus,
            additionalRequirements: additionalRequirements,
            isCastrated: isCastrated
        );

        //Assert
        cat.Should().NotBeNull();
        cat.Id.Should().NotBeEmpty();
        cat.Name.Should().Be(name);
        cat.MedicalHelpUrgency.Should().Be(medicalHelpUrgency);
        cat.AgeCategory.Should().Be(ageCategory);
        cat.Behavior.Should().Be(behavior);
        cat.HealthStatus.Should().Be(healthStatus);
        cat.PriorityScore.Should().Be(420);
        cat.IsCastrated.Should().Be(isCastrated);
        cat.AdditionalRequirements.Should().Be(additionalRequirements);
        cat.IsAdopted.Should().BeFalse();
        cat.PersonId.Should().Be(Person.Id);
    }
    
    [Fact]
    public void PersonIdSet_ShouldThrowArgumentException_WhenProvidedEmptyValue()
    {
        Person person = new Faker<Person>()
        .CustomInstantiator(faker =>
            Person.Create(
                userIdentityId: Guid.NewGuid(),
                firstName: FirstName.Create(faker.Person.FirstName),
                lastName: LastName.Create(faker.Person.LastName),
                email: Email.Create(faker.Person.Email),
                phoneNumber: PhoneNumber.Create(faker.Person.Phone),
                residentalAddress: Address,
                defaultAdvertisementPickupAddress: PickupAddress,
                defaultAdvertisementContactInfoEmail: Email.Create(faker.Person.Email),
                defaultAdvertisementContactInfoPhoneNumber: PhoneNumber.Create(faker.Person.Phone)
            )).Generate();
    
        SharedHelper.SetBackingField(person, nameof(Person.Id), Guid.Empty);
        
        //Act
        Action createCat = () =>
        {
            new Faker<Cat>()
             .CustomInstantiator(faker => Cat.Create(
                     priorityScoreCalculator: Substitute.For<ICatPriorityCalculatorService>(),
                     person: Person,
                     name: CatName.Create(faker.Person.FirstName),
                     additionalRequirements: Description.Create(faker.Person.FirstName),
                     medicalHelpUrgency: faker.PickRandomParam(MedicalHelpUrgency.NoNeed, MedicalHelpUrgency.ShouldSeeVet, MedicalHelpUrgency.HaveToSeeVet),
                     ageCategory: faker.PickRandomParam(AgeCategory.Baby, AgeCategory.Adult, AgeCategory.Senior),
                     behavior: faker.PickRandomParam(Behavior.Unfriendly, Behavior.Friendly),
                     healthStatus: faker.PickRandomParam(HealthStatus.Critical, HealthStatus.Poor, HealthStatus.Good),
                     isCastrated: faker.PickRandomParam(true, false)
                 )).Generate();
        };
        
        //Assert
        createCat.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public void ReCalculatePriorityScore_ShouldUpdatePriorityScore_WhenCalculatorIsProvided()
    {
        //Arrange
        const int score = 420;
        ICatPriorityCalculatorService calculator = Substitute.For<ICatPriorityCalculatorService>();
        calculator.Calculate(Arg.Any<Cat>()).Returns(score);
    
        Cat cat = new Faker<Cat>()
            .CustomInstantiator(faker =>
                Cat.Create(
                    priorityScoreCalculator: calculator,
                    person: Person,
                    name: CatName.Create(faker.Person.FirstName),
                    additionalRequirements: Description.Create(faker.Person.FirstName),
                    medicalHelpUrgency: faker.PickRandomParam(MedicalHelpUrgency.NoNeed, MedicalHelpUrgency.ShouldSeeVet, MedicalHelpUrgency.HaveToSeeVet),
                    ageCategory: faker.PickRandomParam(AgeCategory.Baby, AgeCategory.Adult, AgeCategory.Senior),
                    behavior: faker.PickRandomParam(Behavior.Unfriendly, Behavior.Friendly),
                    healthStatus: faker.PickRandomParam(HealthStatus.Critical, HealthStatus.Poor, HealthStatus.Good),
                    isCastrated: faker.PickRandomParam(true, false)
                )).Generate();
        calculator.Calculate(Arg.Any<Cat>()).Returns(score * 2);
        
        //Act
        cat.RecalculatePriorityScore(calculator);

        //Assert
        cat.PriorityScore.Should().Be(score * 2);
    }
}
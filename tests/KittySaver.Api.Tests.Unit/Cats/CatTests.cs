using Bogus;
using FluentAssertions;
using KittySaver.Api.Shared.Domain.Entites;
using KittySaver.Api.Shared.Domain.Enums;
using KittySaver.Api.Shared.Domain.Services;
using KittySaver.Api.Shared.Domain.ValueObjects;
using NSubstitute;
using Person = KittySaver.Api.Shared.Domain.Entites.Person;

namespace KittySaver.Api.Tests.Unit.Cats;

public class CatTests
{
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
    
    private static readonly PickupAddress PickupAddress = new Faker<PickupAddress>()
        .CustomInstantiator(faker =>
            new PickupAddress
            {
                Country = faker.Address.Country(),
                State = faker.Address.State(),
                ZipCode = faker.Address.ZipCode(),
                City = faker.Address.City(),
                Street = faker.Address.StreetName(),
                BuildingNumber = faker.Address.BuildingNumber()
            }).Generate();
    
    private static readonly ContactInfo ContactInfo = new Faker<ContactInfo>()
        .CustomInstantiator(faker =>
            new ContactInfo
            {
                Email = faker.Person.Email,
                PhoneNumber = faker.Person.Phone
            }).Generate();
    
    private static readonly Person Person = new Faker<Person>()
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
    
    [Fact]
    public void CreateCat_ShouldReturnProperCat_WhenValidDataIsProvided()
    {
        //Arrange
        ICatPriorityCalculator? calculator = Substitute.For<ICatPriorityCalculator>();
        calculator.Calculate(Arg.Any<Cat>()).Returns(420);
        const string name = "Whiskers";
        MedicalHelpUrgency medicalHelpUrgency = MedicalHelpUrgency.NoNeed;
        AgeCategory ageCategory = AgeCategory.Adult;
        Behavior behavior = Behavior.Friendly;
        HealthStatus healthStatus = HealthStatus.Good;
        const bool isCastrated = true;
        const bool isInNeedOfSeeingVet = true;
        const string additionalRequirements = "Lorem ipsum";
        
        //Act
        Cat cat = Cat.Create(
            calculator: calculator,
            personId: Person.Id,
            name: name,
            medicalHelpUrgency: medicalHelpUrgency,
            ageCategory: ageCategory,
            behavior: behavior,
            healthStatus: healthStatus,
            isCastrated: isCastrated,
            isInNeedOfSeeingVet: isInNeedOfSeeingVet,
            additionalRequirements: additionalRequirements
        );
        typeof(Cat).GetProperty(nameof(Cat.Person))?.SetValue(cat, Person, null);

        //Assert
        cat.Should().NotBeNull();
        cat.Id.Should().NotBeEmpty();
        cat.Name.Should().Be(name);
        cat.PersonId.Should().Be(Person.Id);
        cat.MedicalHelpUrgency.Should().Be(medicalHelpUrgency);
        cat.AgeCategory.Should().Be(ageCategory);
        cat.Behavior.Should().Be(behavior);
        cat.HealthStatus.Should().Be(healthStatus);
        cat.PriorityScore.Should().Be(420);
        cat.IsCastrated.Should().Be(isCastrated);
        cat.IsInNeedOfSeeingVet.Should().Be(isInNeedOfSeeingVet);
        cat.AdditionalRequirements.Should().Be(additionalRequirements);
        cat.Person.Should().BeEquivalentTo(Person);
    }
    
    [Fact]
    public void NameSet_ShouldThrowArgumentOutOfRangeException_WhenNameExceedsMaxLength()
    {
        //Arrange
        string longName = new('C', Cat.Constraints.NameMaxLength + 1);

        //Act
        Action createCat = () =>
        {
            Cat cat = Cat.Create(
                Substitute.For<ICatPriorityCalculator>(),
                Person.Id,
                longName,
                MedicalHelpUrgency.NoNeed,
                AgeCategory.Adult,
                Behavior.Unfriendly,
                HealthStatus.Poor
            );
            typeof(Cat).GetProperty(nameof(Cat.Person))?.SetValue(cat, Person, null);
        };

        //Assert
        createCat.Should().Throw<ArgumentOutOfRangeException>();
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void NameSet_ShouldThrowArgumentException_WhenNameIsNullOrWhitespace(string invalidName)
    {
        //Act
        Action createCat = () =>
        {
            Cat cat = Cat.Create(
                Substitute.For<ICatPriorityCalculator>(),
                Person.Id,
                invalidName,
                MedicalHelpUrgency.HaveToSeeVet,
                AgeCategory.Senior,
                Behavior.Friendly,
                HealthStatus.Critical
            );
            typeof(Cat).GetProperty(nameof(Cat.Person))?.SetValue(cat, Person, null);
        };

        //Assert
        createCat.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AdditionalRequirementsSet_ShouldThrowArgumentOutOfRangeException_WhenExceedsMaxLength()
    {
        //Arrange
        string longRequirements = new string('R', Cat.Constraints.AdditionalRequirementsMaxLength + 1);

        //Act
        Action createCat = () =>
        {
            Cat cat = Cat.Create(
                Substitute.For<ICatPriorityCalculator>(),
                Person.Id,
                "Whiskers",
                MedicalHelpUrgency.HaveToSeeVet,
                AgeCategory.Senior,
                Behavior.Friendly,
                HealthStatus.Critical,
                additionalRequirements: longRequirements
            );
            typeof(Cat).GetProperty(nameof(Cat.Person))?.SetValue(cat, Person, null);
        };
        
        //Assert
        createCat.Should().Throw<ArgumentOutOfRangeException>();
    }
    
    [Fact]
    public void ReCalculatePriorityScore_ShouldUpdatePriorityScore_WhenCalculatorIsProvided()
    {
        //Arrange
        const int score = 420;
        ICatPriorityCalculator? calculator = Substitute.For<ICatPriorityCalculator>();
        calculator.Calculate(Arg.Any<Cat>()).Returns(score);
    
        Cat cat = Cat.Create(
            calculator,
            Person.Id,
            "Whiskers",
            MedicalHelpUrgency.HaveToSeeVet,
            AgeCategory.Senior,
            Behavior.Friendly,
            HealthStatus.Critical
        );
        typeof(Cat).GetProperty(nameof(Cat.Person))?.SetValue(cat, Person, null);
        
        //Act
        cat.ReCalculatePriorityScore(calculator);

        //Assert
        cat.PriorityScore.Should().Be(score);
    }
    
    [Fact]
    public void Create_ShouldThrowArgumentException_WhenPersonIdIsEmpty()
    {
        //Arrange
        Guid emptyPersonId = Guid.Empty;
        ICatPriorityCalculator calculator = Substitute.For<ICatPriorityCalculator>();
        
        //Act
        Action createCat = () =>
        {
            Cat cat = Cat.Create(
                calculator: calculator,
                personId: emptyPersonId,
                name: "Whiskers",
                medicalHelpUrgency: MedicalHelpUrgency.HaveToSeeVet,
                ageCategory: AgeCategory.Senior,
                behavior: Behavior.Friendly,
                healthStatus: HealthStatus.Critical
            );
            typeof(Cat).GetProperty(nameof(Cat.Person))?.SetValue(cat, Person, null);

        };

        //Assert
        createCat.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public void Priority_ShouldReturnExpectedResult_WhenFirstCombinationIsGiven()
    {
        //Act
        Cat catThatDontNeedThatMuchHelp = Cat.Create(
            new DefaultCatPriorityCalculator(),
            Person.Id,
            "Whiskers",
            MedicalHelpUrgency.NoNeed,
            AgeCategory.Baby, 
            Behavior.Friendly,
            HealthStatus.Good
        );
        typeof(Cat).GetProperty(nameof(Cat.Person))?.SetValue(catThatDontNeedThatMuchHelp, Person, null);
        
        Cat catThatNeedLittleMoreHelp = Cat.Create(
            new DefaultCatPriorityCalculator(),
            Person.Id,
            "Cutie",
            MedicalHelpUrgency.NoNeed,
            AgeCategory.Senior, 
            Behavior.Friendly,
            HealthStatus.Good
        );
        typeof(Cat).GetProperty(nameof(Cat.Person))?.SetValue(catThatNeedLittleMoreHelp, Person, null);
        
        Cat catThatNeedMuchHelp = Cat.Create(
            new DefaultCatPriorityCalculator(),
            Person.Id,
            "Kitty",
            MedicalHelpUrgency.HaveToSeeVet,
            AgeCategory.Senior, 
            Behavior.Unfriendly,
            HealthStatus.Critical
        );
        typeof(Cat).GetProperty(nameof(Cat.Person))?.SetValue(catThatNeedMuchHelp, Person, null);

        //Assert
        catThatNeedLittleMoreHelp.PriorityScore.Should().BeGreaterThan(catThatDontNeedThatMuchHelp.PriorityScore);
        catThatNeedMuchHelp.PriorityScore.Should().BeGreaterThan(catThatNeedLittleMoreHelp.PriorityScore);
    }
}
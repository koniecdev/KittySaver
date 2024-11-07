using System.Reflection;
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
            person: Person,
            name: name,
            medicalHelpUrgency: medicalHelpUrgency,
            ageCategory: ageCategory,
            behavior: behavior,
            healthStatus: healthStatus,
            isCastrated: isCastrated,
            isInNeedOfSeeingVet: isInNeedOfSeeingVet,
            additionalRequirements: additionalRequirements
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
        cat.IsInNeedOfSeeingVet.Should().Be(isInNeedOfSeeingVet);
        cat.AdditionalRequirements.Should().Be(additionalRequirements);
        cat.IsAdopted.Should().BeFalse();
        
        cat.PersonId.Should().Be(Person.Id);
    }
    
    [Fact]
    public void NameSet_ShouldThrowArgumentOutOfRangeException_WhenNameExceedsMaxLength()
    {
        //Arrange
        string longName = new('C', Cat.Constraints.NameMaxLength + 1);

        //Act
        Action createCat = () =>
        {
            Cat.Create(
                calculator: Substitute.For<ICatPriorityCalculator>(),
                person: Person,
                name: longName,
                medicalHelpUrgency: MedicalHelpUrgency.NoNeed,
                ageCategory: AgeCategory.Adult,
                behavior: Behavior.Unfriendly,
                healthStatus: HealthStatus.Poor
            );
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
            Cat.Create(
                calculator: Substitute.For<ICatPriorityCalculator>(),
                person: Person,
                name: invalidName,
                medicalHelpUrgency: MedicalHelpUrgency.HaveToSeeVet,
                ageCategory: AgeCategory.Senior,
                behavior: Behavior.Friendly,
                healthStatus: HealthStatus.Critical
            );
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
            Cat.Create(
                calculator: Substitute.For<ICatPriorityCalculator>(),
                person: Person,
                name: "Whiskers",
                medicalHelpUrgency: MedicalHelpUrgency.HaveToSeeVet,
                ageCategory: AgeCategory.Senior,
                behavior: Behavior.Friendly,
                healthStatus: HealthStatus.Critical,
                additionalRequirements: longRequirements
            );
        };
        
        //Assert
        createCat.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void PersonId_ShouldThrowArgumentException_WhenProvidedEmptyValue()
    {
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
        SharedHelper.SetBackingField(person, nameof(Person.Id), Guid.Empty);
        
        //Act
        Action createCat = () =>
        {
            Cat.Create(
                calculator: Substitute.For<ICatPriorityCalculator>(),
                person: person,
                name: "Whiskers",
                medicalHelpUrgency: MedicalHelpUrgency.HaveToSeeVet,
                ageCategory: AgeCategory.Senior,
                behavior: Behavior.Friendly,
                healthStatus: HealthStatus.Critical
            );
        };
        
        //Assert
        createCat.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public void ReCalculatePriorityScore_ShouldUpdatePriorityScore_WhenCalculatorIsProvided()
    {
        //Arrange
        const int score = 420;
        ICatPriorityCalculator? calculator = Substitute.For<ICatPriorityCalculator>();
        calculator.Calculate(Arg.Any<Cat>()).Returns(score);
    
        Cat cat = Cat.Create(
            calculator: calculator,
            person: Person,
            name: "Whiskers",
            medicalHelpUrgency: MedicalHelpUrgency.HaveToSeeVet,
            ageCategory: AgeCategory.Senior,
            behavior: Behavior.Friendly,
            healthStatus: HealthStatus.Critical
        );
        
        //Act
        cat.ReCalculatePriorityScore(calculator);

        //Assert
        cat.PriorityScore.Should().Be(score);
    }
    
    [Fact]
    public void Priority_ShouldReturnExpectedResult_WhenFirstCombinationIsGiven()
    {
        //Act
        Cat catThatDontNeedThatMuchHelp = Cat.Create(
            calculator: new DefaultCatPriorityCalculator(),
            person: Person,
            name: "Whiskers",
            medicalHelpUrgency: MedicalHelpUrgency.NoNeed,
            ageCategory: AgeCategory.Baby, 
            behavior: Behavior.Friendly,
            healthStatus: HealthStatus.Good
        );
        
        Cat catThatNeedLittleMoreHelp = Cat.Create(
            calculator: new DefaultCatPriorityCalculator(),
            person: Person,
            name: "Cutie",
            medicalHelpUrgency: MedicalHelpUrgency.NoNeed,
            ageCategory: AgeCategory.Senior, 
            behavior: Behavior.Friendly,
            healthStatus: HealthStatus.Good
        );
        
        Cat catThatNeedMuchHelp = Cat.Create(
            calculator: new DefaultCatPriorityCalculator(),
            person: Person,
            name: "Kitty",
            medicalHelpUrgency: MedicalHelpUrgency.HaveToSeeVet,
            ageCategory: AgeCategory.Senior, 
            behavior: Behavior.Unfriendly,
            healthStatus: HealthStatus.Critical
        );

        //Assert
        catThatNeedLittleMoreHelp.PriorityScore.Should().BeGreaterThan(catThatDontNeedThatMuchHelp.PriorityScore);
        catThatNeedMuchHelp.PriorityScore.Should().BeGreaterThan(catThatNeedLittleMoreHelp.PriorityScore);
    }
    
    [Fact]
    public void AssignAdvertisement_ShouldThrowArgumentException_WhenThereIsAlreadyAdvertisementAssigned()
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

        ICatPriorityCalculator calculator = Substitute.For<ICatPriorityCalculator>();
        calculator.Calculate(Arg.Any<Cat>()).Returns(420);
        Cat cat = Cat.Create(
            calculator: calculator,
            person: person,
            name: "Whiskers",
            medicalHelpUrgency: MedicalHelpUrgency.HaveToSeeVet,
            ageCategory: AgeCategory.Senior,
            behavior: Behavior.Friendly,
            healthStatus: HealthStatus.Critical
        );
        
        Advertisement advertisement = Advertisement.Create(
            currentDate: new DateTimeOffset(2024, 10, 31, 11, 0, 0, TimeSpan.Zero),
            person: person,
            catsIdsToAssign: [cat.Id],
            pickupAddress: PickupAddress,
            contactInfo: ContactInfo,
            description: "lorem ipsum");

        //Act
        SharedHelper.SetBackingField(advertisement, nameof(Advertisement.Id), Guid.Empty);
        Action assignment = () => cat.AssignAdvertisement(advertisement.Id);
        
        //Assert
        assignment.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkAsAdopted_ShouldSuccess_WhenProvidedValidData()
    {
        //Arrange
        Cat cat = Cat.Create(
            calculator: Substitute.For<ICatPriorityCalculator>(),
            person: Person,
            name: "Whiskers",
            medicalHelpUrgency: MedicalHelpUrgency.HaveToSeeVet,
            ageCategory: AgeCategory.Senior,
            behavior: Behavior.Friendly,
            healthStatus: HealthStatus.Critical
        );
        
        //Act
        cat.MarkAsAdopted();
        
        //Assert
        cat.IsAdopted.Should().BeTrue();
    }
}
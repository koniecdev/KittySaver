using Bogus;
using FluentAssertions;
using KittySaver.Domain.Persons.DomainServices;
using KittySaver.Domain.Persons.Entities;
using KittySaver.Domain.Persons.ValueObjects;
using KittySaver.Domain.ValueObjects;
using KittySaver.Shared.Common.Enums;
using KittySaver.Shared.TypedIds;
using NSubstitute;
using Person = KittySaver.Domain.Persons.Entities.Person;

namespace KittySaver.Domain.Tests.Unit.Tests.Cats;

public class CatTests
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
        Cat cat = Person.AddCat(
            priorityScoreCalculator: calculator,
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
    public void UnassignAdvertisement_ShouldBeSuccessfull_WhenValidDataAreProvided()
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
        
        Cat sut = Person.AddCat(
            priorityScoreCalculator: calculator,
            name: name,
            medicalHelpUrgency: medicalHelpUrgency,
            ageCategory: ageCategory,
            behavior: behavior,
            healthStatus: healthStatus,
            additionalRequirements: additionalRequirements,
            isCastrated: isCastrated
        );
        Person.AddAdvertisement(
            dateOfCreation: new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
            catsIdsToAssign: [sut.Id],
            pickupAddress: Person.DefaultAdvertisementsPickupAddress,
            contactInfoEmail: Person.DefaultAdvertisementsContactInfoEmail,
            contactInfoPhoneNumber: Person.DefaultAdvertisementsContactInfoPhoneNumber,
            description: Description.Create("lorem ipsum"));
        
        //Act
        sut.RemoveFromAdvertisement();
        
        //Assert
        sut.AdvertisementId.Should().BeNull();
    }
    
    [Fact]
    public void UnassignAdvertisement_ShouldThrowInvalidOperationException_WhenCatIsNotAssignedToAnyAdvertisement()
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
        
        Cat cat = Person.AddCat(
            priorityScoreCalculator: calculator,
            name: name,
            medicalHelpUrgency: medicalHelpUrgency,
            ageCategory: ageCategory,
            behavior: behavior,
            healthStatus: healthStatus,
            additionalRequirements: additionalRequirements,
            isCastrated: isCastrated
        );
        
        //Act
        Action results = () => cat.RemoveFromAdvertisement();
        
        //Assert
        results.Should().ThrowExactly<InvalidOperationException>();
    }
    
    [Fact]
    public void PriorityScoreSet_ShouldThrowArgumentException_WhenZeroIsProvided()
    {
        //Arrange
        ICatPriorityCalculatorService? calculator = Substitute.For<ICatPriorityCalculatorService>();
        calculator.Calculate(Arg.Any<Cat>()).Returns(0);
        
        CatName name = CatName.Create("Whiskers");
        Description additionalRequirements = Description.Create("Lorem ipsum");
        MedicalHelpUrgency medicalHelpUrgency = MedicalHelpUrgency.NoNeed;
        AgeCategory ageCategory = AgeCategory.Adult;
        Behavior behavior = Behavior.Friendly;
        HealthStatus healthStatus = HealthStatus.Good;
        const bool isCastrated = true;
        
        //Act
        Action results = () => Person.AddCat(
            priorityScoreCalculator: calculator,
            name: name,
            medicalHelpUrgency: medicalHelpUrgency,
            ageCategory: ageCategory,
            behavior: behavior,
            healthStatus: healthStatus,
            additionalRequirements: additionalRequirements,
            isCastrated: isCastrated
        );
        
        //Assert
        results.Should().ThrowExactly<ArgumentException>().WithMessage("PriorityScore cannot be zero, probably something went wrong. (Parameter 'PriorityScore')");
    }
    
    [Fact]
    public void MarkAsAdopted_ShouldSetIsAdoptedToTrue()
    {
        // Arrange
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
        Cat cat = Person.AddCat(
            priorityScoreCalculator: calculator,
            name: name,
            medicalHelpUrgency: medicalHelpUrgency,
            ageCategory: ageCategory,
            behavior: behavior,
            healthStatus: healthStatus,
            additionalRequirements: additionalRequirements,
            isCastrated: isCastrated
        );

        // Act
        cat.MarkAsAdopted();

        // Assert
        cat.IsAdopted.Should().BeTrue();
    }
    
    [Fact]
    public void AssignAdvertisement_WhenCatNotAssignedToAnyAdvertisement_ShouldAssignAdvertisementId()
    {
        // Arrange
        ICatPriorityCalculatorService? calculator = Substitute.For<ICatPriorityCalculatorService>();
        calculator.Calculate(Arg.Any<Cat>()).Returns(420);
        
        CatName name = CatName.Create("Whiskers");
        Description additionalRequirements = Description.Create("Lorem ipsum");
        MedicalHelpUrgency medicalHelpUrgency = MedicalHelpUrgency.NoNeed;
        AgeCategory ageCategory = AgeCategory.Adult;
        Behavior behavior = Behavior.Friendly;
        HealthStatus healthStatus = HealthStatus.Good;
        const bool isCastrated = true;
        
        Cat cat = Person.AddCat(
            priorityScoreCalculator: calculator,
            name: name,
            medicalHelpUrgency: medicalHelpUrgency,
            ageCategory: ageCategory,
            behavior: behavior,
            healthStatus: healthStatus,
            additionalRequirements: additionalRequirements,
            isCastrated: isCastrated
        );
        AdvertisementId advertisementId = AdvertisementId.New();

        // Act
        cat.AssignAdvertisement(advertisementId);

        // Assert
        advertisementId.Should().Be(cat.AdvertisementId!.Value);
    }

    [Fact]
    public void AssignAdvertisement_WhenCatAlreadyAssignedToAdvertisement_ShouldThrowInvalidOperationException()
    {
        // Arrange
        ICatPriorityCalculatorService? calculator = Substitute.For<ICatPriorityCalculatorService>();
        calculator.Calculate(Arg.Any<Cat>()).Returns(420);
        
        CatName name = CatName.Create("Whiskers");
        Description additionalRequirements = Description.Create("Lorem ipsum");
        MedicalHelpUrgency medicalHelpUrgency = MedicalHelpUrgency.NoNeed;
        AgeCategory ageCategory = AgeCategory.Adult;
        Behavior behavior = Behavior.Friendly;
        HealthStatus healthStatus = HealthStatus.Good;
        const bool isCastrated = true;
        
        Cat cat = Person.AddCat(
            priorityScoreCalculator: calculator,
            name: name,
            medicalHelpUrgency: medicalHelpUrgency,
            ageCategory: ageCategory,
            behavior: behavior,
            healthStatus: healthStatus,
            additionalRequirements: additionalRequirements,
            isCastrated: isCastrated
        );
        AdvertisementId firstAdvertisementId = AdvertisementId.New();
        cat.AssignAdvertisement(firstAdvertisementId);

        // Act
        Action action = () => cat.AssignAdvertisement(AdvertisementId.New()); 
        
        //Assert
        action.Should().Throw<InvalidOperationException>();
    }
}
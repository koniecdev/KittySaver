using Bogus;
using FluentAssertions;
using KittySaver.Domain.Advertisements;
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
                nickname: Nickname.Create(faker.Person.FirstName),
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
                nickname: Nickname.Create(faker.Person.FirstName),
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
                     person: person,
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
        createCat.Should().Throw<ArgumentException>().WithMessage("Provided person id is empty (Parameter 'PersonId')");
    }
    
    [Fact]
    public void AdvertisementIdSet_ShouldThrowArgumentException_WhenProvidedEmptyValue()
    {
        Person person = new Faker<Person>()
        .CustomInstantiator(faker =>
            Person.Create(
                userIdentityId: Guid.NewGuid(),
                nickname: Nickname.Create(faker.Person.FirstName),
                email: Email.Create(faker.Person.Email),
                phoneNumber: PhoneNumber.Create(faker.Person.Phone),
                residentalAddress: Address,
                defaultAdvertisementPickupAddress: PickupAddress,
                defaultAdvertisementContactInfoEmail: Email.Create(faker.Person.Email),
                defaultAdvertisementContactInfoPhoneNumber: PhoneNumber.Create(faker.Person.Phone)
            )).Generate();
        ICatPriorityCalculatorService priorityScoreCalculator = Substitute.For<ICatPriorityCalculatorService>();
        priorityScoreCalculator.Calculate(Arg.Any<Cat>()).Returns(420);
        Cat cat = new Faker<Cat>()
         .CustomInstantiator(faker => Cat.Create(
                 priorityScoreCalculator: priorityScoreCalculator,
                 person: person,
                 name: CatName.Create(faker.Person.FirstName),
                 additionalRequirements: Description.Create(faker.Person.FirstName),
                 medicalHelpUrgency: faker.PickRandomParam(MedicalHelpUrgency.NoNeed, MedicalHelpUrgency.ShouldSeeVet, MedicalHelpUrgency.HaveToSeeVet),
                 ageCategory: faker.PickRandomParam(AgeCategory.Baby, AgeCategory.Adult, AgeCategory.Senior),
                 behavior: faker.PickRandomParam(Behavior.Unfriendly, Behavior.Friendly),
                 healthStatus: faker.PickRandomParam(HealthStatus.Critical, HealthStatus.Poor, HealthStatus.Good),
                 isCastrated: faker.PickRandomParam(true, false)
             )).Generate();
        
        //Act
        Action results = () =>
        {
            SharedHelper.SetPrivateSetProperty(cat, nameof(Cat.AdvertisementId), Guid.Empty);
        };
        
        //Assert
        results.Should().Throw<Exception>().WithInnerException<ArgumentException>().WithMessage("Provided advertisement id is empty (Parameter 'AdvertisementId')");
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
        
        Cat sut = Cat.Create(
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
        Advertisement.Create(
            currentDate: new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
            person: Person,
            catsIdsToAssign: [sut.Id],
            pickupAddress: Person.DefaultAdvertisementsPickupAddress,
            contactInfoEmail: Person.DefaultAdvertisementsContactInfoEmail,
            contactInfoPhoneNumber: Person.DefaultAdvertisementsContactInfoPhoneNumber,
            description: Description.Create("lorem ipsum"));
        
        //Act
        sut.UnassignAdvertisement();
        
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
        
        //Act
        Action results = () => cat.UnassignAdvertisement();
        
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
        Action results = () => Cat.Create(
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
        results.Should().ThrowExactly<ArgumentException>().WithMessage("PriorityScore cannot be zero, probably something went wrong. (Parameter 'PriorityScore')");
    }
}
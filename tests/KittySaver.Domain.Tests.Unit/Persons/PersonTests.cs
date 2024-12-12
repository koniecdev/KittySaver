using Bogus;
using FluentAssertions;
using KittySaver.Domain.Advertisements;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Common.Primitives;
using KittySaver.Domain.Common.Primitives.Enums;
using KittySaver.Domain.Persons;
using KittySaver.Domain.ValueObjects;
using NSubstitute;
using Person = KittySaver.Domain.Persons.Person;

namespace KittySaver.Domain.Tests.Unit.Persons;

public class PersonTests
{
    private readonly Guid _userIdentityId = Guid.NewGuid();
    private readonly Nickname _defaultProperNickname = Nickname.Create("Artur");
    private readonly Email _defaultProperEmail = Email.Create("fake@fake.fake");
    private readonly PhoneNumber _defaultProperPhone = PhoneNumber.Create("+48111222333");

    private static readonly Address PickupAddress = new Faker<Address>()
        .CustomInstantiator(faker =>
            Address.Create(
                faker.Address.Country(),
                faker.Address.State(),
                faker.Address.ZipCode(),
                faker.Address.City(),
                faker.Address.StreetName(),
                faker.Address.BuildingNumber()
            )).Generate();
    
    [Fact]
    public void PersonGetters_ShouldReturnProperValues_WhenPersonIsInstantiated()
    {
        //Arrange
        Nickname nickname = Nickname.Create("Artur");
        Email email = Email.Create("koniecdev@gmail.com");
        PhoneNumber phoneNumber = PhoneNumber.Create("535143330");
        Email defaultEmail = Email.Create("koniecdevcontact@gmail.com");
        PhoneNumber defaultPhoneNumber = PhoneNumber.Create("5351433300");
        
        //Act
        Person sut = Person.Create(
            userIdentityId: _userIdentityId,
            nickname: nickname,
            email: email,
            phoneNumber: phoneNumber,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfoEmail: defaultEmail,
            defaultAdvertisementContactInfoPhoneNumber: defaultPhoneNumber
        );

        //Assert
        sut.Id.Should().NotBeEmpty();
        sut.Nickname.Should().Be(nickname);
        sut.Email.Should().Be(email);
        sut.PhoneNumber.Should().Be(phoneNumber);
        sut.DefaultAdvertisementsPickupAddress.Should().BeEquivalentTo(PickupAddress);
        sut.DefaultAdvertisementsContactInfoEmail.Should().BeEquivalentTo(defaultEmail);
        sut.DefaultAdvertisementsContactInfoPhoneNumber.Should().BeEquivalentTo(defaultPhoneNumber);
        sut.UserIdentityId.Should().Be(_userIdentityId);
        sut.CurrentRole.Should().Be(Person.Role.Regular);
    }
    
    [Fact]
    public void PersonGetters_ShouldReturnProperValues_WhenPersonIsInstantiatedWithJustRequiredProperties()
    {
        //Arrange
        Nickname nickname = Nickname.Create("Artur");
        Email email = Email.Create("koniecdev@gmail.com");
        PhoneNumber phoneNumber = PhoneNumber.Create("535143330");
        Email defaultEmail = Email.Create("koniecdevcontact@gmail.com");
        PhoneNumber defaultPhoneNumber = PhoneNumber.Create("5351433300");

        Address pickupAddress = new Faker<Address>()
        .CustomInstantiator(faker =>
            Address.Create(
                country: faker.Address.Country(),
                state: "",
                zipCode: faker.Address.ZipCode(),
                city: faker.Address.City(),
                street: faker.Address.StreetName(),
                buildingNumber: faker.Address.BuildingNumber()
            )).Generate();
        
        //Act
        Person sut = Person.Create(
            userIdentityId: _userIdentityId,
            nickname: nickname,
            email: email,
            phoneNumber: phoneNumber,
            defaultAdvertisementPickupAddress: pickupAddress,
            defaultAdvertisementContactInfoEmail: defaultEmail,
            defaultAdvertisementContactInfoPhoneNumber: defaultPhoneNumber
        );

        //Assert
        sut.Id.Should().NotBeEmpty();
        sut.Nickname.Should().Be(nickname);
        sut.Email.Should().Be(email);
        sut.PhoneNumber.Should().Be(phoneNumber);
        sut.DefaultAdvertisementsPickupAddress.Should().NotBeNull();
        sut.DefaultAdvertisementsPickupAddress.Country.Should().Be(pickupAddress.Country);
        sut.DefaultAdvertisementsPickupAddress.State.Should().BeNull();
        sut.DefaultAdvertisementsPickupAddress.ZipCode.Should().Be(pickupAddress.ZipCode);
        sut.DefaultAdvertisementsPickupAddress.City.Should().Be(pickupAddress.City);
        sut.DefaultAdvertisementsPickupAddress.Street.Should().Be(pickupAddress.Street);
        sut.DefaultAdvertisementsPickupAddress.BuildingNumber.Should().Be(pickupAddress.BuildingNumber);
        sut.DefaultAdvertisementsContactInfoEmail.Should().BeEquivalentTo(defaultEmail);
        sut.DefaultAdvertisementsContactInfoPhoneNumber.Should().BeEquivalentTo(defaultPhoneNumber);
        sut.UserIdentityId.Should().Be(_userIdentityId);
        sut.CurrentRole.Should().Be(Person.Role.Regular);
    }
    
    [Fact]
    public void UserIdentityIdSet_ShouldThrowArgumentException_WhenEmptyGuidIsProvided()
    {
        //Arrange
        Guid emptyGuid = Guid.Empty;

        //Act
        Action creation = () => Person.Create(
            userIdentityId: emptyGuid,
            nickname: _defaultProperNickname,
            email: _defaultProperEmail,
            phoneNumber: _defaultProperPhone,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfoEmail: _defaultProperEmail,
            defaultAdvertisementContactInfoPhoneNumber: _defaultProperPhone
        );

        //Assert
        creation.Should().Throw<ArgumentException>()
            .WithMessage("Provided empty guid. (Parameter 'UserIdentityId')");
    }
    
    [Fact]
    public void AddCat_ShouldAddCatToList_WhenValidCatIsProvided()
    {
        //Arrange
        Person sut = Person.Create(
            userIdentityId: _userIdentityId,
            nickname: _defaultProperNickname,
            email: _defaultProperEmail,
            phoneNumber: _defaultProperPhone,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfoEmail: _defaultProperEmail,
            defaultAdvertisementContactInfoPhoneNumber: _defaultProperPhone
        );
        ICatPriorityCalculatorService calculatorService = Substitute.For<ICatPriorityCalculatorService>();
        calculatorService.Calculate(Arg.Any<Cat>()).ReturnsForAnyArgs(420);
        
        //Act
        Cat cat = new Faker<Cat>()
            .CustomInstantiator( faker =>
                Cat.Create(
                    priorityScoreCalculator: calculatorService,
                    person: sut,
                    name: CatName.Create(faker.Person.FirstName), 
                    medicalHelpUrgency: MedicalHelpUrgency.NoNeed,
                    ageCategory: AgeCategory.Baby,
                    behavior: Behavior.Friendly, 
                    healthStatus: HealthStatus.Good,
                    additionalRequirements: Description.Create(faker.Lorem.Lines(2)),
                    isCastrated: false))
            .Generate();
        
        //Assert
        sut.Cats.Should().Contain(cat);
    }
    
    [Fact]
    public void RemoveCat_ShouldRemoveCatFromList_WhenValidCatIsProvided()
    {
        //Arrange
        Person sut = Person.Create(
            userIdentityId: _userIdentityId,
            nickname: _defaultProperNickname,
            email: _defaultProperEmail,
            phoneNumber: _defaultProperPhone,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfoEmail: _defaultProperEmail,
            defaultAdvertisementContactInfoPhoneNumber: _defaultProperPhone
        );
        ICatPriorityCalculatorService calculatorService = Substitute.For<ICatPriorityCalculatorService>();
        calculatorService.Calculate(Arg.Any<Cat>()).ReturnsForAnyArgs(420);
        Cat cat = new Faker<Cat>()
            .CustomInstantiator( faker =>
                Cat.Create(
                    priorityScoreCalculator: calculatorService,
                    person: sut,
                    name: CatName.Create(faker.Person.FirstName), 
                    medicalHelpUrgency: MedicalHelpUrgency.NoNeed,
                    ageCategory: AgeCategory.Baby,
                    behavior: Behavior.Friendly, 
                    healthStatus: HealthStatus.Good,
                    additionalRequirements: Description.Create(faker.Lorem.Lines(2)),
                    isCastrated: false))
            .Generate();
        
        //Act
        sut.RemoveCat(cat.Id);

        //Assert
        sut.Cats.Should().NotContain(cat);
    }

    [Fact]
    public void AddCat_ShouldThrowInvalidOperationException_WhenTheSameCatIsProvided()
    {
        //Arrange
        Person sut = Person.Create(
            userIdentityId: _userIdentityId,
            nickname: _defaultProperNickname,
            email: _defaultProperEmail,
            phoneNumber: _defaultProperPhone,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfoEmail: _defaultProperEmail,
            defaultAdvertisementContactInfoPhoneNumber: _defaultProperPhone
        );
        ICatPriorityCalculatorService calculatorService = Substitute.For<ICatPriorityCalculatorService>();
        calculatorService.Calculate(Arg.Any<Cat>()).ReturnsForAnyArgs(420);
        Cat cat = new Faker<Cat>()
            .CustomInstantiator( faker =>
                Cat.Create(
                    priorityScoreCalculator: calculatorService,
                    person: sut,
                    name: CatName.Create(faker.Person.FirstName), 
                    medicalHelpUrgency: MedicalHelpUrgency.NoNeed,
                    ageCategory: AgeCategory.Baby,
                    behavior: Behavior.Friendly, 
                    healthStatus: HealthStatus.Good,
                    additionalRequirements: Description.Create(faker.Lorem.Lines(2)),
                    isCastrated: false))
            .Generate();
        
        //Act
        Action operation = () => sut.AddCat(cat);
        
        //Assert
        operation.Should().ThrowExactly<InvalidOperationException>();
    }
    
    [Fact]
    public void RemoveCat_ShouldThrowCatNotFoundException_WhenTheSameCatIsProvided()
    {
        //Arrange
        Person sut = Person.Create(
            userIdentityId: _userIdentityId,
            nickname: _defaultProperNickname,
            email: _defaultProperEmail,
            phoneNumber: _defaultProperPhone,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfoEmail: _defaultProperEmail,
            defaultAdvertisementContactInfoPhoneNumber: _defaultProperPhone
        );
        ICatPriorityCalculatorService calculatorService = Substitute.For<ICatPriorityCalculatorService>();
        calculatorService.Calculate(Arg.Any<Cat>()).ReturnsForAnyArgs(420);
        Cat cat = new Faker<Cat>()
            .CustomInstantiator( faker =>
                Cat.Create(
                    priorityScoreCalculator: calculatorService,
                    person: sut,
                    name: CatName.Create(faker.Person.FirstName), 
                    medicalHelpUrgency: MedicalHelpUrgency.NoNeed,
                    ageCategory: AgeCategory.Baby,
                    behavior: Behavior.Friendly, 
                    healthStatus: HealthStatus.Good,
                    additionalRequirements: Description.Create(faker.Lorem.Lines(2)),
                    isCastrated: false))
            .Generate();
        sut.RemoveCat(cat.Id);
        
        //Act
        Action operation = () => sut.RemoveCat(cat.Id);
        
        //Assert
        operation.Should().ThrowExactly<NotFoundExceptions.CatNotFoundException>();
    }

    [Fact]
    public void GetHighestPriorityScoreFromGivenCats_ShouldBeSuccessfull_WhenValidDataAreProvided()
    {
        //Arrange
        Person sut = Person.Create(
            userIdentityId: _userIdentityId,
            nickname: _defaultProperNickname,
            email: _defaultProperEmail,
            phoneNumber: _defaultProperPhone,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfoEmail: _defaultProperEmail,
            defaultAdvertisementContactInfoPhoneNumber: _defaultProperPhone
        );
        ICatPriorityCalculatorService calculatorService = Substitute.For<ICatPriorityCalculatorService>();
        calculatorService.Calculate(Arg.Any<Cat>()).ReturnsForAnyArgs(420);
        Faker<Cat> catGenerator = new Faker<Cat>()
            .CustomInstantiator(faker =>
                Cat.Create(
                    priorityScoreCalculator: calculatorService,
                    person: sut,
                    name: CatName.Create(faker.Person.FirstName),
                    additionalRequirements: Description.Create(faker.Person.FirstName),
                    medicalHelpUrgency: faker.PickRandomParam(MedicalHelpUrgency.NoNeed, MedicalHelpUrgency.ShouldSeeVet, MedicalHelpUrgency.HaveToSeeVet),
                    ageCategory: faker.PickRandomParam(AgeCategory.Baby, AgeCategory.Adult, AgeCategory.Senior),
                    behavior: faker.PickRandomParam(Behavior.Unfriendly, Behavior.Friendly),
                    healthStatus: faker.PickRandomParam(HealthStatus.Critical, HealthStatus.Poor, HealthStatus.Good),
                    isCastrated: false));
        Cat cat1 = catGenerator.Generate();
        Cat cat2 = catGenerator.Generate();
        
        //Act
        double result = sut.GetHighestPriorityScoreFromGivenCats([cat1.Id, cat2.Id]);

        //Assert
        result.Should().Be(420);
    }
    
    [Fact]
    public void GetHighestPriorityScoreFromGivenCats_ShouldThrow_WhenCatsOfAnotherPersonAreProvided()
    {
        //Arrange
        Person sut = Person.Create(
            userIdentityId: _userIdentityId,
            nickname: _defaultProperNickname,
            email: _defaultProperEmail,
            phoneNumber: _defaultProperPhone,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfoEmail: _defaultProperEmail,
            defaultAdvertisementContactInfoPhoneNumber: _defaultProperPhone
        );
        Person anotherPerson = new Faker<Person>().CustomInstantiator(faker => Person.Create(
            userIdentityId: _userIdentityId,
            nickname: Nickname.Create(faker.Person.FirstName),
            email: Email.Create(faker.Person.Email),
            phoneNumber: PhoneNumber.Create(faker.Person.Phone),
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfoEmail: Email.Create(faker.Person.Email),
            defaultAdvertisementContactInfoPhoneNumber: PhoneNumber.Create(faker.Person.Phone)
        )).Generate();
        ICatPriorityCalculatorService calculatorService = Substitute.For<ICatPriorityCalculatorService>();
        calculatorService.Calculate(Arg.Any<Cat>()).ReturnsForAnyArgs(420);
        Cat cat1 = new Faker<Cat>()
            .CustomInstantiator(faker =>
                Cat.Create(
                    priorityScoreCalculator: calculatorService,
                    person: sut,
                    name: CatName.Create(faker.Person.FirstName),
                    additionalRequirements: Description.Create(faker.Person.FirstName),
                    medicalHelpUrgency: faker.PickRandomParam(MedicalHelpUrgency.NoNeed, MedicalHelpUrgency.ShouldSeeVet, MedicalHelpUrgency.HaveToSeeVet),
                    ageCategory: faker.PickRandomParam(AgeCategory.Baby, AgeCategory.Adult, AgeCategory.Senior),
                    behavior: faker.PickRandomParam(Behavior.Unfriendly, Behavior.Friendly),
                    healthStatus: faker.PickRandomParam(HealthStatus.Critical, HealthStatus.Poor, HealthStatus.Good),
                    isCastrated: false)).Generate();
        Cat cat2 = new Faker<Cat>()
            .CustomInstantiator(faker =>
                Cat.Create(
                    priorityScoreCalculator: calculatorService,
                    person: anotherPerson,
                    name: CatName.Create(faker.Person.FirstName),
                    additionalRequirements: Description.Create(faker.Person.FirstName),
                    medicalHelpUrgency: faker.PickRandomParam(MedicalHelpUrgency.NoNeed, MedicalHelpUrgency.ShouldSeeVet, MedicalHelpUrgency.HaveToSeeVet),
                    ageCategory: faker.PickRandomParam(AgeCategory.Baby, AgeCategory.Adult, AgeCategory.Senior),
                    behavior: faker.PickRandomParam(Behavior.Unfriendly, Behavior.Friendly),
                    healthStatus: faker.PickRandomParam(HealthStatus.Critical, HealthStatus.Poor, HealthStatus.Good),
                    isCastrated: false)).Generate();
        
        //Act
        Action results = () => sut.GetHighestPriorityScoreFromGivenCats([cat1.Id, cat2.Id]);

        //Assert
        results.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public void UpdateCat_ShouldBeSuccessfull_WhenProperCatIsProvided()
    {
        //Arrange
        Person sut = Person.Create(
            userIdentityId: _userIdentityId,
            nickname: _defaultProperNickname,
            email: _defaultProperEmail,
            phoneNumber: _defaultProperPhone,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfoEmail: _defaultProperEmail,
            defaultAdvertisementContactInfoPhoneNumber: _defaultProperPhone
        );
        ICatPriorityCalculatorService calculatorService = Substitute.For<ICatPriorityCalculatorService>();
        calculatorService.Calculate(Arg.Any<Cat>()).ReturnsForAnyArgs(420);
        Cat cat1 = new Faker<Cat>()
            .CustomInstantiator(faker =>
                Cat.Create(
                    priorityScoreCalculator: calculatorService,
                    person: sut,
                    name: CatName.Create(faker.Person.FirstName),
                    additionalRequirements: Description.Create(faker.Person.FirstName),
                    medicalHelpUrgency: faker.PickRandomParam(MedicalHelpUrgency.NoNeed, MedicalHelpUrgency.ShouldSeeVet, MedicalHelpUrgency.HaveToSeeVet),
                    ageCategory: faker.PickRandomParam(AgeCategory.Baby, AgeCategory.Adult, AgeCategory.Senior),
                    behavior: faker.PickRandomParam(Behavior.Unfriendly, Behavior.Friendly),
                    healthStatus: faker.PickRandomParam(HealthStatus.Critical, HealthStatus.Poor, HealthStatus.Good),
                    isCastrated: false)).Generate();
        Advertisement advertisement = Advertisement.Create(
            currentDate: new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
            owner: sut,
            catsIdsToAssign: [cat1.Id],
            pickupAddress: sut.DefaultAdvertisementsPickupAddress,
            contactInfoEmail: sut.DefaultAdvertisementsContactInfoEmail,
            contactInfoPhoneNumber: sut.DefaultAdvertisementsContactInfoPhoneNumber,
            description: Description.Create("lorem ipsum"));
        //Act
        CatName nameForUpdate = CatName.Create("Krówka");
        Description descriptionForUpdate = Description.Create("Krówkaa");
        sut.UpdateCat(
            catId: cat1.Id,
            catPriorityCalculator: calculatorService,
            name: nameForUpdate,
            additionalRequirements: descriptionForUpdate,
            isCastrated: true,
            healthStatus: HealthStatus.Good, 
            ageCategory: AgeCategory.Adult, 
            behavior: Behavior.Friendly, 
            medicalHelpUrgency: MedicalHelpUrgency.NoNeed);
        
        //Assert
        cat1.Name.Value.Should().Be(nameForUpdate.Value);
        cat1.AdditionalRequirements.Value.Should().Be(descriptionForUpdate.Value);
        cat1.HealthStatus.Value.Should().Be(HealthStatus.Good);
        cat1.AgeCategory.Value.Should().Be(AgeCategory.Adult);
        cat1.Behavior.Value.Should().Be(Behavior.Friendly);
        cat1.MedicalHelpUrgency.Value.Should().Be(MedicalHelpUrgency.NoNeed);
    }
    
    [Fact]
    public void UpdateCat_ShouldThrowCatNotFound_WhenNotExistingCatIdIsProvided()
    {
        //Arrange
        Person sut = Person.Create(
            userIdentityId: _userIdentityId,
            nickname: _defaultProperNickname,
            email: _defaultProperEmail,
            phoneNumber: _defaultProperPhone,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfoEmail: _defaultProperEmail,
            defaultAdvertisementContactInfoPhoneNumber: _defaultProperPhone
        );
        ICatPriorityCalculatorService calculatorService = Substitute.For<ICatPriorityCalculatorService>();
        calculatorService.Calculate(Arg.Any<Cat>()).ReturnsForAnyArgs(420);
        //Act
        CatName nameForUpdate = CatName.Create("Krówka");
        Description descriptionForUpdate = Description.Create("Krówkaa");
        Action results = () => sut.UpdateCat(
            catId: Guid.NewGuid(),
            catPriorityCalculator: calculatorService,
            name: nameForUpdate,
            additionalRequirements: descriptionForUpdate,
            isCastrated: true,
            healthStatus: HealthStatus.Good, 
            ageCategory: AgeCategory.Adult, 
            behavior: Behavior.Friendly, 
            medicalHelpUrgency: MedicalHelpUrgency.NoNeed);

        //Assert
        results.Should().ThrowExactly<NotFoundExceptions.CatNotFoundException>();
    }
    
    [Fact]
    public void RemoveCat_ShouldThrowInvalidOperationException_WhenCatWithAssignedAdvertisementIsProvided()
    {
        //Arrange
        Person sut = Person.Create(
            userIdentityId: _userIdentityId,
            nickname: _defaultProperNickname,
            email: _defaultProperEmail,
            phoneNumber: _defaultProperPhone,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfoEmail: _defaultProperEmail,
            defaultAdvertisementContactInfoPhoneNumber: _defaultProperPhone
        );
        ICatPriorityCalculatorService calculatorService = Substitute.For<ICatPriorityCalculatorService>();
        calculatorService.Calculate(Arg.Any<Cat>()).ReturnsForAnyArgs(420);
        Cat cat1 = new Faker<Cat>()
            .CustomInstantiator(faker =>
                Cat.Create(
                    priorityScoreCalculator: calculatorService,
                    person: sut,
                    name: CatName.Create(faker.Person.FirstName),
                    additionalRequirements: Description.Create(faker.Person.FirstName),
                    medicalHelpUrgency: faker.PickRandomParam(MedicalHelpUrgency.NoNeed, MedicalHelpUrgency.ShouldSeeVet, MedicalHelpUrgency.HaveToSeeVet),
                    ageCategory: faker.PickRandomParam(AgeCategory.Baby, AgeCategory.Adult, AgeCategory.Senior),
                    behavior: faker.PickRandomParam(Behavior.Unfriendly, Behavior.Friendly),
                    healthStatus: faker.PickRandomParam(HealthStatus.Critical, HealthStatus.Poor, HealthStatus.Good),
                    isCastrated: false)).Generate();
        Advertisement.Create(
            currentDate: new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
            owner: sut,
            catsIdsToAssign: [cat1.Id],
            pickupAddress: sut.DefaultAdvertisementsPickupAddress,
            contactInfoEmail: sut.DefaultAdvertisementsContactInfoEmail,
            contactInfoPhoneNumber: sut.DefaultAdvertisementsContactInfoPhoneNumber,
            description: Description.Create("lorem ipsum"));
        
        //Act
        Action results = () => sut.RemoveCat(cat1.Id);
        
        //Assert
        results.Should().ThrowExactly<InvalidOperationException>();
    }
    
    // [Fact]
    // public void MarkCatsFromConcreteAdvertisementAsAdopted_BeSuccessfull_WhenValidDataAreProvided()
    // {
    //     //Arrange
    //     Person sut = Person.Create(
    //         userIdentityId: _userIdentityId,
    //         nickname: _defaultProperNickname,
    //         email: _defaultProperEmail,
    //         phoneNumber: _defaultProperPhone,
    //         defaultAdvertisementPickupAddress: PickupAddress,
    //         defaultAdvertisementContactInfoEmail: _defaultProperEmail,
    //         defaultAdvertisementContactInfoPhoneNumber: _defaultProperPhone
    //     );
    //     ICatPriorityCalculatorService calculatorService = Substitute.For<ICatPriorityCalculatorService>();
    //     calculatorService.Calculate(Arg.Any<Cat>()).ReturnsForAnyArgs(420);
    //     Faker<Cat> catGenerator = new Faker<Cat>()
    //         .CustomInstantiator(faker =>
    //             Cat.Create(
    //                 priorityScoreCalculator: calculatorService,
    //                 person: sut,
    //                 name: CatName.Create(faker.Person.FirstName),
    //                 additionalRequirements: Description.Create(faker.Person.FirstName),
    //                 medicalHelpUrgency: faker.PickRandomParam(MedicalHelpUrgency.NoNeed, MedicalHelpUrgency.ShouldSeeVet, MedicalHelpUrgency.HaveToSeeVet),
    //                 ageCategory: faker.PickRandomParam(AgeCategory.Baby, AgeCategory.Adult, AgeCategory.Senior),
    //                 behavior: faker.PickRandomParam(Behavior.Unfriendly, Behavior.Friendly),
    //                 healthStatus: faker.PickRandomParam(HealthStatus.Critical, HealthStatus.Poor, HealthStatus.Good),
    //                 isCastrated: false));
    //     Cat cat1 = catGenerator.Generate();
    //     Cat cat2 = catGenerator.Generate();
    //     Advertisement advertisement = Advertisement.Create(
    //         currentDate: new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
    //         owner: sut,
    //         catsIdsToAssign: [cat1.Id, cat2.Id],
    //         pickupAddress: sut.DefaultAdvertisementsPickupAddress,
    //         contactInfoEmail: sut.DefaultAdvertisementsContactInfoEmail,
    //         contactInfoPhoneNumber: sut.DefaultAdvertisementsContactInfoPhoneNumber,
    //         description: Description.Create("lorem ipsum"));
    //     
    //     //Act
    //     sut.MarkCatsFromConcreteAdvertisementAsAdopted(advertisement.Id);
    //     
    //     //Assert
    //     cat1.IsAdopted.Should().BeTrue();
    //     cat2.IsAdopted.Should().BeTrue();
    // }
}
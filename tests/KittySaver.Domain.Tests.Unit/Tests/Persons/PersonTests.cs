using Bogus;
using FluentAssertions;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Common.Primitives.Enums;
using KittySaver.Domain.Persons;
using KittySaver.Domain.ValueObjects;
using NSubstitute;
using Person = KittySaver.Domain.Persons.Person;

namespace KittySaver.Domain.Tests.Unit.Tests.Persons;

public class PersonTests
{
    private readonly Guid _userIdentityId = Guid.NewGuid();
    private readonly Nickname _defaultProperNickname = Nickname.Create("Artur");
    private readonly Email _defaultProperEmail = Email.Create("fake@fake.fake");
    private readonly PhoneNumber _defaultProperPhone = PhoneNumber.Create("+48111222333");

    private static readonly Address PickupAddress = new Faker<Address>()
        .CustomInstantiator(faker =>
            Address.Create(
                faker.Address.CountryCode(),
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
            nickname: nickname,
            email: email,
            phoneNumber: phoneNumber,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfoEmail: defaultEmail,
            defaultAdvertisementContactInfoPhoneNumber: defaultPhoneNumber
        );
        sut.SetUserIdentityId(_userIdentityId);

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
                country: faker.Address.CountryCode(),
                state: "",
                zipCode: faker.Address.ZipCode(),
                city: faker.Address.City(),
                street: faker.Address.StreetName(),
                buildingNumber: faker.Address.BuildingNumber()
            )).Generate();
        
        //Act
        Person sut = Person.Create(
            nickname: nickname,
            email: email,
            phoneNumber: phoneNumber,
            defaultAdvertisementPickupAddress: pickupAddress,
            defaultAdvertisementContactInfoEmail: defaultEmail,
            defaultAdvertisementContactInfoPhoneNumber: defaultPhoneNumber
        );
        sut.SetUserIdentityId(_userIdentityId);

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

        Person user = Person.Create(
            nickname: _defaultProperNickname,
            email: _defaultProperEmail,
            phoneNumber: _defaultProperPhone,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfoEmail: _defaultProperEmail,
            defaultAdvertisementContactInfoPhoneNumber: _defaultProperPhone
        );
        
        //Act
        Action creation = () => user.SetUserIdentityId(emptyGuid);
        
        //Assert
        creation.Should().Throw<ArgumentException>()
            .WithMessage("Provided empty guid. (Parameter 'UserIdentityId')");
    }
    
    [Fact]
    public void AddCat_ShouldAddCatToList_WhenValidCatIsProvided()
    {
        //Arrange
        Person sut = Person.Create(
            
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
                sut.AddCat(
                    priorityScoreCalculator: calculatorService,
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
                sut.AddCat(
                    priorityScoreCalculator: calculatorService,
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
    public void RemoveCat_ShouldThrowCatNotFoundException_WhenTheSameCatIsProvided()
    {
        //Arrange
        Person sut = Person.Create(
            
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
                sut.AddCat(
                    priorityScoreCalculator: calculatorService,
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
                sut.AddCat(
                    priorityScoreCalculator: calculatorService,
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
            
            nickname: _defaultProperNickname,
            email: _defaultProperEmail,
            phoneNumber: _defaultProperPhone,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfoEmail: _defaultProperEmail,
            defaultAdvertisementContactInfoPhoneNumber: _defaultProperPhone
        );
        Person anotherPerson = new Faker<Person>().CustomInstantiator(faker => Person.Create(
            
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
                sut.AddCat(
                    priorityScoreCalculator: calculatorService,
                    name: CatName.Create(faker.Person.FirstName),
                    additionalRequirements: Description.Create(faker.Person.FirstName),
                    medicalHelpUrgency: faker.PickRandomParam(MedicalHelpUrgency.NoNeed, MedicalHelpUrgency.ShouldSeeVet, MedicalHelpUrgency.HaveToSeeVet),
                    ageCategory: faker.PickRandomParam(AgeCategory.Baby, AgeCategory.Adult, AgeCategory.Senior),
                    behavior: faker.PickRandomParam(Behavior.Unfriendly, Behavior.Friendly),
                    healthStatus: faker.PickRandomParam(HealthStatus.Critical, HealthStatus.Poor, HealthStatus.Good),
                    isCastrated: false)).Generate();
        Cat cat2 = new Faker<Cat>()
            .CustomInstantiator(faker =>
                anotherPerson.AddCat(
                    priorityScoreCalculator: calculatorService,
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
                sut.AddCat(
                    priorityScoreCalculator: calculatorService,
                    name: CatName.Create(faker.Person.FirstName),
                    additionalRequirements: Description.Create(faker.Person.FirstName),
                    medicalHelpUrgency: faker.PickRandomParam(MedicalHelpUrgency.NoNeed, MedicalHelpUrgency.ShouldSeeVet, MedicalHelpUrgency.HaveToSeeVet),
                    ageCategory: faker.PickRandomParam(AgeCategory.Baby, AgeCategory.Adult, AgeCategory.Senior),
                    behavior: faker.PickRandomParam(Behavior.Unfriendly, Behavior.Friendly),
                    healthStatus: faker.PickRandomParam(HealthStatus.Critical, HealthStatus.Poor, HealthStatus.Good),
                    isCastrated: false)).Generate();
        sut.AddAdvertisement(
            dateOfCreation: new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
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
                sut.AddCat(
                    priorityScoreCalculator: calculatorService,
                    name: CatName.Create(faker.Person.FirstName),
                    additionalRequirements: Description.Create(faker.Person.FirstName),
                    medicalHelpUrgency: faker.PickRandomParam(MedicalHelpUrgency.NoNeed, MedicalHelpUrgency.ShouldSeeVet, MedicalHelpUrgency.HaveToSeeVet),
                    ageCategory: faker.PickRandomParam(AgeCategory.Baby, AgeCategory.Adult, AgeCategory.Senior),
                    behavior: faker.PickRandomParam(Behavior.Unfriendly, Behavior.Friendly),
                    healthStatus: faker.PickRandomParam(HealthStatus.Critical, HealthStatus.Poor, HealthStatus.Good),
                    isCastrated: false)).Generate();
        sut.AddAdvertisement(
            dateOfCreation: new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
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

    [Fact]
    public void GetAdvertisements_ShouldReturnAdvertisements_WhenAdvertisementsExists()
    {
        //Arrange
        Person sut = Person.Create(
            
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
            .CustomInstantiator(faker =>
                sut.AddCat(
                    priorityScoreCalculator: calculatorService,
                    name: CatName.Create(faker.Person.FirstName),
                    additionalRequirements: Description.Create(faker.Person.FirstName),
                    medicalHelpUrgency: faker.PickRandomParam(MedicalHelpUrgency.NoNeed, MedicalHelpUrgency.ShouldSeeVet, MedicalHelpUrgency.HaveToSeeVet),
                    ageCategory: faker.PickRandomParam(AgeCategory.Baby, AgeCategory.Adult, AgeCategory.Senior),
                    behavior: faker.PickRandomParam(Behavior.Unfriendly, Behavior.Friendly),
                    healthStatus: faker.PickRandomParam(HealthStatus.Critical, HealthStatus.Poor, HealthStatus.Good),
                    isCastrated: false)).Generate();
        
        //Act
        Advertisement advertisement = sut.AddAdvertisement(
            dateOfCreation: new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
            catsIdsToAssign: [cat.Id],
            pickupAddress: sut.DefaultAdvertisementsPickupAddress,
            contactInfoEmail: sut.DefaultAdvertisementsContactInfoEmail,
            contactInfoPhoneNumber: sut.DefaultAdvertisementsContactInfoPhoneNumber,
            description: Description.Create("lorem ipsum"));
        
        //Assert
        sut.Advertisements.Should().BeEquivalentTo([advertisement]);
    }

    // Advertisement Management Tests
    [Fact]
    public void RemoveAdvertisement_WhenAdvertisementExists_ShouldRemoveAdvertisement()
    {
        // Arrange
        Person sut = Person.Create(
            
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
            .CustomInstantiator(faker =>
                sut.AddCat(
                    priorityScoreCalculator: calculatorService,
                    name: CatName.Create(faker.Person.FirstName),
                    additionalRequirements: Description.Create(faker.Person.FirstName),
                    medicalHelpUrgency: faker.PickRandomParam(MedicalHelpUrgency.NoNeed, MedicalHelpUrgency.ShouldSeeVet, MedicalHelpUrgency.HaveToSeeVet),
                    ageCategory: faker.PickRandomParam(AgeCategory.Baby, AgeCategory.Adult, AgeCategory.Senior),
                    behavior: faker.PickRandomParam(Behavior.Unfriendly, Behavior.Friendly),
                    healthStatus: faker.PickRandomParam(HealthStatus.Critical, HealthStatus.Poor, HealthStatus.Good),
                    isCastrated: false)).Generate();
        
        Advertisement advertisement = sut.AddAdvertisement(
            dateOfCreation: new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
            catsIdsToAssign: [cat.Id],
            pickupAddress: sut.DefaultAdvertisementsPickupAddress,
            contactInfoEmail: sut.DefaultAdvertisementsContactInfoEmail,
            contactInfoPhoneNumber: sut.DefaultAdvertisementsContactInfoPhoneNumber,
            description: Description.Create("lorem ipsum"));

        // Act
        sut.RemoveAdvertisement(advertisement.Id);

        // Assert
        sut.Advertisements.Should().BeEmpty();
    }

    [Fact]
    public void RemoveAdvertisement_WhenAdvertisementDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        Person sut = Person.Create(
            
            nickname: _defaultProperNickname,
            email: _defaultProperEmail,
            phoneNumber: _defaultProperPhone,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfoEmail: _defaultProperEmail,
            defaultAdvertisementContactInfoPhoneNumber: _defaultProperPhone
        );

        // Act & Assert
        Action act = () => sut.RemoveAdvertisement(Guid.NewGuid());
        
        act.Should().Throw<NotFoundExceptions.AdvertisementNotFoundException>();
    }

    [Fact]
    public void UpdateAdvertisement_WhenAdvertisementExists_ShouldUpdateDetails()
    {
        // Arrange
        Person sut = Person.Create(
            
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
            .CustomInstantiator(faker =>
                sut.AddCat(
                    priorityScoreCalculator: calculatorService,
                    name: CatName.Create(faker.Person.FirstName),
                    additionalRequirements: Description.Create(faker.Person.FirstName),
                    medicalHelpUrgency: faker.PickRandomParam(MedicalHelpUrgency.NoNeed, MedicalHelpUrgency.ShouldSeeVet, MedicalHelpUrgency.HaveToSeeVet),
                    ageCategory: faker.PickRandomParam(AgeCategory.Baby, AgeCategory.Adult, AgeCategory.Senior),
                    behavior: faker.PickRandomParam(Behavior.Unfriendly, Behavior.Friendly),
                    healthStatus: faker.PickRandomParam(HealthStatus.Critical, HealthStatus.Poor, HealthStatus.Good),
                    isCastrated: false)).Generate();
        
        //Act
        Advertisement advertisement = sut.AddAdvertisement(
            dateOfCreation: new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
            catsIdsToAssign: [cat.Id],
            pickupAddress: sut.DefaultAdvertisementsPickupAddress,
            contactInfoEmail: sut.DefaultAdvertisementsContactInfoEmail,
            contactInfoPhoneNumber: sut.DefaultAdvertisementsContactInfoPhoneNumber,
            description: Description.Create("lorem ipsum"));
        
        Description newDescription = Description.Create("Updated Description");
        Address newAddress = Address.Create("PL", "New State", "54321", "New City", "New Street", "2");
        Email newEmail = Email.Create("new@example.com");
        PhoneNumber newPhone = PhoneNumber.Create("0987654321");

        // Act
        sut.UpdateAdvertisement(
            advertisement.Id,
            newDescription,
            newAddress,
            newEmail,
            newPhone);

        // Assert
        Advertisement? updatedAd = sut.Advertisements.Should().ContainSingle().Subject;
        updatedAd.Description.Value.Should().Be(newDescription.Value);
        updatedAd.PickupAddress.City.Should().Be(newAddress.City);
        updatedAd.ContactInfoEmail.Value.Should().Be(newEmail.Value);
        updatedAd.ContactInfoPhoneNumber.Value.Should().Be(newPhone.Value);
    }

    [Fact]
    public void CloseAdvertisement_WhenAdvertisementExists_ShouldCloseAdvertisement()
    {
        // Arrange
        Person sut = Person.Create(
            
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
            .CustomInstantiator(faker =>
                sut.AddCat(
                    priorityScoreCalculator: calculatorService,
                    name: CatName.Create(faker.Person.FirstName),
                    additionalRequirements: Description.Create(faker.Person.FirstName),
                    medicalHelpUrgency: faker.PickRandomParam(MedicalHelpUrgency.NoNeed, MedicalHelpUrgency.ShouldSeeVet, MedicalHelpUrgency.HaveToSeeVet),
                    ageCategory: faker.PickRandomParam(AgeCategory.Baby, AgeCategory.Adult, AgeCategory.Senior),
                    behavior: faker.PickRandomParam(Behavior.Unfriendly, Behavior.Friendly),
                    healthStatus: faker.PickRandomParam(HealthStatus.Critical, HealthStatus.Poor, HealthStatus.Good),
                    isCastrated: false)).Generate();
        
        //Act
        Advertisement advertisement = sut.AddAdvertisement(
            dateOfCreation: new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
            catsIdsToAssign: [cat.Id],
            pickupAddress: sut.DefaultAdvertisementsPickupAddress,
            contactInfoEmail: sut.DefaultAdvertisementsContactInfoEmail,
            contactInfoPhoneNumber: sut.DefaultAdvertisementsContactInfoPhoneNumber,
            description: Description.Create("lorem ipsum"));
        sut.ActivateAdvertisementIfThumbnailIsUploadedForTheFirstTime(advertisement.Id);

        DateTimeOffset currentDate = DateTimeOffset.UtcNow;

        // Act
        sut.CloseAdvertisement(advertisement.Id, currentDate);

        // Assert
        Advertisement? closedAd = sut.Advertisements.Should().ContainSingle().Subject;
        closedAd.Status.Should().Be(Advertisement.AdvertisementStatus.Closed);
        closedAd.ClosedOn.Should().Be(currentDate);
    }

    [Fact]
    public void ExpireAdvertisement_WhenAdvertisementExists_ShouldExpireAdvertisement()
    {
        // Arrange
        Person sut = Person.Create(
            
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
            .CustomInstantiator(faker =>
                sut.AddCat(
                    priorityScoreCalculator: calculatorService,
                    name: CatName.Create(faker.Person.FirstName),
                    additionalRequirements: Description.Create(faker.Person.FirstName),
                    medicalHelpUrgency: faker.PickRandomParam(MedicalHelpUrgency.NoNeed, MedicalHelpUrgency.ShouldSeeVet, MedicalHelpUrgency.HaveToSeeVet),
                    ageCategory: faker.PickRandomParam(AgeCategory.Baby, AgeCategory.Adult, AgeCategory.Senior),
                    behavior: faker.PickRandomParam(Behavior.Unfriendly, Behavior.Friendly),
                    healthStatus: faker.PickRandomParam(HealthStatus.Critical, HealthStatus.Poor, HealthStatus.Good),
                    isCastrated: false)).Generate();
        
        Advertisement advertisement = sut.AddAdvertisement(
            dateOfCreation: new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
            catsIdsToAssign: [cat.Id],
            pickupAddress: sut.DefaultAdvertisementsPickupAddress,
            contactInfoEmail: sut.DefaultAdvertisementsContactInfoEmail,
            contactInfoPhoneNumber: sut.DefaultAdvertisementsContactInfoPhoneNumber,
            description: Description.Create("lorem ipsum"));
        sut.ActivateAdvertisementIfThumbnailIsUploadedForTheFirstTime(advertisement.Id);
        DateTimeOffset expirationDate = DateTimeOffset.UtcNow.AddDays(31); // After default expiration period

        // Act
        sut.ExpireAdvertisement(advertisement.Id, expirationDate);

        // Assert
        Advertisement? expiredAd = sut.Advertisements.Should().ContainSingle().Subject;
        expiredAd.Status.Should().Be(Advertisement.AdvertisementStatus.Expired);
    }

    [Fact]
    public void RefreshAdvertisement_WhenAdvertisementExists_ShouldRefreshExpirationDate()
    {
        // Arrange
        Person sut = Person.Create(
            
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
            .CustomInstantiator(faker =>
                sut.AddCat(
                    priorityScoreCalculator: calculatorService,
                    name: CatName.Create(faker.Person.FirstName),
                    additionalRequirements: Description.Create(faker.Person.FirstName),
                    medicalHelpUrgency: faker.PickRandomParam(MedicalHelpUrgency.NoNeed, MedicalHelpUrgency.ShouldSeeVet, MedicalHelpUrgency.HaveToSeeVet),
                    ageCategory: faker.PickRandomParam(AgeCategory.Baby, AgeCategory.Adult, AgeCategory.Senior),
                    behavior: faker.PickRandomParam(Behavior.Unfriendly, Behavior.Friendly),
                    healthStatus: faker.PickRandomParam(HealthStatus.Critical, HealthStatus.Poor, HealthStatus.Good),
                    isCastrated: false)).Generate();
        
        Advertisement advertisement = sut.AddAdvertisement(
            dateOfCreation: new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
            catsIdsToAssign: [cat.Id],
            pickupAddress: sut.DefaultAdvertisementsPickupAddress,
            contactInfoEmail: sut.DefaultAdvertisementsContactInfoEmail,
            contactInfoPhoneNumber: sut.DefaultAdvertisementsContactInfoPhoneNumber,
            description: Description.Create("lorem ipsum"));
        sut.ActivateAdvertisementIfThumbnailIsUploadedForTheFirstTime(advertisement.Id);
        DateTimeOffset refreshDate = DateTimeOffset.UtcNow;
        // Act
        sut.RefreshAdvertisement(advertisement.Id, refreshDate);

        // Assert
        Advertisement? refreshedAd = sut.Advertisements.Should().ContainSingle().Subject;
        refreshedAd.ExpiresOn.Should().Be(refreshDate + Advertisement.ExpiringPeriodInDays);
    }

    [Fact]
    public void ReplaceCatsOfAdvertisement_ShouldUpdateCatsAssignments()
    {
        // Arrange
        Person sut = Person.Create(
            
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
                sut.AddCat(
                    priorityScoreCalculator: calculatorService,
                    name: CatName.Create(faker.Person.FirstName),
                    additionalRequirements: Description.Create(faker.Person.FirstName),
                    medicalHelpUrgency: faker.PickRandomParam(MedicalHelpUrgency.NoNeed, MedicalHelpUrgency.ShouldSeeVet, MedicalHelpUrgency.HaveToSeeVet),
                    ageCategory: faker.PickRandomParam(AgeCategory.Baby, AgeCategory.Adult, AgeCategory.Senior),
                    behavior: faker.PickRandomParam(Behavior.Unfriendly, Behavior.Friendly),
                    healthStatus: faker.PickRandomParam(HealthStatus.Critical, HealthStatus.Poor, HealthStatus.Good),
                    isCastrated: false)).Generate();
        Cat cat2 = new Faker<Cat>()
            .CustomInstantiator(faker =>
                sut.AddCat(
                    priorityScoreCalculator: calculatorService,
                    name: CatName.Create(faker.Person.FirstName),
                    additionalRequirements: Description.Create(faker.Person.FirstName),
                    medicalHelpUrgency: faker.PickRandomParam(MedicalHelpUrgency.NoNeed, MedicalHelpUrgency.ShouldSeeVet, MedicalHelpUrgency.HaveToSeeVet),
                    ageCategory: faker.PickRandomParam(AgeCategory.Baby, AgeCategory.Adult, AgeCategory.Senior),
                    behavior: faker.PickRandomParam(Behavior.Unfriendly, Behavior.Friendly),
                    healthStatus: faker.PickRandomParam(HealthStatus.Critical, HealthStatus.Poor, HealthStatus.Good),
                    isCastrated: false)).Generate();
        
        //Act
        Advertisement advertisement = sut.AddAdvertisement(
            dateOfCreation: new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero),
            catsIdsToAssign: [cat1.Id],
            pickupAddress: sut.DefaultAdvertisementsPickupAddress,
            contactInfoEmail: sut.DefaultAdvertisementsContactInfoEmail,
            contactInfoPhoneNumber: sut.DefaultAdvertisementsContactInfoPhoneNumber,
            description: Description.Create("lorem ipsum"));

        // Act
        sut.ReplaceCatsOfAdvertisement(advertisement.Id, [cat2.Id]);

        // Assert
        cat1.AdvertisementId.Should().BeNull();
        cat2.AdvertisementId.Should().Be(advertisement.Id);
    }

    [Fact]
    public void ChangeNickname_ShouldUpdateNickname()
    {
        // Arrange
        Person sut = Person.Create(
            
            nickname: _defaultProperNickname,
            email: _defaultProperEmail,
            phoneNumber: _defaultProperPhone,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfoEmail: _defaultProperEmail,
            defaultAdvertisementContactInfoPhoneNumber: _defaultProperPhone
        );
        Nickname newNickname = Nickname.Create("NewNickname");

        // Act
        sut.ChangeNickname(newNickname);

        // Assert
        sut.Nickname.Value.Should().Be(newNickname.Value);
    }

    [Fact]
    public void ChangeEmail_ShouldUpdateEmail()
    {
        // Arrange
        Person sut = Person.Create(
            
            nickname: _defaultProperNickname,
            email: _defaultProperEmail,
            phoneNumber: _defaultProperPhone,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfoEmail: _defaultProperEmail,
            defaultAdvertisementContactInfoPhoneNumber: _defaultProperPhone
        );
        Email newEmail = Email.Create("new@example.com");

        // Act
        sut.ChangeEmail(newEmail);

        // Assert
        sut.Email.Value.Should().Be(newEmail.Value);
    }

    [Fact]
    public void ChangePhoneNumber_ShouldUpdatePhoneNumber()
    {
        // Arrange
        Person sut = Person.Create(
            
            nickname: _defaultProperNickname,
            email: _defaultProperEmail,
            phoneNumber: _defaultProperPhone,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfoEmail: _defaultProperEmail,
            defaultAdvertisementContactInfoPhoneNumber: _defaultProperPhone
        );
        PhoneNumber newPhoneNumber = PhoneNumber.Create("9876543210");

        // Act
        sut.ChangePhoneNumber(newPhoneNumber);

        // Assert
        sut.PhoneNumber.Value.Should().Be(newPhoneNumber.Value);
    }

    [Fact]
    public void ChangeDefaultsForAdvertisement_ShouldUpdateDefaults()
    {
        // Arrange
        Person sut = Person.Create(
            
            nickname: _defaultProperNickname,
            email: _defaultProperEmail,
            phoneNumber: _defaultProperPhone,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfoEmail: _defaultProperEmail,
            defaultAdvertisementContactInfoPhoneNumber: _defaultProperPhone
        );
        Address newAddress = Address.Create("PL", "New State", "54321", "New City", "New Street", "2");
        Email newEmail = Email.Create("new@example.com");
        PhoneNumber newPhone = PhoneNumber.Create("9876543210");

        // Act
        sut.ChangeDefaultsForAdvertisement(newAddress, newEmail, newPhone);

        // Assert
        sut.DefaultAdvertisementsPickupAddress.City.Should().Be(newAddress.City);
        sut.DefaultAdvertisementsContactInfoEmail.Value.Should().Be(newEmail.Value);
        sut.DefaultAdvertisementsContactInfoPhoneNumber.Value.Should().Be(newPhone.Value);
    }
}
using Bogus;
using FluentAssertions;
using KittySaver.Api.Shared.Domain.Common.Primitives.Enums;
using KittySaver.Api.Shared.Domain.Persons;
using KittySaver.Api.Shared.Domain.ValueObjects;
using NSubstitute;
using Shared;
using Person = KittySaver.Api.Shared.Domain.Persons.Person;

namespace KittySaver.Api.Tests.Unit.Persons;

public class PersonTests
{
    private readonly Guid _userIdentityId = Guid.NewGuid();
    private readonly FirstName _defaultProperFirstName = FirstName.Create("Artur");
    private readonly LastName _defaultProperLastName = LastName.Create("Koniec");
    private readonly Email _defaultProperEmail = Email.Create("fake@fake.fake");
    private readonly PhoneNumber _defaultProperPhone = PhoneNumber.Create("+48111222333");

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
    
    private static readonly Address PickupAddress = new Faker<Address>()
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
    
    [Fact]
    public void PersonGetters_ShouldReturnProperValues_WhenPersonIsInstantiated()
    {
        //Arrange
        FirstName firstName = FirstName.Create("Artur");
        LastName lastName = LastName.Create("Koniec");
        Email email = Email.Create("koniecdev@gmail.com");
        PhoneNumber phoneNumber = PhoneNumber.Create("535143330");
        Email defaultEmail = Email.Create("koniecdevcontact@gmail.com");
        PhoneNumber defaultPhoneNumber = PhoneNumber.Create("5351433300");
        
        //Act
        Person sut = Person.Create(
            userIdentityId: _userIdentityId,
            firstName: firstName,
            lastName: lastName,
            email: email,
            phoneNumber: phoneNumber,
            residentalAddress: Address,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfoEmail: defaultEmail,
            defaultAdvertisementContactInfoPhoneNumber: defaultPhoneNumber
        );

        //Assert
        sut.Id.Should().NotBeEmpty();
        sut.FirstName.Should().Be(firstName);
        sut.LastName.Should().Be(lastName);
        sut.FullName.Should().Be($"{firstName} {lastName}");
        sut.Email.Should().Be(email);
        sut.PhoneNumber.Should().Be(phoneNumber);
        sut.ResidentalAddress.Should().BeEquivalentTo(Address);
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
        FirstName firstName = FirstName.Create("Artur");
        LastName lastName = LastName.Create("Koniec");
        Email email = Email.Create("koniecdev@gmail.com");
        PhoneNumber phoneNumber = PhoneNumber.Create("535143330");
        Email defaultEmail = Email.Create("koniecdevcontact@gmail.com");
        PhoneNumber defaultPhoneNumber = PhoneNumber.Create("5351433300");
        
        Address address = new Faker<Address>()
        .CustomInstantiator(faker =>
            new Address
            {
                Country = faker.Address.Country(),
                State = "",
                ZipCode = faker.Address.ZipCode(),
                City = faker.Address.City(),
                Street = faker.Address.StreetName(),
                BuildingNumber = faker.Address.BuildingNumber()
            }).Generate();
    
        Address pickupAddress = new Faker<Address>()
        .CustomInstantiator(faker =>
            new Address
            {
                Country = faker.Address.Country(),
                State = "",
                ZipCode = faker.Address.ZipCode(),
                City = faker.Address.City(),
                Street = faker.Address.StreetName(),
                BuildingNumber = faker.Address.BuildingNumber()
            }).Generate();
        
        //Act
        Person sut = Person.Create(
            userIdentityId: _userIdentityId,
            firstName: firstName,
            lastName: lastName,
            email: email,
            phoneNumber: phoneNumber,
            residentalAddress: address,
            defaultAdvertisementPickupAddress: pickupAddress,
            defaultAdvertisementContactInfoEmail: defaultEmail,
            defaultAdvertisementContactInfoPhoneNumber: defaultPhoneNumber
        );

        //Assert
        sut.Id.Should().NotBeEmpty();
        sut.FirstName.Should().Be(firstName);
        sut.LastName.Should().Be(lastName);
        sut.FullName.Should().Be($"{firstName} {lastName}");
        sut.Email.Should().Be(email);
        sut.PhoneNumber.Should().Be(phoneNumber);
        sut.ResidentalAddress.Should().NotBeNull();
        sut.ResidentalAddress.Country.Should().Be(address.Country);
        sut.ResidentalAddress.State.Should().BeNull();
        sut.ResidentalAddress.ZipCode.Should().Be(address.ZipCode);
        sut.ResidentalAddress.City.Should().Be(address.City);
        sut.ResidentalAddress.Street.Should().Be(address.Street);
        sut.ResidentalAddress.BuildingNumber.Should().Be(address.BuildingNumber);
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
    public void FullNameGet_ShouldReturnProperlyConcatenatedName_WhenBothFirstNameAndLastNameAreProperlyProvided()
    {
        //Act
        Person sut = Person.Create(
            userIdentityId: _userIdentityId,
            firstName: _defaultProperFirstName,
            lastName: _defaultProperLastName,
            email: _defaultProperEmail,
            phoneNumber: _defaultProperPhone,
            residentalAddress: Address,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfoEmail: _defaultProperEmail,
            defaultAdvertisementContactInfoPhoneNumber: _defaultProperPhone
        );

        //Assert
        sut.FullName.Should().Be("Artur Koniec");
    }
    
    [Fact]
    public void UserIdentityIdSet_ShouldThrowArgumentException_WhenEmptyGuidIsProvided()
    {
        //Arrange
        Guid emptyGuid = Guid.Empty;

        //Act
        Action creation = () => Person.Create(
            userIdentityId: emptyGuid,
            firstName: _defaultProperFirstName,
            lastName: _defaultProperLastName,
            email: _defaultProperEmail,
            phoneNumber: _defaultProperPhone,
            residentalAddress: Address,
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
            firstName: _defaultProperFirstName,
            lastName: _defaultProperLastName,
            email: _defaultProperEmail,
            phoneNumber: _defaultProperPhone,
            residentalAddress: Address,
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
            firstName: _defaultProperFirstName,
            lastName: _defaultProperLastName,
            email: _defaultProperEmail,
            phoneNumber: _defaultProperPhone,
            residentalAddress: Address,
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
        sut.RemoveCat(cat);

        //Assert
        sut.Cats.Should().NotContain(cat);
    }

    [Fact]
    public void AddCat_ShouldThrowInvalidOperationException_WhenTheSameCatIsProvided()
    {
        //Arrange
        Person sut = Person.Create(
            userIdentityId: _userIdentityId,
            firstName: _defaultProperFirstName,
            lastName: _defaultProperLastName,
            email: _defaultProperEmail,
            phoneNumber: _defaultProperPhone,
            residentalAddress: Address,
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
    public void RemoveCat_ShouldThrowInvalidOperationException_WhenTheSameCatIsProvided()
    {
        //Arrange
        Person sut = Person.Create(
            userIdentityId: _userIdentityId,
            firstName: _defaultProperFirstName,
            lastName: _defaultProperLastName,
            email: _defaultProperEmail,
            phoneNumber: _defaultProperPhone,
            residentalAddress: Address,
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
        sut.RemoveCat(cat);
        
        //Act
        Action operation = () => sut.RemoveCat(cat);
        
        //Assert
        operation.Should().ThrowExactly<InvalidOperationException>();
    }
}
using Bogus;
using FluentAssertions;
using KittySaver.Api.Shared.Domain.Common.Interfaces;
using KittySaver.Api.Shared.Domain.Entites;
using KittySaver.Api.Shared.Domain.Enums;
using KittySaver.Api.Shared.Domain.Services;
using KittySaver.Api.Shared.Domain.ValueObjects;
using NSubstitute;
using Shared;
using Person = KittySaver.Api.Shared.Domain.Entites.Person;

namespace KittySaver.Api.Tests.Unit.Persons;

public class PersonTests
{
    private readonly Guid _userIdentityId = Guid.NewGuid();
    private const string DefaultProperFirstName = "artur";
    private const string DefaultProperLastName = "koniec";
    private const string DefaultProperEmail = "fake@fake.fake";
    private const string DefaultProperPhone = "+48111222333";

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
    
    [Fact]
    public void PersonGetters_ShouldReturnProperValues_WhenPersonIsInstantiated()
    {
        //Arrange
        const string firstName = "Artur";
        const string lastName = "Koniec";
        const string email = "koniecdev@gmail.com";
        const string phone = "535143330";
        
        //Act
        Person sut = Person.Create(
            userIdentityId: _userIdentityId,
            firstName: firstName,
            lastName: lastName,
            email: email,
            phoneNumber: phone,
            address: Address,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfo: ContactInfo
        );

        //Assert
        sut.Id.Should().NotBeEmpty();
        sut.FirstName.Should().Be(firstName);
        sut.LastName.Should().Be(lastName);
        sut.FullName.Should().Be($"{firstName} {lastName}");
        sut.Email.Should().Be(email);
        sut.PhoneNumber.Should().Be(phone);
        sut.Address.Should().BeEquivalentTo(Address);
        sut.DefaultAdvertisementsPickupAddress.Should().BeEquivalentTo(PickupAddress);
        sut.DefaultAdvertisementsContactInfo.Should().BeEquivalentTo(ContactInfo);
        sut.UserIdentityId.Should().Be(_userIdentityId);
        sut.CurrentRole.Should().Be(Person.Role.Regular);
    }
    
    [Fact]
    public void PersonGetters_ShouldReturnProperValues_WhenPersonIsInstantiatedWithJustRequiredProperties()
    {
        //Arrange
        const string firstName = "Artur";
        const string lastName = "Koniec";
        const string email = "koniecdev@gmail.com";
        const string phone = "535143330";
        
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
    
        PickupAddress pickupAddress = new Faker<PickupAddress>()
        .CustomInstantiator(faker =>
            new PickupAddress
            {
                Country = faker.Address.Country(),
                State = "",
                ZipCode = faker.Address.ZipCode(),
                City = faker.Address.City(),
                Street = "",
                BuildingNumber = ""
            }).Generate();
        
        //Act
        Person sut = Person.Create(
            userIdentityId: _userIdentityId,
            firstName: firstName,
            lastName: lastName,
            email: email,
            phoneNumber: phone,
            address: address,
            defaultAdvertisementPickupAddress: pickupAddress,
            defaultAdvertisementContactInfo: ContactInfo
        );

        //Assert
        sut.Id.Should().NotBeEmpty();
        sut.FirstName.Should().Be(firstName);
        sut.LastName.Should().Be(lastName);
        sut.FullName.Should().Be($"{firstName} {lastName}");
        sut.Email.Should().Be(email);
        sut.PhoneNumber.Should().Be(phone);
        sut.Address.Should().NotBeNull();
        sut.Address.Country.Should().Be(address.Country);
        sut.Address.State.Should().BeNull();
        sut.Address.ZipCode.Should().Be(address.ZipCode);
        sut.Address.City.Should().Be(address.City);
        sut.Address.Street.Should().Be(address.Street);
        sut.Address.BuildingNumber.Should().Be(address.BuildingNumber);
        sut.DefaultAdvertisementsPickupAddress.Should().NotBeNull();
        sut.DefaultAdvertisementsPickupAddress.Country.Should().Be(pickupAddress.Country);
        sut.DefaultAdvertisementsPickupAddress.State.Should().BeNull();
        sut.DefaultAdvertisementsPickupAddress.ZipCode.Should().Be(pickupAddress.ZipCode);
        sut.DefaultAdvertisementsPickupAddress.City.Should().Be(pickupAddress.City);
        sut.DefaultAdvertisementsPickupAddress.Street.Should().BeNull();
        sut.DefaultAdvertisementsPickupAddress.BuildingNumber.Should().BeNull();
        sut.DefaultAdvertisementsContactInfo.Should().BeEquivalentTo(ContactInfo);
        sut.UserIdentityId.Should().Be(_userIdentityId);
        sut.CurrentRole.Should().Be(Person.Role.Regular);
    }
    
    [Theory]
    [InlineData("artur")]
    [InlineData("Artur")]
    [InlineData("ARTUR")]
    public void FirstNameSet_ShouldCapitalizeFirstLetter_WhenNotEmptyValueIsProvided(string firstName)
    {
        //Act
        Person sut = Person.Create(
            userIdentityId: _userIdentityId,
            firstName: firstName,
            lastName: DefaultProperLastName,
            email: DefaultProperEmail,
            phoneNumber: DefaultProperPhone,
            address: Address,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfo: ContactInfo
        );
        //Assert
        sut.FirstName.Should().Be("Artur");
    }

    [Theory]
    [InlineData("koniec")]
    [InlineData("Koniec")]
    [InlineData("Koniec-Początek")]
    public void LastNameSet_ShouldCapitalizeFirstLetter_WhenNotEmptyValueIsProvided(string lastName)
    {
        //Act
        Person sut = Person.Create(
            userIdentityId: _userIdentityId,
            firstName: DefaultProperFirstName,
            lastName: lastName,
            email: DefaultProperEmail,
            phoneNumber: DefaultProperPhone,
            address: Address,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfo: ContactInfo
        );
        
        //Assert
        sut.LastName.Should().StartWith("Koniec");
    }

    [Fact]
    public void FirstNameSet_ShouldThrowArgumentOutOfRangeException_WhenFirstNameExceedsMaxLength()
    {
        //Arrange
        string longFirstName = new('A', Person.Constraints.FirstNameMaxLength + 1);

        //Act
        Action creation = () => Person.Create(
            userIdentityId: _userIdentityId,
            firstName: longFirstName,
            lastName: DefaultProperLastName,
            email: DefaultProperEmail,
            phoneNumber: DefaultProperPhone,
            address: Address,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfo: ContactInfo
        );

        //Assert
        creation.Should().Throw<ArgumentOutOfRangeException>();
    }
    
    [Fact]
    public void LastNameSet_ShouldThrowArgumentOutOfRangeException_WhenLastNameExceedsMaxLength()
    {
        //Arrange
        string longLastName = new('B', Person.Constraints.LastNameMaxLength + 1);

        //Act
        Action creation = () => Person.Create(
            userIdentityId: _userIdentityId,
            firstName: DefaultProperFirstName,
            lastName: longLastName,
            email: DefaultProperEmail,
            phoneNumber: DefaultProperPhone,
            address: Address,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfo: ContactInfo
        );

        //Assert
        creation.Should().Throw<ArgumentOutOfRangeException>();
    }
    
    [Fact]
    public void FullNameGet_ShouldReturnProperlyConcatenatedName_WhenBothFirstNameAndLastNameAreProperlyProvided()
    {
        //Act
        Person sut = Person.Create(
            userIdentityId: _userIdentityId,
            firstName: DefaultProperFirstName,
            lastName: DefaultProperLastName,
            email: DefaultProperEmail,
            phoneNumber: DefaultProperPhone,
            address: Address,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfo: ContactInfo
        );

        //Assert
        sut.FullName.Should().Be("Artur Koniec");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void EmailSet_ShouldThrowEmailEmptyException_WhenEmptyEmailIsProvided(string emptyEmail)
    {
        //Act
        Action creation = () => Person.Create(
            userIdentityId: _userIdentityId,
            firstName: DefaultProperFirstName,
            lastName: DefaultProperLastName,
            email: emptyEmail,
            phoneNumber: DefaultProperPhone,
            address: Address,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfo: ContactInfo
        );

        //Assert
        creation.Should().Throw<ArgumentException>();
    }

    [Theory]
    [ClassData(typeof(InvalidEmailData))]
    public void EmailSet_ShouldThrowEmailInvalidFormatException_WhenInvalidEmailIsProvided(string invalidEmail)
    {
        //Act
        Action creation = () => Person.Create(
            userIdentityId: _userIdentityId,
            firstName: DefaultProperFirstName,
            lastName: DefaultProperLastName,
            email: invalidEmail,
            phoneNumber: DefaultProperPhone,
            address: Address,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfo: ContactInfo
        );

        //Assert
        creation.Should().Throw<FormatException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void PhoneNumberSet_ShouldThrowPhoneNumberEmptyException_WhenEmptyPhoneNumberIsProvided(
        string emptyPhoneNumber)
    {
        //Act
        Action creation = () => Person.Create(
            userIdentityId: _userIdentityId,
            firstName: DefaultProperFirstName,
            lastName: DefaultProperLastName,
            email: DefaultProperEmail,
            phoneNumber: emptyPhoneNumber,
            address: Address,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfo: ContactInfo
        );

        //Assert
        creation.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public void PhoneNumberSet_ShouldThrowArgumentOutOfRangeException_WhenPhoneNumberExceedsMaxLength()
    {
        //Arrange
        string longPhoneNumber = new('1', IContact.Constraints.PhoneNumberMaxLength + 1);

        //Act
        Action creation = () => Person.Create(
            userIdentityId: _userIdentityId,
            firstName: DefaultProperFirstName,
            lastName: DefaultProperLastName,
            email: DefaultProperEmail,
            phoneNumber: longPhoneNumber,
            address: Address,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfo: ContactInfo
        );

        //Assert
        creation.Should().Throw<ArgumentOutOfRangeException>();
    }
    
    [Fact]
    public void AddCat_ShouldAddCatToList_WhenValidCatIsProvided()
    {
        //Arrange
        Cat cat = new Faker<Cat>()
            .CustomInstantiator( faker =>
                Cat.Create(
                    calculator: Substitute.For<ICatPriorityCalculator>(),
                    personId: _userIdentityId,
                    name: faker.Person.FirstName,
                    medicalHelpUrgency: MedicalHelpUrgency.NoNeed,
                    ageCategory: AgeCategory.Baby,
                    behavior: Behavior.Friendly, 
                    healthStatus: HealthStatus.Good
                )).Generate();

        Person sut = Person.Create(
            userIdentityId: _userIdentityId,
            firstName: DefaultProperFirstName,
            lastName: DefaultProperLastName,
            email: DefaultProperEmail,
            phoneNumber: DefaultProperPhone,
            address: Address,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfo: ContactInfo
        );

        //Act
        sut.AddCat(cat);

        //Assert
        sut.Cats.Should().Contain(cat);
    }
    
    [Fact]
    public void RemoveCat_ShouldRemoveCatFromList_WhenValidCatIsProvided()
    {
        //Arrange
        Cat cat = new Faker<Cat>()
            .CustomInstantiator( faker =>
                Cat.Create(
                    calculator: Substitute.For<ICatPriorityCalculator>(),
                    personId: _userIdentityId,
                    name: faker.Person.FirstName,
                    medicalHelpUrgency: MedicalHelpUrgency.NoNeed,
                    ageCategory: AgeCategory.Baby,
                    behavior: Behavior.Friendly, 
                    healthStatus: HealthStatus.Good
                )).Generate();
        
        Person sut = Person.Create(
            userIdentityId: _userIdentityId,
            firstName: DefaultProperFirstName,
            lastName: DefaultProperLastName,
            email: DefaultProperEmail,
            phoneNumber: DefaultProperPhone,
            address: Address,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfo: ContactInfo
        );
        sut.AddCat(cat);

        //Act
        sut.RemoveCat(cat);

        //Assert
        sut.Cats.Should().NotContain(cat);
    }
    
    [Fact]
    public void UserIdentityIdSet_ShouldThrowArgumentException_WhenEmptyGuidIsProvided()
    {
        //Arrange
        Guid emptyGuid = Guid.Empty;

        //Act
        Action creation = () => Person.Create(
            userIdentityId: emptyGuid,
            firstName: DefaultProperFirstName,
            lastName: DefaultProperLastName,
            email: DefaultProperEmail,
            phoneNumber: DefaultProperPhone,
            address: Address,
            defaultAdvertisementPickupAddress: PickupAddress,
            defaultAdvertisementContactInfo: ContactInfo
        );

        //Assert
        creation.Should().Throw<ArgumentException>()
            .WithMessage("Provided empty guid. (Parameter 'UserIdentityId')");
    }
}
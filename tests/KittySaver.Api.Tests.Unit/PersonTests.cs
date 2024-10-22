using Bogus;
using FluentAssertions;
using KittySaver.Api.Features.Persons;
using KittySaver.Api.Shared.Domain.Entites;
using KittySaver.Api.Shared.Domain.Enums;
using KittySaver.Api.Shared.Domain.Services;
using KittySaver.Api.Shared.Domain.ValueObjects;
using KittySaver.Api.Shared.Infrastructure.Extensions;
using NSubstitute;
using Shared;
using Person = KittySaver.Api.Shared.Domain.Entites.Person;

namespace KittySaver.Api.Tests.Unit;

public class PersonTests
{
    private readonly Guid _userIdentityId = Guid.NewGuid();
    private const string DefaultProperFirstName = "artur";
    private const string DefaultProperLastName = "koniec";
    private const string DefaultProperEmail = "fake@fake.fake";
    private const string DefaultProperPhone = "+48111222333";

    private readonly Address _address = new Faker<Address>()
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
        const string firstName = "Artur";
        const string lastName = "Koniec";
        const string email = "koniecdev@gmail.com";
        const string phone = "535143330";
        
        //Act
        Person sut = new Person
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phone,
            UserIdentityId = _userIdentityId,
            Address = _address
        };

        //Assert
        sut.FirstName.Should().Be(firstName);
        sut.LastName.Should().Be(lastName);
        sut.FullName.Should().Be($"{firstName} {lastName}");
        sut.Email.Should().Be(email);
        sut.PhoneNumber.Should().Be(phone);
        sut.UserIdentityId.Should().Be(_userIdentityId);
        sut.CurrentRole.Should().Be(Person.Role.Regular);
    }
    
    [Theory]
    [InlineData("artur")]
    [InlineData("Artur")]
    [InlineData("ARTUR")]
    public void FirstNameSet_ShouldCapitalizeFirstLetter_WhenNotEmptyValueIsProvided(string firstName)
    {
        //Arrange && Act
        Person sut = new Person
        {
            FirstName = firstName,
            LastName = DefaultProperLastName,
            Email = DefaultProperEmail,
            PhoneNumber = DefaultProperPhone,
            UserIdentityId = _userIdentityId,
            Address = _address
        };
        //Assert
        sut.FirstName.Should().Be("Artur");
    }

    [Theory]
    [InlineData("koniec")]
    [InlineData("Koniec")]
    [InlineData("Koniec-Początek")]
    public void LastNameSet_ShouldCapitalizeFirstLetter_WhenNotEmptyValueIsProvided(string lastName)
    {
        //Arrange && Act
        Person sut = new Person
        {
            FirstName = DefaultProperFirstName,
            LastName = lastName,
            Email = DefaultProperEmail,
            PhoneNumber = DefaultProperPhone,
            UserIdentityId = _userIdentityId,
            Address = _address
        };
        //Assert
        sut.LastName.Should().StartWith("Koniec");
    }

    [Fact]
    public void FirstNameSet_ShouldThrowArgumentOutOfRangeException_WhenFirstNameExceedsMaxLength()
    {
        // Arrange
        Person? sut = null;
        string longFirstName = new('A', Person.Constraints.FirstNameMaxLength + 1);

        // Act
        Action creation = () =>
        {
            sut = new Person
            {
                FirstName = longFirstName,
                LastName = DefaultProperLastName,
                Email = DefaultProperEmail,
                PhoneNumber = DefaultProperPhone,
                UserIdentityId = _userIdentityId,
                Address = _address
            };
        };

        // Assert
        sut.Should().BeNull();
        creation.Should().Throw<ArgumentOutOfRangeException>();
    }
    
    [Fact]
    public void LastNameSet_ShouldThrowArgumentOutOfRangeException_WhenLastNameExceedsMaxLength()
    {
        // Arrange
        Person? sut = null;
        string longLastName = new('B', Person.Constraints.LastNameMaxLength + 1);

        // Act
        Action creation = () =>
        {
            sut = new Person
            {
                FirstName = DefaultProperFirstName,
                LastName = longLastName,
                Email = DefaultProperEmail,
                PhoneNumber = DefaultProperPhone,
                UserIdentityId = _userIdentityId,
                Address = _address
            };
        };

        // Assert
        sut.Should().BeNull();
        creation.Should().Throw<ArgumentOutOfRangeException>();
    }
    
    [Fact]
    public void FullNameGet_ShouldReturnProperlyConcatenatedName_WhenBothFirstNameAndLastNameAreProperlyProvided()
    {
        //Arrange && Act
        Person sut = new Person
        {
            FirstName = DefaultProperFirstName,
            LastName = DefaultProperLastName,
            Email = DefaultProperEmail,
            PhoneNumber = DefaultProperPhone,
            UserIdentityId = _userIdentityId,
            Address = _address
        };

        //Assert
        sut.FullName.Should().Be("Artur Koniec");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void EmailSet_ShouldThrowEmailEmptyException_WhenEmptyEmailIsProvided(string emptyEmail)
    {
        //Arrange
        Person sut = null!;

        //Act
        Action creation = () =>
        {
            sut = new Person
            {
                FirstName = DefaultProperFirstName,
                LastName = DefaultProperLastName,
                Email = emptyEmail,
                PhoneNumber = DefaultProperPhone,
                UserIdentityId = _userIdentityId,
                Address = _address
            };
        };

        //Assert
        sut.Should().BeNull();
        creation.Should().Throw<ArgumentException>();
    }

    [Theory]
    [ClassData(typeof(InvalidEmailData))]
    public void EmailSet_ShouldThrowEmailInvalidFormatException_WhenInvalidEmailIsProvided(string invalidEmail)
    {
        //Arrange
        Person sut = null!;

        //Act
        Action creation = () =>
        {
            sut = new Person
            {
                FirstName = DefaultProperFirstName,
                LastName = DefaultProperLastName,
                Email = invalidEmail,
                PhoneNumber = DefaultProperPhone,
                UserIdentityId = _userIdentityId,
                Address = _address
            };
        };

        //Assert
        sut.Should().BeNull();
        creation.Should().Throw<FormatException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void PhoneNumberSet_ShouldThrowPhoneNumberEmptyException_WhenEmptyPhoneNumberIsProvided(
        string emptyPhoneNumber)
    {
        //Arrange
        Person? sut = null;

        //Act
        Action creation = () =>
        {
            sut = new Person
            {
                FirstName = DefaultProperFirstName,
                LastName = DefaultProperLastName,
                Email = DefaultProperEmail,
                PhoneNumber = emptyPhoneNumber,
                UserIdentityId = _userIdentityId,
                Address = _address
            };
        };

        //Assert
        sut.Should().BeNull();
        creation.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public void PhoneNumberSet_ShouldThrowArgumentOutOfRangeException_WhenPhoneNumberExceedsMaxLength()
    {
        // Arrange
        Person? sut = null;
        string longPhoneNumber = new('1', Person.Constraints.PhoneNumberMaxLength + 1);

        // Act
        Action creation = () =>
        {
            sut = new Person
            {
                FirstName = DefaultProperFirstName,
                LastName = DefaultProperLastName,
                Email = DefaultProperEmail,
                PhoneNumber = longPhoneNumber,
                UserIdentityId = _userIdentityId,
                Address = _address
            };
        };

        // Assert
        sut.Should().BeNull();
        creation.Should().Throw<ArgumentOutOfRangeException>();
    }
    
    [Fact]
    public void AddCat_ShouldAddCatToList_WhenValidCatIsProvided()
    {
        // Arrange
        Faker<Cat> catGenerator = new Faker<Cat>()
                            .CustomInstantiator( faker =>
                                Cat.Create(
                                    Substitute.For<ICatPriorityCalculator>(),
                                    _userIdentityId,
                                    faker.Person.FirstName,
                                    MedicalHelpUrgency.NoNeed,
                                    AgeCategory.Baby,
                                    Behavior.Friendly, 
                                    HealthStatus.Good
                                ));
        Cat cat = catGenerator.Generate();
        
        Person sut = new()
        {
            FirstName = DefaultProperFirstName,
            LastName = DefaultProperLastName,
            Email = DefaultProperEmail,
            PhoneNumber = DefaultProperPhone,
            UserIdentityId = _userIdentityId,
            Address = _address
        };

        // Act
        sut.AddCat(cat);

        // Assert
        sut.Cats.Should().Contain(cat);
    }
    
    [Fact]
    public void RemoveCat_ShouldRemoveCatFromList_WhenValidCatIsProvided()
    {
        // Arrange
        Faker<Cat> catGenerator = new Faker<Cat>()
            .CustomInstantiator( faker =>
                Cat.Create(
                    Substitute.For<ICatPriorityCalculator>(),
                    _userIdentityId,
                    faker.Person.FirstName,
                    MedicalHelpUrgency.NoNeed,
                    AgeCategory.Baby,
                    Behavior.Friendly, 
                    HealthStatus.Good
                ));
        Cat cat = catGenerator.Generate();
        
        Person sut = new()
        {
            FirstName = DefaultProperFirstName,
            LastName = DefaultProperLastName,
            Email = DefaultProperEmail,
            PhoneNumber = DefaultProperPhone,
            UserIdentityId = _userIdentityId,
            Address = _address
        };
        sut.AddCat(cat);

        // Act
        sut.RemoveCat(cat);

        // Assert
        sut.Cats.Should().NotContain(cat);
    }
    
    [Fact]
    public void UserIdentityIdSet_ShouldThrowArgumentException_WhenEmptyGuidIsProvided()
    {
        // Arrange
        Person? sut = null;
        Guid emptyGuid = Guid.Empty;
        
        // Act
        Action creation = () =>
        {
            sut = new Person
            {
                FirstName = DefaultProperFirstName,
                LastName = DefaultProperLastName,
                Email = DefaultProperEmail,
                PhoneNumber = DefaultProperPhone,
                UserIdentityId = emptyGuid,
                Address = _address
            };
        };

        // Assert
        sut.Should().BeNull();
        creation.Should().Throw<ArgumentException>()
            .WithMessage("Provided empty guid. (Parameter 'UserIdentityId')");
    }
}
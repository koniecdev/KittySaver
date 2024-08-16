using FluentAssertions;
using KittySaver.Api.Shared.Domain.Entites;

namespace KittySaver.Api.Tests.Unit;

public class PersonTests
{
    private readonly Guid _userIdentityId = Guid.NewGuid();
    private const string DefaultProperFirstName = "artur";
    private const string DefaultProperLastName = "koniec";
    private const string DefaultProperEmail = "fake@fake.fake";
    private const string DefaultProperPhone = "+48111222333";

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
            UserIdentityId = _userIdentityId
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
            UserIdentityId = _userIdentityId
        };
        //Assert
        sut.LastName.Should().StartWith("Koniec");
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
            UserIdentityId = _userIdentityId
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
                UserIdentityId = _userIdentityId
            };
        };

        //Assert
        sut.Should().BeNull();
        creation.Should().Throw<ArgumentException>();
    }

    [Theory]
    [MemberData(nameof(InvalidEmails))]
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
                UserIdentityId = _userIdentityId
            };
        };

        //Assert
        sut.Should().BeNull();
        creation.Should().Throw<Person.Exceptions.Email.InvalidFormatException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void PhoneNumberSet_ShouldThrowPhoneNumberEmptyException_WhenEmptyPhoneNumberIsProvided(
        string emptyPhoneNumber)
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
                Email = DefaultProperEmail,
                PhoneNumber = emptyPhoneNumber,
                UserIdentityId = _userIdentityId
            };
        };

        //Assert
        sut.Should().BeNull();
        creation.Should().Throw<ArgumentException>();
    }

    public static IEnumerable<object[]> InvalidEmails =>
        new List<object[]>
        {
            new object[] { "plainaddress" },
            new object[] { "@missingusername.com" },
            new object[] { "missingatsign.com" },
            new object[] { "username@.com" },
            new object[] { "username@com" },
            new object[] { "username@missingtld." },
            new object[] { "username@.missingtld" },
            new object[] { "username@domain,com" },
            new object[] { "username@domain#com" },
            new object[] { "username@domain!com" },
            new object[] { "username@domain.com (Joe Smith)" },
            new object[] { "username@domain.com>" },
            new object[] { "user name@domain.com" },
            new object[] { "username@ domain.com" },
            new object[] { "username@domain .com" },
            new object[] { " username@domain.com" },
            new object[] { "username@domain.com " },
        };
}
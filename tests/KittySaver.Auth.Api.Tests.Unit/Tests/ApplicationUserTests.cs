using FluentAssertions;
using KittySaver.Auth.Api.Shared.Domain.Entites;

namespace KittySaver.Auth.Api.Tests.Unit.Tests;

public class ApplicationUserTests
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
        ApplicationUser sut = new ApplicationUser
        {
            FirstName = firstName,
            LastName = DefaultProperLastName,
            UserName = DefaultProperEmail,
            Email = DefaultProperEmail,
            PhoneNumber = DefaultProperPhone,
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
        ApplicationUser sut = new ApplicationUser
        {
            FirstName = DefaultProperFirstName,
            LastName = lastName,
            UserName = DefaultProperEmail,
            Email = DefaultProperEmail,
            PhoneNumber = DefaultProperPhone
        };
        //Assert
        sut.LastName.Should().StartWith("Koniec");
    }
    
    [Fact]
    public void FullNameGet_ShouldReturnProperlyConcatenatedName_WhenBothFirstNameAndLastNameAreProperlyProvided()
    {
        //Arrange && Act
        ApplicationUser sut = new ApplicationUser
        {
            FirstName = DefaultProperFirstName,
            LastName = DefaultProperLastName,
            Email = DefaultProperEmail,
            PhoneNumber = DefaultProperPhone
        };
        
        //Assert
        sut.FullName.Should().Be("Artur Koniec");
    }

    
    [Fact]
    public void EmailSet_ShouldSetProperlyBothEmailAndUsername_WhenNotMatchingUsernameIsProvided()
    {
        //Arrange && Act
        const string username = "invalid";
        
        ApplicationUser sut = new ApplicationUser
        {
            FirstName = DefaultProperFirstName,
            LastName = DefaultProperLastName,
            UserName = username,
            Email = DefaultProperEmail,
            PhoneNumber = DefaultProperPhone
        };
        
        ApplicationUser validApplicationUser = new()
        {
            FirstName = DefaultProperFirstName,
            LastName = DefaultProperLastName,
            UserName = DefaultProperEmail,
            Email = DefaultProperEmail,
            PhoneNumber = DefaultProperPhone
        };

        //Assert
        sut.Should()
            .BeEquivalentTo(validApplicationUser,
                options =>
                {
                    return options
                        .Excluding(m => m.Id)
                        .Excluding(m=>m.ConcurrencyStamp);
                });
    }
    
    [Fact]
    public void EmailSet_ShouldSetBothEmailAndUsername_WhenNoUsernameIsProvided()
    {
        //Arrange && Act
        ApplicationUser sut = new ApplicationUser
        {
            FirstName = DefaultProperFirstName,
            LastName = DefaultProperLastName,
            Email = DefaultProperEmail,
            PhoneNumber = DefaultProperPhone
        };
        
        //Assert
        sut.UserName.Should().Be(DefaultProperEmail);
    }
    
    [Fact]
    public void EmailSet_ShouldSetBothEmailAndUsername_WhenMatchingUsernameIsProvided()
    {
        //Arrange && Act
        ApplicationUser sut = new ApplicationUser
        {
            FirstName = DefaultProperFirstName,
            LastName = DefaultProperLastName,
            UserName = DefaultProperEmail,
            Email = DefaultProperEmail,
            PhoneNumber = DefaultProperPhone
        };
        
        //Assert
        sut.UserName.Should().Be(DefaultProperEmail);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void EmailSet_ShouldThrowEmailEmptyException_WhenEmptyEmailIsProvided(string emptyEmail)
    {
        //Arrange
        ApplicationUser sut = null!;
        
        //Act
        Action creation = () =>
        {
            sut = new ApplicationUser
            {
                FirstName = DefaultProperFirstName,
                LastName = DefaultProperLastName,
                Email = emptyEmail,
                PhoneNumber = DefaultProperPhone
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
        ApplicationUser sut = null!;
        
        //Act
        Action creation = () =>
        {
            sut = new ApplicationUser
            {
                FirstName = DefaultProperFirstName,
                LastName = DefaultProperLastName,
                Email = invalidEmail,
                PhoneNumber = DefaultProperPhone
            };
        };
        
        //Assert
        sut.Should().BeNull();
        creation.Should().Throw<ApplicationUser.Exceptions.Email.InvalidFormatException>();
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void PhoneNumberSet_ShouldThrowPhoneNumberEmptyException_WhenEmptyPhoneNumberIsProvided(string emptyPhoneNumber)
    {
        //Arrange
        ApplicationUser sut = null!;
        
        //Act
        Action creation = () =>
        {
            sut = new ApplicationUser
            {
                FirstName = DefaultProperFirstName,
                LastName = DefaultProperLastName,
                Email = DefaultProperEmail,
                PhoneNumber = emptyPhoneNumber
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
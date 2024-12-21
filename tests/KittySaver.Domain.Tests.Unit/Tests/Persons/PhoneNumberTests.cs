using FluentAssertions;
using KittySaver.Domain.ValueObjects;

namespace KittySaver.Domain.Tests.Unit.Tests.Persons;

public class PhoneNumberTests
{
    [Fact]
    public void PhoneNumberCreate_ShouldCreateSuccessfully_WhenCorrectValueIsProvided()
    {
        //Assign
        const string properPhoneNumber = "123321123";
        
        //Act
        PhoneNumber phoneNumber = PhoneNumber.Create(properPhoneNumber);
        string phoneNumberAsString = phoneNumber.ToString();
        
        //Assert
        phoneNumber.Value.Should().Be(properPhoneNumber);
        phoneNumberAsString.Should().Be(properPhoneNumber);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void PhoneNumberCreate_ShouldThrowPhoneNumberEmptyException_WhenEmptyPhoneNumberIsProvided(string emptyPhoneNumberValue)
    {
        //Act
        Action creation = () => PhoneNumber.Create(emptyPhoneNumberValue);

        //Assert
        creation.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public void PhoneNumberCreate_ShouldThrowArgumentOutOfRangeException_WhenPhoneNumberExceedsMaxLength()
    {
        //Arrange
        string longPhoneNumberValue = new('1', PhoneNumber.MaxLength + 1);
        
        //Act
        Action creation = () => PhoneNumber.Create(longPhoneNumberValue);

        //Assert
        creation.Should().Throw<ArgumentOutOfRangeException>();
    }
}
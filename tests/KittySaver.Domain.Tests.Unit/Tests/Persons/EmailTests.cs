using FluentAssertions;
using KittySaver.Domain.ValueObjects;
using Shared;

namespace KittySaver.Domain.Tests.Unit.Tests.Persons;

public class EmailTests
{
    [Fact]
    public void EmailCreate_ShouldCreateSuccessfully_WhenCorrectValueIsProvided()
    {
        //Assign
        const string properEmail = "properemail@properemail.com";
        
        //Act
        Email email = Email.Create(properEmail);
        string emailAsString = email.ToString();
        
        //Assert
        email.Value.Should().Be(properEmail);
        emailAsString.Should().Be(properEmail);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void EmailCreate_ShouldThrowEmailEmptyException_WhenEmptyEmailIsProvided(string emptyEmailValue)
    {
        //Act
        Action creation = () => Email.Create(emptyEmailValue);

        //Assert
        creation.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public void EmailCreate_ShouldThrowArgumentOutOfRangeException_WhenTooLongEmailIsProvided()
    {
        //Arrange
        string longFirstNameValue = new('A', Email.MaxLength + 1);
        
        //Act
        Action creation = () => Email.Create(longFirstNameValue);

        //Assert
        creation.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [ClassData(typeof(InvalidEmailData))]
    public void EmailCreate_ShouldThrowArgumentException_WhenInvalidEmailIsProvided(string invalidEmailValue)
    {
        //Act
        Action creation = () => Email.Create(invalidEmailValue);

        //Assert
        creation.Should().Throw<ArgumentException>();
    }
}
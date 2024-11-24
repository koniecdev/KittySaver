using FluentAssertions;
using KittySaver.Domain.Persons;

namespace KittySaver.Domain.Tests.Unit.Persons;

public class LastNameTests
{
    [Fact]
    public void LastNameCreate_ShouldCreateSuccessfully_WhenCorrectValueIsProvided()
    {
        //Assign
        const string properLastName = "ProperlastName";
        
        //Act
        LastName lastName = LastName.Create(properLastName);
        string lastNameAsString = lastName.ToString();
        
        //Assert
        lastName.Value.Should().Be(properLastName);
        lastNameAsString.Should().Be(properLastName);
    }
    
    [Fact]
    public void LastNameCreate_ShouldThrowArgumentOutOfRangeException_WhenLastNameExceedsMaxLength()
    {
        //Arrange
        string longLastNameValue = new('B', LastName.MaxLength + 1);

        //Act
        Action creation = () => LastName.Create(longLastNameValue);

        //Assert
        creation.Should().Throw<ArgumentOutOfRangeException>();
    }
}
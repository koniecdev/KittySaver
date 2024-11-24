using FluentAssertions;
using KittySaver.Domain.Persons;
using KittySaver.Domain.ValueObjects;

namespace KittySaver.Domain.Tests.Unit.Cats;

public class DescriptionTests
{
    [Fact]
    public void DescriptionCreate_ShouldCreateSuccessfully_WhenCorrectValueIsProvided()
    {
        //Assign
        const string properDescription = "ProperDescription";
        
        //Act
        Description description = Description.Create(properDescription);
        string descriptionAsString = description.ToString();
        
        //Assert
        description.Value.Should().Be(properDescription);
        descriptionAsString.Should().Be(properDescription);
    }
    
    [Fact]
    public void AdditionalRequirementsSet_ShouldThrowArgumentOutOfRangeException_WhenExceedsMaxLength()
    {
        //Arrange
        string longRequirements = new string('R', Description.MaxLength + 1);

        //Act
        Action createCat = () => CatName.Create(longRequirements);
        
        //Assert
        createCat.Should().Throw<ArgumentOutOfRangeException>();
    }
}
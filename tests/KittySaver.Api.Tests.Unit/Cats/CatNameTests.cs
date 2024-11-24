using Bogus;
using FluentAssertions;
using KittySaver.Api.Shared.Domain.Persons;
using KittySaver.Api.Shared.Domain.ValueObjects;
using Person = KittySaver.Api.Shared.Domain.Persons.Person;

namespace KittySaver.Api.Tests.Unit.Cats;

public class CatNameTests
{
    [Fact]
    public void CatNameCreate_ShouldCreateSuccessfully_WhenCorrectValueIsProvided()
    {
        //Assign
        const string properCatName = "ProperCatName";
        
        //Act
        CatName catName = CatName.Create(properCatName);
        string catNameAsString = catName.ToString();
        
        //Assert
        catName.Value.Should().Be(properCatName);
        catNameAsString.Should().Be(properCatName);
    }
    
    [Fact]
    public void CatNameCreate_ShouldThrowArgumentOutOfRangeException_WhenNameExceedsMaxLength()
    {
        //Arrange
        string longName = new('C', CatName.MaxLength + 1);

        //Act
        Action createCat = () => CatName.Create(longName);

        //Assert
        createCat.Should().Throw<ArgumentOutOfRangeException>();
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void CatNameCreate_ShouldThrowArgumentException_WhenNameIsNullOrWhitespace(string invalidName)
    {
        //Act
        Action createCat = () => CatName.Create(invalidName);

        //Assert
        createCat.Should().Throw<ArgumentException>();
    }
}
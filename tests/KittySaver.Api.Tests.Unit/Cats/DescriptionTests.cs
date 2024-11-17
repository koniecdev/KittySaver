using Bogus;
using FluentAssertions;
using KittySaver.Api.Shared.Domain.Persons;
using KittySaver.Api.Shared.Domain.ValueObjects;
using Person = KittySaver.Api.Shared.Domain.Persons.Person;

namespace KittySaver.Api.Tests.Unit.Cats;

public class DescriptionTests
{
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
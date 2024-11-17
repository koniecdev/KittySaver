using Bogus;
using FluentAssertions;
using KittySaver.Api.Shared.Domain.Common.Primitives.Enums;
using KittySaver.Api.Shared.Domain.Persons;
using KittySaver.Api.Shared.Domain.ValueObjects;
using NSubstitute;
using Shared;
using Person = KittySaver.Api.Shared.Domain.Persons.Person;

namespace KittySaver.Api.Tests.Unit.Persons;

public class EmailTests
{
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
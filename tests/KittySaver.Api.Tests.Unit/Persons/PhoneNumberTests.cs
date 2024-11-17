using Bogus;
using FluentAssertions;
using KittySaver.Api.Shared.Domain.Common.Primitives.Enums;
using KittySaver.Api.Shared.Domain.Persons;
using KittySaver.Api.Shared.Domain.ValueObjects;
using NSubstitute;
using Shared;
using Person = KittySaver.Api.Shared.Domain.Persons.Person;

namespace KittySaver.Api.Tests.Unit.Persons;

public class PhoneNumberTests
{
    
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
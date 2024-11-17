using Bogus;
using FluentAssertions;
using KittySaver.Api.Shared.Domain.Common.Primitives.Enums;
using KittySaver.Api.Shared.Domain.Persons;
using KittySaver.Api.Shared.Domain.ValueObjects;
using NSubstitute;
using Shared;
using Person = KittySaver.Api.Shared.Domain.Persons.Person;

namespace KittySaver.Api.Tests.Unit.Persons;

public class LastNameTests
{
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
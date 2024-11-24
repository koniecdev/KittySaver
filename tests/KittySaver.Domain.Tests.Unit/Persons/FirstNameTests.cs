using Bogus;
using FluentAssertions;
using KittySaver.Domain.Persons;
using KittySaver.Domain.ValueObjects;
using Person = KittySaver.Domain.Persons.Person;

namespace KittySaver.Domain.Tests.Unit.Persons;

public class FirstNameTests
{
    private readonly Guid _userIdentityId = Guid.NewGuid();
    private readonly LastName _defaultProperLastName = LastName.Create("Koniec");
    private readonly Email _defaultProperEmail = Email.Create("fake@fake.fake");
    private readonly PhoneNumber _defaultProperPhone = PhoneNumber.Create("+48111222333");

    private static readonly Address Address = new Faker<Address>()
        .CustomInstantiator(faker =>
            new Address
            {
                Country = faker.Address.Country(),
                State = faker.Address.State(),
                ZipCode = faker.Address.ZipCode(),
                City = faker.Address.City(),
                Street = faker.Address.StreetName(),
                BuildingNumber = faker.Address.BuildingNumber()
            }).Generate();
    
   
    [Fact]
    public void FirstNameCreate_ShouldCreateSuccessfully_WhenCorrectValueIsProvided()
    {
        //Assign
        const string properFirstName = "ProperfirstName";
        
        //Act
        FirstName firstName = FirstName.Create(properFirstName);
        string firstNameAsString = firstName.ToString();
        
        //Assert
        firstName.Value.Should().Be(properFirstName);
        firstNameAsString.Should().Be(properFirstName);
    }

    
    [Theory]
    [InlineData("artur")]
    [InlineData("Artur")]
    public void FirstNameCreate_ShouldCapitalizeFirstLetter_WhenNotEmptyValueIsProvided(string firstNameValue)
    {
        //Act
        FirstName firstName = FirstName.Create(firstNameValue);
        Person sut = Person.Create(
            userIdentityId: _userIdentityId,
            firstName: firstName,
            lastName: _defaultProperLastName,
            email: _defaultProperEmail,
            phoneNumber: _defaultProperPhone,
            residentalAddress: Address,
            defaultAdvertisementPickupAddress: Address,
            defaultAdvertisementContactInfoEmail: _defaultProperEmail,
            defaultAdvertisementContactInfoPhoneNumber: _defaultProperPhone
        );
        //Assert
        sut.FirstName.Value.Should().Be("Artur");
    }

    [Fact]
    public void FirstNameCreate_ShouldThrowArgumentOutOfRangeException_WhenValueExceedsMaxLength()
    {
        //Arrange
        string longFirstNameValue = new('A', FirstName.MaxLength + 1);
        
        //Act
        Action creation = () => FirstName.Create(longFirstNameValue);

        //Assert
        creation.Should().Throw<ArgumentOutOfRangeException>();
    }
}
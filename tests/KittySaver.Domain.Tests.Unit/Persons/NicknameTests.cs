using Bogus;
using FluentAssertions;
using KittySaver.Domain.Persons;
using KittySaver.Domain.ValueObjects;
using Person = KittySaver.Domain.Persons.Person;

namespace KittySaver.Domain.Tests.Unit.Persons;

public class NicknameTests
{
    private readonly Guid _userIdentityId = Guid.NewGuid();
    private readonly Email _defaultProperEmail = Email.Create("fake@fake.fake");
    private readonly PhoneNumber _defaultProperPhone = PhoneNumber.Create("+48111222333");

    private static readonly Address Address = new Faker<Address>()
        .CustomInstantiator(faker =>
            Address.Create(
                country: faker.Address.Country(),
                state: faker.Address.State(),
                zipCode: faker.Address.ZipCode(),
                city: faker.Address.City(),
                street: faker.Address.StreetName(),
                buildingNumber: faker.Address.BuildingNumber()
            )).Generate();
   
    [Fact]
    public void FirstNameCreate_ShouldCreateSuccessfully_WhenCorrectValueIsProvided()
    {
        //Assign
        const string properFirstName = "ProperfirstName";
        
        //Act
        Nickname nickname = Nickname.Create(properFirstName);
        string firstNameAsString = nickname.ToString();
        
        //Assert
        nickname.Value.Should().Be(properFirstName);
        firstNameAsString.Should().Be(properFirstName);
    }

    
    [Theory]
    [InlineData("artur")]
    [InlineData("Artur")]
    public void FirstNameCreate_ShouldCapitalizeFirstLetter_WhenNotEmptyValueIsProvided(string firstNameValue)
    {
        //Act
        Nickname nickname = Nickname.Create(firstNameValue);
        Person sut = Person.Create(
            userIdentityId: _userIdentityId,
            nickname: nickname,
            email: _defaultProperEmail,
            phoneNumber: _defaultProperPhone,
            defaultAdvertisementPickupAddress: Address,
            defaultAdvertisementContactInfoEmail: _defaultProperEmail,
            defaultAdvertisementContactInfoPhoneNumber: _defaultProperPhone
        );
        //Assert
        sut.Nickname.Value.Should().Be("Artur");
    }

    [Fact]
    public void FirstNameCreate_ShouldThrowArgumentOutOfRangeException_WhenValueExceedsMaxLength()
    {
        //Arrange
        string longFirstNameValue = new('A', Nickname.MaxLength + 1);
        
        //Act
        Action creation = () => Nickname.Create(longFirstNameValue);

        //Assert
        creation.Should().Throw<ArgumentOutOfRangeException>();
    }
}
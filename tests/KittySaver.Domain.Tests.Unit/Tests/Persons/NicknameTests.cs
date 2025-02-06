using Bogus;
using FluentAssertions;
using KittySaver.Domain.Persons;
using KittySaver.Domain.ValueObjects;
using Person = KittySaver.Domain.Persons.Person;

namespace KittySaver.Domain.Tests.Unit.Tests.Persons;

public class NicknameTests
{
    private readonly Guid _userIdentityId = Guid.NewGuid();
    private readonly Email _defaultProperEmail = Email.Create("fake@fake.fake");
    private readonly PhoneNumber _defaultProperPhone = PhoneNumber.Create("+48111222333");

    private static readonly Address Address = new Faker<Address>()
        .CustomInstantiator(faker =>
            Address.Create(
                country: faker.Address.CountryCode(),
                state: faker.Address.State(),
                zipCode: faker.Address.ZipCode(),
                city: faker.Address.City(),
                street: faker.Address.StreetName(),
                buildingNumber: faker.Address.BuildingNumber()
            )).Generate();
   
    [Fact]
    public void NicknameCreate_ShouldCreateSuccessfully_WhenCorrectValueIsProvided()
    {
        //Assign
        const string properNickname = "Propernickname";
        
        //Act
        Nickname nickname = Nickname.Create(properNickname);
        string nicknameAsString = nickname.ToString();
        
        //Assert
        nickname.Value.Should().Be(properNickname);
        nicknameAsString.Should().Be(properNickname);
    }

    
    [Theory]
    [InlineData("artur")]
    [InlineData("Artur")]
    public void NicknameCreate_ShouldCapitalizeFirstLetter_WhenNotEmptyValueIsProvided(string nicknameValue)
    {
        //Act
        Nickname nickname = Nickname.Create(nicknameValue);
        Person sut = Person.Create(
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
    public void NicknameCreate_ShouldThrowArgumentOutOfRangeException_WhenValueExceedsMaxLength()
    {
        //Arrange
        string longNicknameValue = new('A', Nickname.MaxLength + 1);
        
        //Act
        Action creation = () => Nickname.Create(longNicknameValue);

        //Assert
        creation.Should().Throw<ArgumentOutOfRangeException>();
    }
}
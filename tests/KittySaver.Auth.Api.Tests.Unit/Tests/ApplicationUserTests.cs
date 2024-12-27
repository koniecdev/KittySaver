using Bogus;
using FluentAssertions;
using KittySaver.Auth.Api.Features.ApplicationUsers;
using KittySaver.Auth.Api.Shared.Domain.Entites;
using Shared;

namespace KittySaver.Auth.Api.Tests.Unit.Tests;

public class ApplicationUserTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void EmailSet_ShouldThrowEmailEmptyException_WhenEmptyEmailIsProvided(string emptyEmail)
    {
        //Act
        Action creation = () =>
        {
            new Faker<ApplicationUser>()
                .CustomInstantiator( faker =>
                    new ApplicationUser
                    {
                        UserName = faker.Person.UserName,
                        Email = emptyEmail,
                        PhoneNumber = faker.Person.Phone,
                        DefaultAdvertisementPickupAddressCountry = faker.Address.CountryCode(),
                        DefaultAdvertisementPickupAddressState = faker.Address.State(),
                        DefaultAdvertisementPickupAddressZipCode = faker.Address.ZipCode(),
                        DefaultAdvertisementPickupAddressCity = faker.Address.City(),
                        DefaultAdvertisementPickupAddressStreet = faker.Address.StreetName(),
                        DefaultAdvertisementPickupAddressBuildingNumber = faker.Address.BuildingNumber(),
                        DefaultAdvertisementContactInfoEmail = faker.Person.Email,
                        DefaultAdvertisementContactInfoPhoneNumber = faker.Person.Phone
                    }).Generate();
        };
        
        //Assert
        creation.Should().Throw<ArgumentException>();
    }
    
    [Theory]
    [ClassData(typeof(InvalidEmailData))]
    public void EmailSet_ShouldThrowEmailInvalidFormatException_WhenInvalidEmailIsProvided(string invalidEmail)
    {
        //Act
        Action creation = () =>
        {
            ApplicationUser sut = new Faker<ApplicationUser>()
                .CustomInstantiator( faker =>
                    new ApplicationUser
                    {
                        UserName = faker.Person.UserName,
                        Email = invalidEmail,
                        PhoneNumber = faker.Person.Phone,
                        DefaultAdvertisementPickupAddressCountry = faker.Address.CountryCode(),
                        DefaultAdvertisementPickupAddressState = faker.Address.State(),
                        DefaultAdvertisementPickupAddressZipCode = faker.Address.ZipCode(),
                        DefaultAdvertisementPickupAddressCity = faker.Address.City(),
                        DefaultAdvertisementPickupAddressStreet = faker.Address.StreetName(),
                        DefaultAdvertisementPickupAddressBuildingNumber = faker.Address.BuildingNumber(),
                        DefaultAdvertisementContactInfoEmail = faker.Person.Email,
                        DefaultAdvertisementContactInfoPhoneNumber = faker.Person.Phone
                    }).Generate();
        };
        
        //Assert
        creation.Should().Throw<ApplicationUser.Exceptions.Email.InvalidFormatException>();
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void PhoneNumberSet_ShouldThrowPhoneNumberEmptyException_WhenEmptyPhoneNumberIsProvided(string emptyPhoneNumber)
    {
        //Act
        Action creation = () =>
        {
            new Faker<ApplicationUser>()
                .CustomInstantiator( faker =>
                    new ApplicationUser
                    {
                        UserName = faker.Person.UserName,
                        Email = emptyPhoneNumber,
                        PhoneNumber = faker.Person.Phone,
                        DefaultAdvertisementPickupAddressCountry = faker.Address.CountryCode(),
                        DefaultAdvertisementPickupAddressState = faker.Address.State(),
                        DefaultAdvertisementPickupAddressZipCode = faker.Address.ZipCode(),
                        DefaultAdvertisementPickupAddressCity = faker.Address.City(),
                        DefaultAdvertisementPickupAddressStreet = faker.Address.StreetName(),
                        DefaultAdvertisementPickupAddressBuildingNumber = faker.Address.BuildingNumber(),
                        DefaultAdvertisementContactInfoEmail = faker.Person.Email,
                        DefaultAdvertisementContactInfoPhoneNumber = faker.Person.Phone
                    }).Generate();
        };
        
        //Assert
        creation.Should().Throw<ArgumentException>();
    }
    
}
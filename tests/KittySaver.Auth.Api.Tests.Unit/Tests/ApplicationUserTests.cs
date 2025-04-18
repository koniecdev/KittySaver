﻿using Bogus;
using FluentAssertions;
using KittySaver.Auth.Api.Shared.Domain.Entites;
using KittySaver.Tests.Shared;
using Microsoft.SqlServer.Server;

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
                        PhoneNumber = faker.Person.Phone
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
            _ = new Faker<ApplicationUser>()
                .CustomInstantiator( faker =>
                    new ApplicationUser
                    {
                        UserName = faker.Person.UserName,
                        Email = invalidEmail,
                        PhoneNumber = faker.Person.Phone
                    }).Generate();
        };
        
        //Assert
        creation.Should().Throw<FormatException>();
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
                        PhoneNumber = faker.Person.Phone
                    }).Generate();
        };
        
        //Assert
        creation.Should().Throw<ArgumentException>();
    }
    
}
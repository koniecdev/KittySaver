﻿using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Shared.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using KittySaver.Tests.Shared;

namespace KittySaver.Auth.Api.Tests.Integration.Tests.ApplicationUser;

[Collection("AuthApi")]
public class RegisterApplicationUserEndpointTests(KittySaverAuthApiFactory appFactory)
{
    private readonly HttpClient _httpClient = appFactory.CreateClient();

    private readonly Faker<RegisterRequest> _createApplicationUserRequestGenerator =
        new Faker<RegisterRequest>()
            .CustomInstantiator( faker =>
                new RegisterRequest(
                    UserName: faker.Person.FirstName,
                    Email: faker.Person.Email,
                    PhoneNumber: faker.Person.Phone,
                    Password: "Default1234%"
                ));

    [Fact]
    public async Task Register_ShouldRegisterUser_WhenValidDataIsProvided()
    {
        //Arrange
        RegisterRequest request = _createApplicationUserRequestGenerator.Generate();
        
        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/application-users", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        ApiResponses.CreatedWithIdResponse? registerResponse = await response.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>();
        registerResponse.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/api/v1/application-users/{registerResponse?.Id}");
        registerResponse!.Id.Should().NotBeEmpty();
    }
    
    [Theory]
    [ClassData(typeof(InvalidEmailData))]
    public async Task Register_ShouldReturnBadRequest_WhenInvalidEmailIsProvided(string email)
    {
        //Arrange
        RegisterRequest request = new Faker<RegisterRequest>()
            .CustomInstantiator( faker =>
                new RegisterRequest(
                    UserName: faker.Person.FirstName,
                    Email: email,
                    PhoneNumber: faker.Person.Phone,
                    Password: "Default1234%"
                ));
        
        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/application-users", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors["Email"][0].Should().Be("'Email' is not in the correct format.");
    }
    
    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenEmptyDataIsProvided()
    {
        //Arrange
        RegisterRequest request = new(
            UserName: "",
            Email: "",
            PhoneNumber: "",
            Password: ""
        );
        
        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/application-users", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors["UserName"][0].Should().Be("'User Name' must not be empty.");
        validationProblemDetails.Errors["PhoneNumber"][0].Should().Be("'Phone Number' must not be empty.");
        validationProblemDetails.Errors["Email"][0].Should().Be("'Email' must not be empty.");
        validationProblemDetails.Errors["Password"][0].Should().Be("'Password' must not be empty.");
    }
    
    [Theory]
    [InlineData("pass")]
    [InlineData("password")]
    [InlineData("Password")]
    [InlineData("PASSWORD")]
    [InlineData("Pass1")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public async Task Register_ShouldReturnBadRequest_WhenInvalidPasswordIsProvided(string password)
    {
        //Arrange
        RegisterRequest request = new Faker<RegisterRequest>()
            .CustomInstantiator( faker =>
                new RegisterRequest(
                    UserName: faker.Person.FirstName,
                    Email: faker.Person.Email,
                    PhoneNumber: faker.Person.Phone,
                    Password: password
                ));
        
        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/application-users", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors["Password"][0].Should().StartWith("'Password' is not in the correct format.");
    }
    
    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenDuplicatedRequestOccur()
    {
        //Arrange
        RegisterRequest request = _createApplicationUserRequestGenerator.Generate();
        _ = await _httpClient.PostAsJsonAsync("api/v1/application-users", request);
        
        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/application-users", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors["Email"][0].Should().StartWith("Email is already used by another user.");
    }
}


using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Features.Persons;
using KittySaver.Auth.Api.Features.ApplicationUsers;
using Microsoft.AspNetCore.Mvc;

namespace KittySaver.Auth.Api.Tests.Integration.Tests;

[Collection("AuthApi")]
public class RegisterEndpointTests(KittySaverAuthApiFactory appFactory)
{
    private readonly HttpClient _httpClient = appFactory.CreateClient();

    private readonly Faker<Register.RegisterRequest> _createPersonRequestGenerator =
        new Faker<Register.RegisterRequest>()
            .CustomInstantiator( faker =>
                new Register.RegisterRequest(
                    FirstName: faker.Person.FirstName,
                    LastName: faker.Person.LastName,
                    Email: faker.Person.Email,
                    PhoneNumber: faker.Person.Phone,
                    Password: "Default1234%"
                ));

    [Fact]
    public async Task Register_ShouldRegisterUser_WhenValidDataIsProvided()
    {
        //Arrange
        Register.RegisterRequest request = _createPersonRequestGenerator.Generate();
        
        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/application-users/register", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        ApiResponses.CreatedWithIdResponse? registerResponse = await response.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>();
        response.Headers.Location!.ToString().Should().Contain($"/api/v1/application-users/{registerResponse?.Id}");
        registerResponse?.Id.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task Register_ShouldReturnValidationProblemDetails_WhenInvalidEmailIsProvided()
    {
        //Arrange
        Register.RegisterRequest request = new Faker<Register.RegisterRequest>()
            .CustomInstantiator( faker =>
                new Register.RegisterRequest(
                    FirstName: faker.Person.FirstName,
                    LastName: faker.Person.LastName,
                    Email: "ReallyIncorrectEmail",
                    PhoneNumber: faker.Person.Phone,
                    Password: "Default1234%"
                ));
        
        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/application-users/register", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails?.Status.Should().Be(400);
        validationProblemDetails?.Errors["Email"][0].Should().Be("'Email' is not in the correct format.");
    }
    
    [Fact]
    public async Task Register_ShouldReturnValidationProblemDetails_WhenEmptyDataIsProvided()
    {
        //Arrange
        Register.RegisterRequest request = new(
            FirstName: "",
            LastName: "",
            Email: "",
            PhoneNumber: "",
            Password: ""
        );
        
        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/application-users/register", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails?.Status.Should().Be(400);
        validationProblemDetails?.Errors["FirstName"][0].Should().Be("'First Name' must not be empty.");
        validationProblemDetails?.Errors["LastName"][0].Should().Be("'Last Name' must not be empty.");
        validationProblemDetails?.Errors["PhoneNumber"][0].Should().Be("'Phone Number' must not be empty.");
        validationProblemDetails?.Errors["Email"][0].Should().Be("'Email' must not be empty.");
        validationProblemDetails?.Errors["Password"][0].Should().Be("'Password' must not be empty.");
    }
    
    [Theory]
    [InlineData("pass")]
    [InlineData("password")]
    [InlineData("Password")]
    [InlineData("PASSWORD")]
    [InlineData("Pass1")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public async Task Register_ShouldReturnValidationProblemDetails_WhenInvalidPasswordIsProvided(string password)
    {
        //Arrange
        Register.RegisterRequest request = new Faker<Register.RegisterRequest>()
            .CustomInstantiator( faker =>
                new Register.RegisterRequest(
                    FirstName: faker.Person.FirstName,
                    LastName: faker.Person.LastName,
                    Email: faker.Person.Email,
                    PhoneNumber: faker.Person.Phone,
                    Password: password
                ));
        
        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/application-users/register", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails?.Status.Should().Be(400);
        validationProblemDetails?.Errors["Password"][0].Should().StartWith("'Password' is not in the correct format.");
    }
    
    [Fact]
    public async Task Register_ShouldRegisterUser_WhenValidPasswordIsProvided()
    {
        //Arrange
        Register.RegisterRequest request = new Faker<Register.RegisterRequest>()
            .CustomInstantiator( faker =>
                new Register.RegisterRequest(
                    FirstName: faker.Person.FirstName,
                    LastName: faker.Person.LastName,
                    Email: faker.Person.Email,
                    PhoneNumber: faker.Person.Phone,
                    Password: "Default12345#"
                ));
        
        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/application-users/register", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        ApiResponses.CreatedWithIdResponse? registerResponse = await response.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>();
        response.Headers.Location!.ToString().Should().Contain($"/api/v1/application-users/{registerResponse?.Id}");
        registerResponse?.Id.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task Register_ShouldReturnValidationProblemDetails_WhenDuplicatedRequestOccur()
    {
        //Arrange
        Register.RegisterRequest request = _createPersonRequestGenerator.Generate();
        _ = await _httpClient.PostAsJsonAsync("api/v1/application-users/register", request);
        
        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/application-users/register", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails?.Status.Should().Be(400);
        validationProblemDetails?.Errors["Email"][0].Should().StartWith("Email is already registered in database");
    }
    [Fact]
    public async Task Register_ShouldReturnProblemDetails_WhenApiIsDown()
    {
        //Arrange
        Register.RegisterRequest request = new Faker<Register.RegisterRequest>()
            .CustomInstantiator( faker =>
                new Register.RegisterRequest(
                    FirstName: faker.Person.FirstName,
                    LastName: faker.Person.LastName,
                    Email: "apiFactoryWill@Throw.IntServErr",
                    PhoneNumber: faker.Person.Phone,
                    Password: "Default12345#"
                ));
        
        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/application-users/register", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        ProblemDetails? problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails?.Status.Should().Be(500);
    }
}


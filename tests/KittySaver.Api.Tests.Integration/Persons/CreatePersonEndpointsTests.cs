using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Features.Persons;
using KittySaver.Api.Features.Persons.Contracts;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace KittySaver.Api.Tests.Integration.Persons;

[Collection("Api")]
public class CreatePersonEndpointsTests(KittySaverApiFactory appFactory)
{
    private readonly HttpClient _httpClient = appFactory.CreateClient();

    private readonly Faker<CreatePerson.CreatePersonRequest> _createPersonRequestGenerator =
        new Faker<CreatePerson.CreatePersonRequest>()
            .CustomInstantiator( faker =>
                new CreatePerson.CreatePersonRequest(
                    FirstName: faker.Person.FirstName,
                    LastName: faker.Person.LastName,
                    Email: faker.Person.Email,
                    PhoneNumber: faker.Person.Phone,
                    UserIdentityId: Guid.NewGuid()
                ));
    
    [Fact]
    public async Task CreatePerson_ShouldCreatePerson_WhenValidDataIsProvided()
    {
        //Arrange
        CreatePerson.CreatePersonRequest request = _createPersonRequestGenerator.Generate();
        
        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        ApiResponses.CreatedWithIdResponse? registerResponse = await response.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>();
        response.Headers.Location!.ToString().Should().Contain($"/api/v1/persons/{registerResponse?.Id}");
        registerResponse?.Id.Should().NotBeEmpty();
    }
    
    [Theory]
    [ClassData(typeof(InvalidEmailData))]
    public async Task CreatePerson_ShouldReturnValidationProblemDetails_WhenInvalidEmailIsProvided(string email)
    {
        //Arrange
        CreatePerson.CreatePersonRequest request = new Faker<CreatePerson.CreatePersonRequest>()
            .CustomInstantiator( faker =>
                new CreatePerson.CreatePersonRequest(
                    FirstName: faker.Person.FirstName,
                    LastName: faker.Person.LastName,
                    Email: email,
                    PhoneNumber: faker.Person.Phone,
                    UserIdentityId: Guid.NewGuid()
                ));
        
        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails?.Status.Should().Be(400);
        validationProblemDetails?.Errors["Email"][0].Should().Be("'Email' is not in the correct format.");
    }
    
    [Fact]
    public async Task CreatePerson_ShouldReturnValidationProblemDetails_WhenEmptyDataIsProvided()
    {
        //Arrange
        CreatePerson.CreatePersonRequest request = new(
            FirstName: "",
            LastName: "",
            Email: "",
            PhoneNumber: "",
            UserIdentityId: Guid.Empty
        );
        
        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails?.Status.Should().Be(400);
        validationProblemDetails?.Errors["FirstName"][0].Should().Be("'First Name' must not be empty.");
        validationProblemDetails?.Errors["LastName"][0].Should().Be("'Last Name' must not be empty.");
        validationProblemDetails?.Errors["PhoneNumber"][0].Should().Be("'Phone Number' must not be empty.");
        validationProblemDetails?.Errors["Email"][0].Should().Be("'Email' must not be empty.");
        validationProblemDetails?.Errors["UserIdentityId"][0].Should().Be("'User Identity Id' must not be empty.");
    }
    
    [Fact]
    public async Task CreatePerson_ShouldReturnValidationProblemDetails_WhenDuplicatedRequestOccur()
    {
        //Arrange
        CreatePerson.CreatePersonRequest request = _createPersonRequestGenerator.Generate();
        _ = await _httpClient.PostAsJsonAsync("api/v1/persons", request);
        
        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails?.Status.Should().Be(400);
        validationProblemDetails?.Errors["Email"][0].Should().StartWith("Email is already registered in database");
    }
}

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Features.Persons;
using KittySaver.Api.Features.Persons.SharedContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute.Extensions;
using Shared;

namespace KittySaver.Api.Tests.Integration.Persons;

[Collection("Api")]
public class DeletePersonEndpointsTests(KittySaverApiFactory appFactory)
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
                    UserIdentityId: Guid.NewGuid(),
                    AddressCountry: faker.Address.Country(),
                    AddressZipCode: faker.Address.ZipCode(),
                    AddressCity: faker.Address.City(),
                    AddressStreet: faker.Address.StreetName(),
                    AddressBuildingNumber: faker.Address.BuildingNumber(),
                    AddressState: faker.Address.State()
                ));
    
    [Fact]
    public async Task DeletePerson_ShouldReturnSuccess_WhenValidDataIsProvided()
    {
        //Arrange
        CreatePerson.CreatePersonRequest createRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", createRequest);
        ApiResponses.CreatedWithIdResponse registeredPersonResponse = 
            await response.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        
        //Act
        HttpResponseMessage deleteResponse = await _httpClient.DeleteAsync($"api/v1/persons/{registeredPersonResponse.Id}");
        
        //Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        HttpResponseMessage userNotFoundProblemDetailsMessage = 
            await _httpClient.GetAsync($"api/v1/persons/{registeredPersonResponse.Id}");
        userNotFoundProblemDetailsMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails? notFoundProblemDetails =
            await userNotFoundProblemDetailsMessage.Content.ReadFromJsonAsync<ProblemDetails>();
        notFoundProblemDetails.Should().NotBeNull();
        notFoundProblemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
    }
    
    [Fact]
    public async Task DeletePerson_ShouldReturnSuccess_WhenValidDataIsProvidedWithUserIdentityId()
    {
        //Arrange
        CreatePerson.CreatePersonRequest createRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", createRequest);
        ApiResponses.CreatedWithIdResponse registeredPersonResponse = 
            await response.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        PersonResponse person = await _httpClient.GetFromJsonAsync<PersonResponse>($"api/v1/persons/{registeredPersonResponse.Id}")
                                 ?? throw new JsonException();
        
        //Act
        HttpResponseMessage deleteResponse = await _httpClient.DeleteAsync($"api/v1/persons/{person.UserIdentityId}");
        
        //Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        HttpResponseMessage userNotFoundProblemDetailsMessage = 
            await _httpClient.GetAsync($"api/v1/persons/{registeredPersonResponse.Id}");
        userNotFoundProblemDetailsMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails? notFoundProblemDetails =
            await userNotFoundProblemDetailsMessage.Content.ReadFromJsonAsync<ProblemDetails>();
        notFoundProblemDetails.Should().NotBeNull();
        notFoundProblemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
    }
    
    [Fact]
    public async Task DeletePerson_ShouldReturnNotFound_WhenNonRegisteredUserIdProvided()
    {
        //Arrange
        Guid randomId = Guid.NewGuid();
        //Act
        HttpResponseMessage deleteResponse = await _httpClient.DeleteAsync($"api/v1/persons/{randomId}");
        
        //Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails? notFoundProblemDetails = await deleteResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        notFoundProblemDetails.Should().NotBeNull();
        notFoundProblemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
    }
    
    [Fact]
    public async Task DeletePerson_ShouldReturnBadRequest_WhenEmptyDataAreProvided()
    {
        //Arrange
        Guid randomId = Guid.Empty;
        //Act
        HttpResponseMessage deleteResponse = await _httpClient.DeleteAsync($"api/v1/persons/{randomId}");
        
        //Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await deleteResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(1);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo([
            nameof(DeletePerson.DeletePersonCommand.IdOrUserIdentityId)
        ]);
        validationProblemDetails.Errors.Values.Count.Should().Be(1);
        validationProblemDetails.Errors[nameof(DeletePerson.DeletePersonCommand.IdOrUserIdentityId)][0]
            .Should().Be("'Id Or User Identity Id' must not be empty.");
    }
}
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
using JsonException = System.Text.Json.JsonException;

namespace KittySaver.Api.Tests.Integration.Persons;

[Collection("Api")]
public class UpdatePersonEndpointsTests(KittySaverApiFactory appFactory)
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
    public async Task UpdatePerson_ShouldReturnSuccess_WhenValidDataIsProvided()
    {
        //Arrange
        CreatePerson.CreatePersonRequest createRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", createRequest);
        ApiResponses.CreatedWithIdResponse? registeredPersonResponse = await response.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>();
        PersonResponse person = await _httpClient.GetFromJsonAsync<PersonResponse>($"api/v1/persons/{registeredPersonResponse!.Id}") 
                                ?? throw new Exception("could not deserialize get person request");
        //Act
        UpdatePerson.UpdatePersonRequest request = new Faker<UpdatePerson.UpdatePersonRequest>()
            .CustomInstantiator(faker =>
                new UpdatePerson.UpdatePersonRequest(
                    FirstName: person.FirstName,
                    LastName: faker.Person.LastName,
                    Email: person.Email,
                    PhoneNumber: person.PhoneNumber
                ));
        
        HttpResponseMessage updateResponse = await _httpClient.PutAsJsonAsync($"api/v1/persons/{registeredPersonResponse.Id}", request);
        
        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        PersonResponse personAfterUpdate = await _httpClient.GetFromJsonAsync<PersonResponse>($"api/v1/persons/{registeredPersonResponse.Id}") 
                                ?? throw new JsonException();
        personAfterUpdate.Should().NotBeEquivalentTo(person);
        personAfterUpdate.LastName.Should().Be(request.LastName);
    }
    
    [Fact]
    public async Task UpdatePerson_ShouldReturnNotFound_WhenNonRegisteredUserIdProvided()
    {
        //Arrange
        Guid randomId = Guid.NewGuid();
        //Act
        UpdatePerson.UpdatePersonRequest request = new Faker<UpdatePerson.UpdatePersonRequest>()
            .CustomInstantiator(faker =>
                new UpdatePerson.UpdatePersonRequest(
                    FirstName: faker.Person.FirstName,
                    LastName: faker.Person.LastName,
                    Email: faker.Person.Email,
                    PhoneNumber: faker.Person.Phone
                ));
        
        HttpResponseMessage updateResponse = await _httpClient.PutAsJsonAsync($"api/v1/persons/{randomId}", request);
        
        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails? notFoundProblemDetails = await updateResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        notFoundProblemDetails.Should().NotBeNull();
        notFoundProblemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task UpdatePerson_ShouldReturnBadRequest_WhenEmptyDataAreProvided()
    {
        //Arrange
        CreatePerson.CreatePersonRequest firstUserCreateRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", firstUserCreateRequest);
        ApiResponses.CreatedWithIdResponse firstUserIdResponse = await response.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        UpdatePerson.UpdatePersonRequest request = new("", "", "", "");
        
        //Act
        HttpResponseMessage updateResponse = await _httpClient.PutAsJsonAsync($"api/v1/persons/{firstUserIdResponse.Id}", request);
        
        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await updateResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(4);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo([
            nameof(UpdatePerson.UpdatePersonRequest.FirstName),
            nameof(UpdatePerson.UpdatePersonRequest.LastName),
            nameof(UpdatePerson.UpdatePersonRequest.Email),
            nameof(UpdatePerson.UpdatePersonRequest.PhoneNumber)
        ]);
        validationProblemDetails.Errors.Values.Count.Should().Be(4);
        validationProblemDetails.Errors[nameof(UpdatePerson.UpdatePersonRequest.FirstName)][0]
            .Should().Be("'First Name' must not be empty.");
        validationProblemDetails.Errors[nameof(UpdatePerson.UpdatePersonRequest.LastName)][0]
            .Should().Be("'Last Name' must not be empty.");
        validationProblemDetails.Errors[nameof(UpdatePerson.UpdatePersonRequest.Email)][0]
            .Should().Be("'Email' must not be empty.");
        validationProblemDetails.Errors[nameof(UpdatePerson.UpdatePersonRequest.PhoneNumber)][0]
            .Should().Be("'Phone Number' must not be empty.");
    }
    
    [Theory]
    [ClassData(typeof(InvalidEmailData))]
    public async Task UpdatePerson_ShouldReturnBadRequest_WhenInvalidEmailIsProvided(string email)
    {
        //Arrange
        CreatePerson.CreatePersonRequest createPersonRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage createPersonResponse = await _httpClient.PostAsJsonAsync("api/v1/persons", createPersonRequest);
        ApiResponses.CreatedWithIdResponse idOfCreatedPersonResponse = 
            await createPersonResponse.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>() 
            ?? throw new JsonException();
        PersonResponse person = await _httpClient.GetFromJsonAsync<PersonResponse>($"api/v1/persons/{idOfCreatedPersonResponse.Id}") 
                                ?? throw new JsonException();
        
        UpdatePerson.UpdatePersonRequest request = new UpdatePerson.UpdatePersonRequest(
            FirstName: person.FirstName,
            LastName: person.LastName,
            Email: email,
            PhoneNumber: person.PhoneNumber
        );
        
        //Act
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/v1/persons/{person.Id}", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(1);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo([
            nameof(UpdatePerson.UpdatePersonRequest.Email)
        ]);
        validationProblemDetails.Errors.Values.Count.Should().Be(1);
        validationProblemDetails.Errors[nameof(UpdatePerson.UpdatePersonRequest.Email)][0]
            .Should().Be("'Email' is not in the correct format.");
    }
    
    [Fact]
    public async Task UpdatePerson_ShouldReturnBadRequest_WhenAlreadyTakenUniquePropertiesAreProvided()
    {
        //Arrange
        CreatePerson.CreatePersonRequest firstUserCreateRequest = _createPersonRequestGenerator.Generate();
        CreatePerson.CreatePersonRequest secondUserCreateRequest = firstUserCreateRequest with
        {
            Email = "unique@email.com",
            PhoneNumber = "420420420",
            UserIdentityId = Guid.NewGuid()
        };
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", firstUserCreateRequest);
        _= await _httpClient.PostAsJsonAsync("api/v1/persons", secondUserCreateRequest);
        
        ApiResponses.CreatedWithIdResponse? firstUserIdResponse = await response.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>();
        PersonResponse firstUser = await _httpClient.GetFromJsonAsync<PersonResponse>($"api/v1/persons/{firstUserIdResponse!.Id}") 
                                ?? throw new Exception("could not deserialize get person request");
        
        //Act
        UpdatePerson.UpdatePersonRequest firstUserUpdateRequest = new(
            firstUser.FirstName,
            firstUser.LastName,
            secondUserCreateRequest.Email,
            secondUserCreateRequest.PhoneNumber
        );
        
        HttpResponseMessage updateResponse = await _httpClient.PutAsJsonAsync($"api/v1/persons/{firstUserIdResponse.Id}", firstUserUpdateRequest);
        
        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await updateResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(2);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo([
            nameof(UpdatePerson.UpdatePersonRequest.Email),
            nameof(UpdatePerson.UpdatePersonRequest.PhoneNumber)
        ]);
        validationProblemDetails.Errors.Values.Count.Should().Be(2);
        validationProblemDetails.Errors[nameof(UpdatePerson.UpdatePersonRequest.Email)][0]
            .Should().Be("'Email' is already used by another user.");
        validationProblemDetails.Errors[nameof(UpdatePerson.UpdatePersonRequest.PhoneNumber)][0]
            .Should().Be("'Phone Number' is already used by another user.");
    }
}
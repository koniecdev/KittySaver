using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Features.Persons;
using KittySaver.Api.Features.Persons.SharedContracts;
using KittySaver.Api.Tests.Integration.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared;
using JsonException = System.Text.Json.JsonException;

namespace KittySaver.Api.Tests.Integration.Tests.Persons;

[Collection("Api")]
public class UpdatePersonEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CleanupHelper _cleanup;
    public UpdatePersonEndpointsTests(KittySaverApiFactory appFactory)
    {
        _httpClient = appFactory.CreateClient();
        _cleanup = new CleanupHelper(_httpClient);
    }

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
    public async Task UpdatePerson_ShouldReturnSuccess_WhenValidDataIsProvided()
    {
        //Arrange
        CreatePerson.CreatePersonRequest createRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", createRequest);
        ApiResponses.CreatedWithIdResponse registeredPersonResponse = await response.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
                                                                      ?? throw new JsonException();
        PersonResponse person = await _httpClient.GetFromJsonAsync<PersonResponse>($"api/v1/persons/{registeredPersonResponse.Id}") 
                                ?? throw new JsonException();
        //Act
        UpdatePerson.UpdatePersonRequest request = new Faker<UpdatePerson.UpdatePersonRequest>()
            .CustomInstantiator(faker =>
                new UpdatePerson.UpdatePersonRequest(
                    FirstName: faker.Person.FirstName,
                    LastName: faker.Person.LastName,
                    Email: faker.Person.Email,
                    PhoneNumber: faker.Person.Phone,
                    AddressCountry: faker.Address.Country(),
                    AddressZipCode: faker.Address.ZipCode(),
                    AddressCity: faker.Address.City(),
                    AddressStreet: faker.Address.StreetName(),
                    AddressBuildingNumber: faker.Address.BuildingNumber(),
                    AddressState: faker.Address.State()
                ));
        
        HttpResponseMessage updateResponse = await _httpClient.PutAsJsonAsync($"api/v1/persons/{registeredPersonResponse.Id}", request);
        
        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        PersonResponse personAfterUpdate = await _httpClient.GetFromJsonAsync<PersonResponse>($"api/v1/persons/{registeredPersonResponse.Id}") 
                                ?? throw new JsonException();
        personAfterUpdate.Should().NotBeEquivalentTo(person);
        personAfterUpdate.FirstName.Should().Be(request.FirstName);
        personAfterUpdate.LastName.Should().Be(request.LastName);
        personAfterUpdate.Email.Should().Be(request.Email);
        personAfterUpdate.PhoneNumber.Should().Be(request.PhoneNumber);
        personAfterUpdate.FullName.Should().Be($"{request.FirstName} {request.LastName}");
        personAfterUpdate.AddressCountry.Should().Be(request.AddressCountry);
        personAfterUpdate.AddressState.Should().Be(request.AddressState);
        personAfterUpdate.AddressZipCode.Should().Be(request.AddressZipCode);
        personAfterUpdate.AddressCity.Should().Be(request.AddressCity);
        personAfterUpdate.AddressStreet.Should().Be(request.AddressStreet);
        personAfterUpdate.AddressCity.Should().Be(request.AddressCity);
        personAfterUpdate.AddressBuildingNumber.Should().Be(request.AddressBuildingNumber);
    }
    
    [Fact]
    public async Task UpdatePerson_ShouldReturnSuccess_WhenValidDataIsProvidedWithUserIdentityId()
    {
        //Arrange
        CreatePerson.CreatePersonRequest createRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", createRequest);
        ApiResponses.CreatedWithIdResponse registeredPersonResponse = await response.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
                                                                      ?? throw new JsonException();
        PersonResponse person = await _httpClient.GetFromJsonAsync<PersonResponse>($"api/v1/persons/{registeredPersonResponse.Id}") 
                                ?? throw new JsonException();
        //Act
        UpdatePerson.UpdatePersonRequest request = new Faker<UpdatePerson.UpdatePersonRequest>()
            .CustomInstantiator(faker =>
                new UpdatePerson.UpdatePersonRequest(
                    FirstName: person.FirstName,
                    LastName: faker.Person.LastName,
                    Email: person.Email,
                    PhoneNumber: person.PhoneNumber,
                    AddressCountry: person.AddressCountry,
                    AddressZipCode: person.AddressZipCode,
                    AddressCity: person.AddressCity,
                    AddressStreet: person.AddressStreet,
                    AddressBuildingNumber: faker.Address.BuildingNumber(),
                    AddressState: person.AddressState
                ));
        
        HttpResponseMessage updateResponse = await _httpClient.PutAsJsonAsync($"api/v1/persons/{person.UserIdentityId}", request);
        
        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        PersonResponse personAfterUpdate = await _httpClient.GetFromJsonAsync<PersonResponse>($"api/v1/persons/{registeredPersonResponse.Id}") 
                                           ?? throw new JsonException();
        personAfterUpdate.Should().NotBeEquivalentTo(person);
        personAfterUpdate.FirstName.Should().Be(request.FirstName);
        personAfterUpdate.LastName.Should().Be(request.LastName);
        personAfterUpdate.FullName.Should().Be($"{request.FirstName} {request.LastName}");
        personAfterUpdate.Email.Should().Be(request.Email);
        personAfterUpdate.PhoneNumber.Should().Be(request.PhoneNumber);
        personAfterUpdate.AddressCountry.Should().Be(request.AddressCountry);
        personAfterUpdate.AddressState.Should().Be(request.AddressState);
        personAfterUpdate.AddressZipCode.Should().Be(request.AddressZipCode);
        personAfterUpdate.AddressCity.Should().Be(request.AddressCity);
        personAfterUpdate.AddressStreet.Should().Be(request.AddressStreet);
        personAfterUpdate.AddressCity.Should().Be(request.AddressCity);
        personAfterUpdate.AddressBuildingNumber.Should().Be(request.AddressBuildingNumber);
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
                    PhoneNumber: faker.Person.Phone,
                    AddressCountry: faker.Address.Country(),
                    AddressZipCode: faker.Address.ZipCode(),
                    AddressCity: faker.Address.City(),
                    AddressStreet: faker.Address.StreetName(),
                    AddressBuildingNumber: faker.Address.BuildingNumber(),
                    AddressState: faker.Address.State()
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
        
        //Act
        UpdatePerson.UpdatePersonRequest request = 
            new("", "", "", "", "", "", "", "", "", "");
        HttpResponseMessage updateResponse = await _httpClient.PutAsJsonAsync($"api/v1/persons/{firstUserIdResponse.Id}", request);
        
        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails = await updateResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(9);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo(
            nameof(UpdatePerson.UpdatePersonRequest.FirstName),
            nameof(UpdatePerson.UpdatePersonRequest.LastName),
            nameof(UpdatePerson.UpdatePersonRequest.Email),
            nameof(UpdatePerson.UpdatePersonRequest.PhoneNumber),
            nameof(UpdatePerson.UpdatePersonRequest.AddressCountry),
            nameof(UpdatePerson.UpdatePersonRequest.AddressZipCode),
            nameof(UpdatePerson.UpdatePersonRequest.AddressCity),
            nameof(UpdatePerson.UpdatePersonRequest.AddressStreet),
            nameof(UpdatePerson.UpdatePersonRequest.AddressBuildingNumber)
        );
        
        validationProblemDetails.Errors.Values.Count.Should().Be(9);
        
        validationProblemDetails.Errors[nameof(UpdatePerson.UpdatePersonRequest.FirstName)][0]
            .Should()
            .Be("'First Name' must not be empty.");
        
        validationProblemDetails.Errors[nameof(UpdatePerson.UpdatePersonRequest.LastName)][0]
            .Should()
            .Be("'Last Name' must not be empty.");
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.PhoneNumber)][0]
            .Should()
            .Be("'Phone Number' must not be empty.");
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.Email)][0]
            .Should()
            .Be("'Email' must not be empty.");
        
        validationProblemDetails.Errors[nameof(UpdatePerson.UpdatePersonRequest.AddressCountry)][0]
            .Should()
            .Be("'Address Country' must not be empty.");
        
        validationProblemDetails.Errors[nameof(UpdatePerson.UpdatePersonRequest.AddressZipCode)][0]
            .Should()
            .Be("'Address Zip Code' must not be empty.");
        
        validationProblemDetails.Errors[nameof(UpdatePerson.UpdatePersonRequest.AddressCity)][0]
            .Should()
            .Be("'Address City' must not be empty.");
        
        validationProblemDetails.Errors[nameof(UpdatePerson.UpdatePersonRequest.AddressStreet)][0]
            .Should()
            .Be("'Address Street' must not be empty.");
        
        validationProblemDetails.Errors[nameof(UpdatePerson.UpdatePersonRequest.AddressBuildingNumber)][0]
            .Should()
            .Be("'Address Building Number' must not be empty.");
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
            PhoneNumber: person.PhoneNumber,
            AddressCountry: person.AddressCountry,
            AddressZipCode: person.AddressZipCode,
            AddressCity: person.AddressCity,
            AddressStreet: person.AddressStreet,
            AddressBuildingNumber: person.AddressBuildingNumber,
            AddressState: person.AddressState
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
        CreatePerson.CreatePersonRequest firstPersonCreateRequest = _createPersonRequestGenerator.Generate();
        CreatePerson.CreatePersonRequest secondPersonCreateRequest = firstPersonCreateRequest with
        {
            Email = "unique@email.com",
            PhoneNumber = "420420420",
            UserIdentityId = Guid.NewGuid()
        };
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", firstPersonCreateRequest);
        _= await _httpClient.PostAsJsonAsync("api/v1/persons", secondPersonCreateRequest);
        
        ApiResponses.CreatedWithIdResponse firstUserIdResponse = await response.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
                                                                  ?? throw new JsonException();
        PersonResponse firstUser = await _httpClient.GetFromJsonAsync<PersonResponse>($"api/v1/persons/{firstUserIdResponse.Id}") 
                                 ?? throw new JsonException();
        
        //Act
        UpdatePerson.UpdatePersonRequest firstUserUpdateRequest = new(
            firstUser.FirstName,
            firstUser.LastName,
            secondPersonCreateRequest.Email,
            secondPersonCreateRequest.PhoneNumber,
            firstUser.AddressCountry,
            firstUser.AddressZipCode,
            firstUser.AddressCity,
            firstUser.AddressStreet,
            firstUser.AddressBuildingNumber,
            firstUser.AddressState
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
    
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _cleanup.Cleanup();
    }
}
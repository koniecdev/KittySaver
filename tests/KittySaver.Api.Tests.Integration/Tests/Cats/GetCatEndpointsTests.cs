using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Features.Cats.SharedContracts;
using KittySaver.Api.Features.Persons;
using KittySaver.Api.Features.Persons.SharedContracts;
using KittySaver.Api.Shared.Domain.Enums;
using KittySaver.Api.Tests.Integration.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace KittySaver.Api.Tests.Integration.Tests.Cats;

[Collection("Api")]
public class GetCatEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CleanupHelper _cleanup;
    public GetCatEndpointsTests(KittySaverApiFactory appFactory)
    {
        _httpClient = appFactory.CreateClient();
        _cleanup = new CleanupHelper(_httpClient);
    }
    
    private readonly CreatePerson.CreatePersonRequest _createPersonRequest =
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
                )).Generate();
    
    private readonly CreateCat.CreateCatRequest _createCatRequest =
        new Faker<CreateCat.CreateCatRequest>()
            .CustomInstantiator(faker =>
                new CreateCat.CreateCatRequest(
                    Name: faker.Name.FirstName(),
                    IsCastrated: true,
                    IsInNeedOfSeeingVet: false,
                    MedicalHelpUrgency: MedicalHelpUrgency.NoNeed.Name,
                    Behavior: Behavior.Friendly.Name,
                    HealthStatus: HealthStatus.Good.Name,
                    AgeCategory: AgeCategory.Adult.Name,
                    AdditionalRequirements: "Lorem ipsum"
                )).Generate();

    [Fact]
    public async Task GetCat_ShouldReturnCat_WhenCatExist()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        ApiResponses.CreatedWithIdResponse personRegisterResponse =
            await personRegisterResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        
        HttpResponseMessage catCreateResponseMessage = 
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats", _createCatRequest);
        ApiResponses.CreatedWithIdResponse catCreateResponse = 
            await catCreateResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        
        //Act
        HttpResponseMessage response = await _httpClient.GetAsync($"api/v1/persons/{personRegisterResponse.Id}/cats/{catCreateResponse.Id}");
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        CatResponse cat = await response.Content.ReadFromJsonAsync<CatResponse>() ?? throw new JsonException();
        cat.Id.Should().Be(catCreateResponse.Id);
        cat.PersonId.Should().Be(personRegisterResponse.Id);
        cat.Name.Should().Be(_createCatRequest.Name);
        cat.AdditionalRequirements.Should().Be(_createCatRequest.AdditionalRequirements);
        cat.Behavior.Should().Be(_createCatRequest.Behavior);
        cat.AgeCategory.Should().Be(_createCatRequest.AgeCategory);
        cat.MedicalHelpUrgency.Should().Be(_createCatRequest.MedicalHelpUrgency);
        cat.HealthStatus.Should().Be(_createCatRequest.HealthStatus);
        cat.IsCastrated.Should().Be(_createCatRequest.IsCastrated);
        cat.IsInNeedOfSeeingVet.Should().Be(_createCatRequest.IsInNeedOfSeeingVet);
        cat.PriorityScore.Should().BeGreaterThan(0);
    }
    
    [Fact]
    public async Task GetPerson_ShouldReturnNotFound_WhenNonExistingCatIsProvided()
    {
        //Act
        HttpResponseMessage personRegisterResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        ApiResponses.CreatedWithIdResponse personRegisterResponse =
            await personRegisterResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        HttpResponseMessage response = await _httpClient.GetAsync($"api/v1/persons/{personRegisterResponse.Id}/cats/{Guid.NewGuid()}");
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails? problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetCat_ShouldReturnNotFound_WhenNonExistingPersonIsProvided()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        ApiResponses.CreatedWithIdResponse personRegisterResponse =
            await personRegisterResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        HttpResponseMessage catCreateResponseMessage = 
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats", _createCatRequest);
        ApiResponses.CreatedWithIdResponse catCreateResponse = 
            await catCreateResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        
        //Act
        HttpResponseMessage response = await _httpClient.GetAsync($"api/v1/persons/{Guid.NewGuid()}/cats/{catCreateResponse.Id}");
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails? problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
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

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using Bogus.Extensions;
using FluentAssertions;
using KittySaver.Api.Features.Cats.SharedContracts;
using KittySaver.Api.Features.Persons;
using KittySaver.Api.Shared.Domain.Common.Primitives.Enums;
using KittySaver.Api.Shared.Domain.Persons;
using KittySaver.Api.Shared.Domain.ValueObjects;
using KittySaver.Api.Tests.Integration.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace KittySaver.Api.Tests.Integration.Tests.Cats;

[Collection("Api")]
public class CreateCatEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CleanupHelper _cleanup;
    public CreateCatEndpointsTests(KittySaverApiFactory appFactory)
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
                    AddressState: faker.Address.State(),
                    DefaultAdvertisementPickupAddressCountry: faker.Address.Country(),
                    DefaultAdvertisementPickupAddressState: faker.Address.State(),
                    DefaultAdvertisementPickupAddressZipCode: faker.Address.ZipCode(),
                    DefaultAdvertisementPickupAddressCity: faker.Address.City(),
                    DefaultAdvertisementPickupAddressStreet: faker.Address.StreetName(),
                    DefaultAdvertisementPickupAddressBuildingNumber: faker.Address.BuildingNumber(),
                    DefaultAdvertisementContactInfoEmail: faker.Person.Email,
                    DefaultAdvertisementContactInfoPhoneNumber: faker.Person.Phone
                )).Generate();
    
    private readonly Faker<CreateCat.CreateCatRequest> _createCatRequestGenerator =
        new Faker<CreateCat.CreateCatRequest>()
            .CustomInstantiator(faker =>
                new CreateCat.CreateCatRequest(
                    Name: faker.Name.FirstName(),
                    IsCastrated: true,
                    MedicalHelpUrgency: MedicalHelpUrgency.NoNeed.Name,
                    Behavior: Behavior.Friendly.Name,
                    HealthStatus: HealthStatus.Good.Name,
                    AgeCategory: AgeCategory.Adult.Name,
                    AdditionalRequirements: "Lorem ipsum"
                ));
    
    [Fact]
    public async Task CreateCat_ShouldReturnSuccess_WhenValidDataIsProvided()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        ApiResponses.CreatedWithIdResponse personRegisterResponse = 
            await personRegisterResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        CreateCat.CreateCatRequest request = _createCatRequestGenerator.Generate();
        
        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        ApiResponses.CreatedWithIdResponse? registerResponse = await response.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>();
        registerResponse.Should().NotBeNull();
        registerResponse!.Id.Should().NotBeEmpty();
        response.Headers.Location!.ToString().Should().Contain($"/api/v1/persons/{personRegisterResponse.Id}/cats/{registerResponse.Id}");
    }
    
    [Fact]
    public async Task CreateCat_ShouldReturnSuccess_WhenValidDataIsProvidedWithoutAdditionalParameters()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        ApiResponses.CreatedWithIdResponse personRegisterResponse = 
            await personRegisterResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        CreateCat.CreateCatRequest request = new CreateCat.CreateCatRequest(
            Name: "Whiskers",
            IsCastrated: true,
            MedicalHelpUrgency: MedicalHelpUrgency.NoNeed.Name,
            Behavior: Behavior.Friendly.Name,
            HealthStatus: HealthStatus.Good.Name,
            AgeCategory: AgeCategory.Adult.Name
        );
        
        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        ApiResponses.CreatedWithIdResponse? registerResponse = await response.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>();
        registerResponse.Should().NotBeNull();
        registerResponse!.Id.Should().NotBeEmpty();
        response.Headers.Location!.ToString().Should().Contain($"/api/v1/persons/{personRegisterResponse.Id}/cats/{registerResponse.Id}");
    }
    
    [Fact]
    public async Task CreateCat_ShouldReturnSuccess_WhenInvalidAdditionalRequirementsAreProvided()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        ApiResponses.CreatedWithIdResponse personRegisterResponse = 
            await personRegisterResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        CreateCat.CreateCatRequest request = new CreateCat.CreateCatRequest(
            Name: "Whiskers",
            IsCastrated: true,
            MedicalHelpUrgency: MedicalHelpUrgency.NoNeed.Name,
            Behavior: Behavior.Friendly.Name,
            HealthStatus: HealthStatus.Good.Name,
            AgeCategory: AgeCategory.Adult.Name,
            AdditionalRequirements: " "
        );
        
        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        ApiResponses.CreatedWithIdResponse? registerResponse = await response.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>();
        registerResponse.Should().NotBeNull();
        registerResponse!.Id.Should().NotBeEmpty();
        response.Headers.Location!.ToString().Should().Contain($"/api/v1/persons/{personRegisterResponse.Id}/cats/{registerResponse.Id}");
        CatResponse catAfterUpdate = 
            await _httpClient.GetFromJsonAsync<CatResponse>($"api/v1/persons/{personRegisterResponse.Id}/cats/{registerResponse.Id}") 
            ?? throw new JsonException();
        catAfterUpdate.AdditionalRequirements.Should().Be(string.Empty);
    }
    
    [Fact]
    public async Task CreateCat_ShouldReturnBadRequest_WhenTooLongDataAreProvided()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        ApiResponses.CreatedWithIdResponse personRegisterResponse = 
            await personRegisterResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        CreateCat.CreateCatRequest request = new Faker<CreateCat.CreateCatRequest>()
            .CustomInstantiator( faker =>
                new CreateCat.CreateCatRequest(
                    Name: faker.Person.FirstName.ClampLength(CatName.MaxLength + 1),
                    IsCastrated: true,
                    MedicalHelpUrgency: MedicalHelpUrgency.ShouldSeeVet.Name,
                    Behavior: Behavior.Unfriendly.Name,
                    HealthStatus: HealthStatus.Poor.Name,
                    AgeCategory: AgeCategory.Baby.Name,
                    AdditionalRequirements: faker.Address.State().ClampLength(Description.MaxLength + 1)
                )).Generate();
        
        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(2);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo(
            nameof(CreateCat.CreateCatRequest.Name),
            nameof(CreateCat.CreateCatRequest.AdditionalRequirements)
        );
        validationProblemDetails.Errors.Values.Count.Should().Be(2);
        
        validationProblemDetails.Errors[nameof(CreateCat.CreateCatRequest.Name)][0]
            .Should()
            .StartWith($"The length of 'Name' must be {CatName.MaxLength} characters or fewer. You entered");
        
        validationProblemDetails.Errors[nameof(CreateCat.CreateCatRequest.AdditionalRequirements)][0]
            .Should()
            .StartWith($"The length of 'Additional Requirements' must be {Description.MaxLength} characters or fewer. You entered");
    }
    
    [Fact]
    public async Task CreateCat_ShouldReturnBadRequest_WhenEmptyDataAreProvided()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        ApiResponses.CreatedWithIdResponse personRegisterResponse = 
            await personRegisterResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        CreateCat.CreateCatRequest request = new(
            Name: "",
            IsCastrated: false,
            MedicalHelpUrgency: "",
            Behavior: "",
            HealthStatus: "",
            AgeCategory: ""
        );
        
        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        
        validationProblemDetails.Errors.Count.Should().Be(5);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo(
            nameof(CreateCat.CreateCatRequest.Name),
            nameof(CreateCat.CreateCatRequest.MedicalHelpUrgency),
            nameof(CreateCat.CreateCatRequest.Behavior),
            nameof(CreateCat.CreateCatRequest.AgeCategory),
            nameof(CreateCat.CreateCatRequest.HealthStatus)
        );
        validationProblemDetails.Errors.Values.Count.Should().Be(5);
        
        validationProblemDetails.Errors[nameof(CreateCat.CreateCatRequest.MedicalHelpUrgency)][0]
            .Should()
            .Be("Provided empty or invalid Medical Help Urgency.");
        
        validationProblemDetails.Errors[nameof(CreateCat.CreateCatRequest.Behavior)][0]
            .Should()
            .Be("Provided empty or invalid Behavior.");
        
        validationProblemDetails.Errors[nameof(CreateCat.CreateCatRequest.AgeCategory)][0]
            .Should()
            .Be("Provided empty or invalid Age Category.");
        
        validationProblemDetails.Errors[nameof(CreateCat.CreateCatRequest.HealthStatus)][0]
            .Should()
            .Be("Provided empty or invalid Health Status.");
        
        validationProblemDetails.Errors[nameof(CreateCat.CreateCatRequest.Name)][0]
            .Should()
            .Be("'Name' must not be empty.");
    }
    
    [Fact]
    public async Task CreateCat_ShouldReturnNotFound_WhenNotExistingPersonIsProvided()
    {
        //Arrange
        CreateCat.CreateCatRequest request = _createCatRequestGenerator.Generate();
        
        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/v1/persons/{Guid.NewGuid()}/cats", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails? notFoundProblemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        notFoundProblemDetails.Should().NotBeNull();
        notFoundProblemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
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

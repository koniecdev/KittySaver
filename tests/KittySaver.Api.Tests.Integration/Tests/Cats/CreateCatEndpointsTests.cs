using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using Bogus.Extensions;
using FluentAssertions;
using KittySaver.Api.Features.Cats.SharedContracts;
using KittySaver.Api.Features.Persons;
using KittySaver.Api.Shared.Domain.Entites;
using KittySaver.Api.Shared.Domain.Enums;
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
                    AddressState: faker.Address.State()
                )).Generate();
    
    private readonly Faker<CreateCat.CreateCatRequest> _createCatRequestGenerator =
        new Faker<CreateCat.CreateCatRequest>()
            .CustomInstantiator(faker =>
                new CreateCat.CreateCatRequest(
                    Name: faker.Name.FirstName(),
                    IsCastrated: true,
                    IsInNeedOfSeeingVet: false,
                    MedicalHelpUrgencyName: MedicalHelpUrgency.NoNeed.Name,
                    BehaviorName: Behavior.Friendly.Name,
                    HealthStatusName: HealthStatus.Good.Name,
                    AgeCategoryName: AgeCategory.Adult.Name,
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
            IsInNeedOfSeeingVet: false,
            MedicalHelpUrgencyName: MedicalHelpUrgency.NoNeed.Name,
            BehaviorName: Behavior.Friendly.Name,
            HealthStatusName: HealthStatus.Good.Name,
            AgeCategoryName: AgeCategory.Adult.Name
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
            IsInNeedOfSeeingVet: false,
            MedicalHelpUrgencyName: MedicalHelpUrgency.NoNeed.Name,
            BehaviorName: Behavior.Friendly.Name,
            HealthStatusName: HealthStatus.Good.Name,
            AgeCategoryName: AgeCategory.Adult.Name,
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
        catAfterUpdate.AdditionalRequirements.Should().BeNull();
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
                    Name: faker.Person.FirstName.ClampLength(Cat.Constraints.NameMaxLength + 1),
                    IsCastrated: true,
                    IsInNeedOfSeeingVet: false,
                    MedicalHelpUrgencyName: MedicalHelpUrgency.ShouldSeeVet.Name,
                    BehaviorName: Behavior.Unfriendly.Name,
                    HealthStatusName: HealthStatus.Poor.Name,
                    AgeCategoryName: AgeCategory.Baby.Name,
                    AdditionalRequirements: faker.Address.State().ClampLength(Cat.Constraints.AdditionalRequirementsMaxLength + 1)
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
            .StartWith($"The length of 'Name' must be {Cat.Constraints.NameMaxLength} characters or fewer. You entered");
        
        validationProblemDetails.Errors[nameof(CreateCat.CreateCatRequest.AdditionalRequirements)][0]
            .Should()
            .StartWith($"The length of 'Additional Requirements' must be {Cat.Constraints.AdditionalRequirementsMaxLength} characters or fewer. You entered");
    }
    
    [Fact]
    public async Task CreateCat_ShouldReturnBadRequest_WhenEmptySmartEnumsAreProvided()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        ApiResponses.CreatedWithIdResponse personRegisterResponse = 
            await personRegisterResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        CreateCat.CreateCatRequest request = new(
            Name: "",
            IsCastrated: false,
            IsInNeedOfSeeingVet: false,
            MedicalHelpUrgencyName: "",
            BehaviorName: "",
            HealthStatusName: "",
            AgeCategoryName: ""
        );
        
        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(4);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo(
            nameof(CreateCat.CreateCatRequest.MedicalHelpUrgencyName),
            nameof(CreateCat.CreateCatRequest.BehaviorName),
            nameof(CreateCat.CreateCatRequest.AgeCategoryName),
            nameof(CreateCat.CreateCatRequest.HealthStatusName)
        );
        validationProblemDetails.Errors.Values.Count.Should().Be(4);
        
        validationProblemDetails.Errors[nameof(CreateCat.CreateCatRequest.MedicalHelpUrgencyName)][0]
            .Should()
            .Be($"Provided invalid '{nameof(CreateCat.CreateCatRequest.MedicalHelpUrgencyName)}' value.");
        
        validationProblemDetails.Errors[nameof(CreateCat.CreateCatRequest.BehaviorName)][0]
            .Should()
            .Be($"Provided invalid '{nameof(CreateCat.CreateCatRequest.BehaviorName)}' value.");
        
        validationProblemDetails.Errors[nameof(CreateCat.CreateCatRequest.AgeCategoryName)][0]
            .Should()
            .Be($"Provided invalid '{nameof(CreateCat.CreateCatRequest.AgeCategoryName)}' value.");
        
        validationProblemDetails.Errors[nameof(CreateCat.CreateCatRequest.HealthStatusName)][0]
            .Should()
            .Be($"Provided invalid '{nameof(CreateCat.CreateCatRequest.HealthStatusName)}' value.");
    }
    
    [Fact]
    public async Task CreateCat_ShouldReturnBadRequest_WhenEmptyNameIsProvided()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        ApiResponses.CreatedWithIdResponse personRegisterResponse = 
            await personRegisterResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        CreateCat.CreateCatRequest request = new(
            Name: "",
            IsCastrated: false,
            IsInNeedOfSeeingVet: false,
            MedicalHelpUrgencyName: MedicalHelpUrgency.NoNeed.Name,
            BehaviorName: Behavior.Friendly.Name,
            HealthStatusName: HealthStatus.Good.Name,
            AgeCategoryName: AgeCategory.Adult.Name
        );
        
        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(1);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo(
            nameof(CreateCat.CreateCatRequest.Name)
        );
        validationProblemDetails.Errors.Values.Count.Should().Be(1);
        
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

using System.Net;
using System.Net.Http.Json;
using Bogus;
using Bogus.Extensions;
using FluentAssertions;
using KittySaver.Api.Features.Cats;
using KittySaver.Api.Features.Cats.SharedContracts;
using KittySaver.Api.Features.Persons;
using KittySaver.Api.Shared.Domain.Entites;
using KittySaver.Api.Shared.Domain.Enums;
using KittySaver.Api.Tests.Integration.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared;
using JsonException = System.Text.Json.JsonException;

namespace KittySaver.Api.Tests.Integration.Tests.Cats;

[Collection("Api")]
public class UpdateCatEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CleanupHelper _cleanup;
    public UpdateCatEndpointsTests(KittySaverApiFactory appFactory)
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
                    IsInNeedOfSeeingVet: true,
                    MedicalHelpUrgencyName: MedicalHelpUrgency.NoNeed.Name,
                    BehaviorName: Behavior.Friendly.Name,
                    HealthStatusName: HealthStatus.Good.Name,
                    AgeCategoryName: AgeCategory.Baby.Name,
                    AdditionalRequirements: "Lorem ipsum"
                )).Generate();
    
    [Fact]
    public async Task UpdateCat_ShouldReturnSuccess_WhenValidDataIsProvided()
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
        
        CatResponse catBeforeUpdate =
            await _httpClient.GetFromJsonAsync<CatResponse>($"api/v1/persons/{personRegisterResponse.Id}/cats/{catCreateResponse.Id}") 
            ?? throw new JsonException();
        
        //Act
        UpdateCat.UpdateCatRequest request = new Faker<UpdateCat.UpdateCatRequest>()
            .CustomInstantiator(faker =>
                new UpdateCat.UpdateCatRequest(
                    Name: faker.Name.FirstName(),
                    IsCastrated: false,
                    IsInNeedOfSeeingVet: false,
                    MedicalHelpUrgencyName: MedicalHelpUrgency.ShouldSeeVet.Name,
                    BehaviorName: Behavior.Unfriendly.Name,
                    HealthStatusName: HealthStatus.Critical.Name,
                    AgeCategoryName: AgeCategory.Senior.Name,
                    AdditionalRequirements: "Lorem ipsum dolor sit"
                )).Generate();
        
        HttpResponseMessage updateResponse = 
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats/{catCreateResponse.Id}", request);
        
        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        CatResponse catAfterUpdate = 
            await _httpClient.GetFromJsonAsync<CatResponse>($"api/v1/persons/{personRegisterResponse.Id}/cats/{catCreateResponse.Id}") 
            ?? throw new JsonException();
        catBeforeUpdate.Should().NotBeEquivalentTo(catAfterUpdate);
        catAfterUpdate.Name.Should().Be(request.Name);
        catAfterUpdate.AdditionalRequirements.Should().Be(request.AdditionalRequirements);
        catAfterUpdate.BehaviorName.Should().Be(request.BehaviorName);
        catAfterUpdate.HealthStatusName.Should().Be(request.HealthStatusName);
        catAfterUpdate.AgeCategoryName.Should().Be(request.AgeCategoryName);
        catAfterUpdate.MedicalHelpUrgencyName.Should().Be(request.MedicalHelpUrgencyName);
        catAfterUpdate.IsCastrated.Should().Be(request.IsCastrated);
        catAfterUpdate.IsInNeedOfSeeingVet.Should().Be(request.IsInNeedOfSeeingVet);
    }
    
    [Fact]
    public async Task UpdateCat_ShouldReturnSuccess_WhenValidDataIsProvidedWithoutAdditionalParameters()
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
        
        CatResponse catBeforeUpdate =
            await _httpClient.GetFromJsonAsync<CatResponse>($"api/v1/persons/{personRegisterResponse.Id}/cats/{catCreateResponse.Id}") 
            ?? throw new JsonException();
        
        //Act
        UpdateCat.UpdateCatRequest request = new Faker<UpdateCat.UpdateCatRequest>()
            .CustomInstantiator(faker =>
                new UpdateCat.UpdateCatRequest(
                    Name: faker.Name.FirstName(),
                    IsCastrated: false,
                    IsInNeedOfSeeingVet: false,
                    MedicalHelpUrgencyName: MedicalHelpUrgency.ShouldSeeVet.Name,
                    BehaviorName: Behavior.Unfriendly.Name,
                    HealthStatusName: HealthStatus.Critical.Name,
                    AgeCategoryName: AgeCategory.Senior.Name,
                    AdditionalRequirements: null
                )).Generate();
        
        HttpResponseMessage updateResponse = 
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats/{catCreateResponse.Id}", request);
        
        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        CatResponse catAfterUpdate = 
            await _httpClient.GetFromJsonAsync<CatResponse>($"api/v1/persons/{personRegisterResponse.Id}/cats/{catCreateResponse.Id}") 
            ?? throw new JsonException();
        catBeforeUpdate.Should().NotBeEquivalentTo(catAfterUpdate);
        catAfterUpdate.AdditionalRequirements.Should().BeNull();
        catAfterUpdate.Name.Should().Be(request.Name);
        catAfterUpdate.BehaviorName.Should().Be(request.BehaviorName);
        catAfterUpdate.HealthStatusName.Should().Be(request.HealthStatusName);
        catAfterUpdate.AgeCategoryName.Should().Be(request.AgeCategoryName);
        catAfterUpdate.MedicalHelpUrgencyName.Should().Be(request.MedicalHelpUrgencyName);
        catAfterUpdate.IsCastrated.Should().Be(request.IsCastrated);
        catAfterUpdate.IsInNeedOfSeeingVet.Should().Be(request.IsInNeedOfSeeingVet);
    }
    
    [Fact]
    public async Task UpdateCat_ShouldReturnSuccess_WhenInvalidAdditionalRequirementsAreProvided()
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
        
        CatResponse catBeforeUpdate =
            await _httpClient.GetFromJsonAsync<CatResponse>($"api/v1/persons/{personRegisterResponse.Id}/cats/{catCreateResponse.Id}") 
            ?? throw new JsonException();
        
        //Act
        UpdateCat.UpdateCatRequest request = new Faker<UpdateCat.UpdateCatRequest>()
            .CustomInstantiator(faker =>
                new UpdateCat.UpdateCatRequest(
                    Name: faker.Name.FirstName(),
                    IsCastrated: false,
                    IsInNeedOfSeeingVet: false,
                    MedicalHelpUrgencyName: MedicalHelpUrgency.ShouldSeeVet.Name,
                    BehaviorName: Behavior.Unfriendly.Name,
                    HealthStatusName: HealthStatus.Critical.Name,
                    AgeCategoryName: AgeCategory.Senior.Name,
                    AdditionalRequirements: " "
                )).Generate();
        
        HttpResponseMessage updateResponse = 
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats/{catCreateResponse.Id}", request);
        
        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        CatResponse catAfterUpdate = 
            await _httpClient.GetFromJsonAsync<CatResponse>($"api/v1/persons/{personRegisterResponse.Id}/cats/{catCreateResponse.Id}") 
            ?? throw new JsonException();
        catBeforeUpdate.Should().NotBeEquivalentTo(catAfterUpdate);
        catAfterUpdate.AdditionalRequirements.Should().BeNull();
        catAfterUpdate.Name.Should().Be(request.Name);
        catAfterUpdate.BehaviorName.Should().Be(request.BehaviorName);
        catAfterUpdate.HealthStatusName.Should().Be(request.HealthStatusName);
        catAfterUpdate.AgeCategoryName.Should().Be(request.AgeCategoryName);
        catAfterUpdate.MedicalHelpUrgencyName.Should().Be(request.MedicalHelpUrgencyName);
        catAfterUpdate.IsCastrated.Should().Be(request.IsCastrated);
        catAfterUpdate.IsInNeedOfSeeingVet.Should().Be(request.IsInNeedOfSeeingVet);
    }
    
    [Fact]
    public async Task UpdateCat_ShouldReturnNotFound_WhenNonExistingCatIsProvided()
    {
        //Arrange
        Guid randomId = Guid.NewGuid();
        
        HttpResponseMessage personRegisterResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        ApiResponses.CreatedWithIdResponse personRegisterResponse = 
            await personRegisterResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        
        //Act
        UpdateCat.UpdateCatRequest request = new Faker<UpdateCat.UpdateCatRequest>()
            .CustomInstantiator(faker =>
                new UpdateCat.UpdateCatRequest(
                    Name: faker.Name.FirstName(),
                    IsCastrated: false,
                    IsInNeedOfSeeingVet: false,
                    MedicalHelpUrgencyName: MedicalHelpUrgency.ShouldSeeVet.Name,
                    BehaviorName: Behavior.Unfriendly.Name,
                    HealthStatusName: HealthStatus.Critical.Name,
                    AgeCategoryName: AgeCategory.Senior.Name,
                    AdditionalRequirements: "Lorem ipsum dolor sit"
                )).Generate();
        
        HttpResponseMessage updateResponse = 
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats/{randomId}", request);
        
        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails? notFoundProblemDetails = await updateResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        notFoundProblemDetails.Should().NotBeNull();
        notFoundProblemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
    }
    
    [Fact]
    public async Task UpdateCat_ShouldReturnNotFound_WhenNonExistingPersonIsProvided()
    {
        //Arrange
        Guid randomId = Guid.NewGuid();
        
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
        UpdateCat.UpdateCatRequest request = new Faker<UpdateCat.UpdateCatRequest>()
            .CustomInstantiator(faker =>
                new UpdateCat.UpdateCatRequest(
                    Name: faker.Name.FirstName(),
                    IsCastrated: false,
                    IsInNeedOfSeeingVet: false,
                    MedicalHelpUrgencyName: MedicalHelpUrgency.ShouldSeeVet.Name,
                    BehaviorName: Behavior.Unfriendly.Name,
                    HealthStatusName: HealthStatus.Critical.Name,
                    AgeCategoryName: AgeCategory.Senior.Name,
                    AdditionalRequirements: "Lorem ipsum dolor sit"
                )).Generate();
        
        HttpResponseMessage updateResponse = 
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{randomId}/cats/{catCreateResponse.Id}", request);
        
        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails? notFoundProblemDetails = await updateResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        notFoundProblemDetails.Should().NotBeNull();
        notFoundProblemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
    }
    
    [Fact]
    public async Task UpdatePerson_ShouldReturnBadRequest_WhenEmptySmartEnumsAreProvided()
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
        UpdateCat.UpdateCatRequest request =
            new UpdateCat.UpdateCatRequest(
                Name: "Whiskers",
                IsCastrated: false,
                IsInNeedOfSeeingVet: false,
                MedicalHelpUrgencyName: "",
                BehaviorName: "",
                HealthStatusName: "",
                AgeCategoryName: ""
            );
        HttpResponseMessage response = 
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats/{catCreateResponse.Id}", request);
        
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
    public async Task UpdateCat_ShouldReturnBadRequest_WhenEmptyNameIsProvided()
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
        UpdateCat.UpdateCatRequest request =
            new UpdateCat.UpdateCatRequest(
                Name: "",
                IsCastrated: false,
                IsInNeedOfSeeingVet: false,
                MedicalHelpUrgencyName: MedicalHelpUrgency.ShouldSeeVet.Name,
                BehaviorName: Behavior.Unfriendly.Name,
                HealthStatusName: HealthStatus.Critical.Name,
                AgeCategoryName: AgeCategory.Senior.Name
            );
        HttpResponseMessage response =
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats/{catCreateResponse.Id}", request);
        
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
    public async Task CreateCat_ShouldReturnBadRequest_WhenTooLongDataAreProvided()
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
        UpdateCat.UpdateCatRequest request = new Faker<UpdateCat.UpdateCatRequest>()
            .CustomInstantiator( faker =>
                new UpdateCat.UpdateCatRequest(
                    Name: faker.Person.FirstName.ClampLength(Cat.Constraints.NameMaxLength + 1),
                    IsCastrated: true,
                    IsInNeedOfSeeingVet: false,
                    MedicalHelpUrgencyName: MedicalHelpUrgency.ShouldSeeVet.Name,
                    BehaviorName: Behavior.Unfriendly.Name,
                    HealthStatusName: HealthStatus.Poor.Name,
                    AgeCategoryName: AgeCategory.Baby.Name,
                    AdditionalRequirements: faker.Address.State().ClampLength(Cat.Constraints.AdditionalRequirementsMaxLength + 1)
                )).Generate();
        
        HttpResponseMessage response = 
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats/{catCreateResponse.Id}", request);
        
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
    
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _cleanup.Cleanup();
    }
}
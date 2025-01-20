using System.Net;
using System.Net.Http.Json;
using Bogus;
using Bogus.Extensions;
using FluentAssertions;
using KittySaver.Api.Features.Advertisements;
using KittySaver.Api.Features.Advertisements.SharedContracts;
using KittySaver.Api.Features.Cats;
using KittySaver.Api.Features.Cats.SharedContracts;
using KittySaver.Api.Features.Persons;
using KittySaver.Api.Shared.Endpoints;
using KittySaver.Api.Shared.Hateoas;
using KittySaver.Api.Tests.Integration.Helpers;
using KittySaver.Domain.Common.Primitives.Enums;
using KittySaver.Domain.Persons;
using KittySaver.Domain.ValueObjects;
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
            .CustomInstantiator(faker =>
                new CreatePerson.CreatePersonRequest(
                    Nickname: faker.Person.FirstName,
                    Email: faker.Person.Email,
                    PhoneNumber: faker.Person.Phone,
                    UserIdentityId: Guid.NewGuid(),
                    DefaultAdvertisementPickupAddressCountry: faker.Address.CountryCode(),
                    DefaultAdvertisementPickupAddressState: faker.Address.State(),
                    DefaultAdvertisementPickupAddressZipCode: faker.Address.ZipCode(),
                    DefaultAdvertisementPickupAddressCity: faker.Address.City(),
                    DefaultAdvertisementPickupAddressStreet: faker.Address.StreetName(),
                    DefaultAdvertisementPickupAddressBuildingNumber: faker.Address.BuildingNumber(),
                    DefaultAdvertisementContactInfoEmail: faker.Person.Email,
                    DefaultAdvertisementContactInfoPhoneNumber: faker.Person.Phone
                )).Generate();

    private readonly CreateCat.CreateCatRequest _createCatRequest =
        new Faker<CreateCat.CreateCatRequest>()
            .CustomInstantiator(faker =>
                new CreateCat.CreateCatRequest(
                    Name: faker.Name.FirstName(),
                    IsCastrated: true,
                    MedicalHelpUrgency: MedicalHelpUrgency.NoNeed.Name,
                    Behavior: Behavior.Friendly.Name,
                    HealthStatus: HealthStatus.Good.Name,
                    AgeCategory: AgeCategory.Baby.Name,
                    AdditionalRequirements: "Lorem ipsum"
                )).Generate();

    [Fact]
    public async Task UpdateCat_ShouldReturnSuccess_WhenValidDataIsProvided()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        ApiResponses.CreatedWithIdResponse personRegisterResponse =
            await personRegisterResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();

        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats", _createCatRequest);
        ApiResponses.CreatedWithIdResponse catCreateResponse =
            await catCreateResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();

        CatResponse catBeforeUpdate =
            await _httpClient.GetFromJsonAsync<CatResponse>(
                $"api/v1/persons/{personRegisterResponse.Id}/cats/{catCreateResponse.Id}")
            ?? throw new JsonException();

        //Act
        UpdateCat.UpdateCatRequest request = new Faker<UpdateCat.UpdateCatRequest>()
            .CustomInstantiator(faker =>
                new UpdateCat.UpdateCatRequest(
                    Name: faker.Name.FirstName(),
                    IsCastrated: false,
                    MedicalHelpUrgency: MedicalHelpUrgency.ShouldSeeVet.Name,
                    Behavior: Behavior.Unfriendly.Name,
                    HealthStatus: HealthStatus.Critical.Name,
                    AgeCategory: AgeCategory.Senior.Name,
                    AdditionalRequirements: "Lorem ipsum dolor sit"
                )).Generate();

        HttpResponseMessage updateResponse =
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats/{catCreateResponse.Id}",
                request);

        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        CatHateoasResponse? hateoasResponse = await updateResponse.Content.ReadFromJsonAsync<CatHateoasResponse>();
        hateoasResponse.Should().NotBeNull();
        hateoasResponse!.Id.Should().Be(catCreateResponse.Id);
        hateoasResponse.Links.Select(x => x.Rel).Should()
            .BeEquivalentTo(EndpointNames.SelfRel,
                EndpointNames.UpdateCat.Rel,
                EndpointNames.DeleteCat.Rel,
                EndpointNames.UpdateCatThumbnail.Rel);
        hateoasResponse.Links.Select(x => x.Href).All(x => x.Contains("://")).Should().BeTrue();
        CatResponse catAfterUpdate =
            await _httpClient.GetFromJsonAsync<CatResponse>(
                $"api/v1/persons/{personRegisterResponse.Id}/cats/{catCreateResponse.Id}")
            ?? throw new JsonException();
        catBeforeUpdate.Should().NotBeEquivalentTo(catAfterUpdate);
        catAfterUpdate.Name.Should().Be(request.Name);
        catAfterUpdate.AdditionalRequirements.Should().Be(request.AdditionalRequirements);
        catAfterUpdate.Behavior.Should().Be(request.Behavior);
        catAfterUpdate.HealthStatus.Should().Be(request.HealthStatus);
        catAfterUpdate.AgeCategory.Should().Be(request.AgeCategory);
        catAfterUpdate.MedicalHelpUrgency.Should().Be(request.MedicalHelpUrgency);
        catAfterUpdate.IsCastrated.Should().Be(request.IsCastrated);
    }

    [Fact]
    public async Task UpdateCat_ShouldReturnSuccess_WhenCatWithAssignedAdvertisementIsProvided()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        ApiResponses.CreatedWithIdResponse createPersonResponse =
            await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync();

        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{createPersonResponse.Id}/cats", _createCatRequest);
        ApiResponses.CreatedWithIdResponse createCatResponse =
            await catCreateResponseMessage.GetIdResponseFromResponseMessageAsync();

        CreateAdvertisement.CreateAdvertisementRequest createAdvertisementRequest =
            new CreateAdvertisement.CreateAdvertisementRequest(
                CatsIdsToAssign: [createCatResponse.Id],
                Description: _createCatRequest.AdditionalRequirements,
                PickupAddressCountry: _createPersonRequest.DefaultAdvertisementPickupAddressCountry,
                PickupAddressState: _createPersonRequest.DefaultAdvertisementPickupAddressState,
                PickupAddressZipCode: _createPersonRequest.DefaultAdvertisementPickupAddressZipCode,
                PickupAddressCity: _createPersonRequest.DefaultAdvertisementPickupAddressCity,
                PickupAddressStreet: _createPersonRequest.DefaultAdvertisementPickupAddressStreet,
                PickupAddressBuildingNumber: _createPersonRequest.DefaultAdvertisementPickupAddressBuildingNumber,
                ContactInfoEmail: _createPersonRequest.DefaultAdvertisementContactInfoEmail,
                ContactInfoPhoneNumber: _createPersonRequest.DefaultAdvertisementContactInfoPhoneNumber);
        HttpResponseMessage createAdvertisementResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{createPersonResponse.Id}/advertisements", createAdvertisementRequest);
        ApiResponses.CreatedWithIdResponse createAdvertisementResponse =
            await createAdvertisementResponseMessage.GetIdResponseFromResponseMessageAsync();

        CatResponse catBeforeUpdate =
            await _httpClient.GetFromJsonAsync<CatResponse>(
                $"api/v1/persons/{createPersonResponse.Id}/cats/{createCatResponse.Id}")
            ?? throw new JsonException();

        //Act
        UpdateCat.UpdateCatRequest request = new Faker<UpdateCat.UpdateCatRequest>()
            .CustomInstantiator(faker =>
                new UpdateCat.UpdateCatRequest(
                    Name: faker.Name.FirstName(),
                    IsCastrated: false,
                    MedicalHelpUrgency: MedicalHelpUrgency.ShouldSeeVet.Name,
                    Behavior: Behavior.Unfriendly.Name,
                    HealthStatus: HealthStatus.Critical.Name,
                    AgeCategory: AgeCategory.Senior.Name,
                    AdditionalRequirements: "Lorem ipsum dolor sit"
                )).Generate();
        HttpResponseMessage updateResponse =
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{createPersonResponse.Id}/cats/{createCatResponse.Id}",
                request);

        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        CatHateoasResponse? hateoasResponse = await updateResponse.Content.ReadFromJsonAsync<CatHateoasResponse>();
        hateoasResponse.Should().NotBeNull();
        hateoasResponse!.Id.Should().Be(createCatResponse.Id);
        hateoasResponse.Links.Select(x => x.Rel).Should()
            .BeEquivalentTo(EndpointNames.SelfRel,
                EndpointNames.UpdateCat.Rel,
                EndpointNames.DeleteCat.Rel,
                EndpointNames.UpdateCatThumbnail.Rel,
                EndpointNames.GetAdvertisement.Rel);
        hateoasResponse.Links.Select(x => x.Href).All(x => x.Contains("://")).Should().BeTrue();
        CatResponse catAfterUpdate =
            await _httpClient.GetFromJsonAsync<CatResponse>(
                $"api/v1/persons/{createPersonResponse.Id}/cats/{createCatResponse.Id}")
            ?? throw new JsonException();
        catBeforeUpdate.Should().NotBeEquivalentTo(catAfterUpdate);
        catAfterUpdate.Name.Should().Be(request.Name);
        catAfterUpdate.AdditionalRequirements.Should().Be(request.AdditionalRequirements);
        catAfterUpdate.Behavior.Should().Be(request.Behavior);
        catAfterUpdate.HealthStatus.Should().Be(request.HealthStatus);
        catAfterUpdate.AgeCategory.Should().Be(request.AgeCategory);
        catAfterUpdate.MedicalHelpUrgency.Should().Be(request.MedicalHelpUrgency);
        catAfterUpdate.IsCastrated.Should().Be(request.IsCastrated);
        AdvertisementResponse advertisementAfterUpdate =
            await _httpClient.GetFromJsonAsync<AdvertisementResponse>(
                $"api/v1/persons/{createPersonResponse.Id}/advertisements/{createAdvertisementResponse.Id}")
            ?? throw new JsonException();
        advertisementAfterUpdate.PriorityScore.Should().Be(catAfterUpdate.PriorityScore);
    }

    [Fact]
    public async Task UpdateCat_ShouldReturnSuccess_WhenValidDataIsProvidedWithoutAdditionalParameters()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        ApiResponses.CreatedWithIdResponse personRegisterResponse =
            await personRegisterResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats", _createCatRequest);
        ApiResponses.CreatedWithIdResponse catCreateResponse =
            await catCreateResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();

        CatResponse catBeforeUpdate =
            await _httpClient.GetFromJsonAsync<CatResponse>(
                $"api/v1/persons/{personRegisterResponse.Id}/cats/{catCreateResponse.Id}")
            ?? throw new JsonException();

        //Act
        UpdateCat.UpdateCatRequest request = new Faker<UpdateCat.UpdateCatRequest>()
            .CustomInstantiator(faker =>
                new UpdateCat.UpdateCatRequest(
                    Name: faker.Name.FirstName(),
                    IsCastrated: false,
                    MedicalHelpUrgency: MedicalHelpUrgency.ShouldSeeVet.Name,
                    Behavior: Behavior.Unfriendly.Name,
                    HealthStatus: HealthStatus.Critical.Name,
                    AgeCategory: AgeCategory.Senior.Name,
                    AdditionalRequirements: null
                )).Generate();

        HttpResponseMessage updateResponse =
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats/{catCreateResponse.Id}",
                request);

        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        CatHateoasResponse? hateoasResponse = await updateResponse.Content.ReadFromJsonAsync<CatHateoasResponse>();
        hateoasResponse.Should().NotBeNull();
        hateoasResponse!.Id.Should().Be(catCreateResponse.Id);
        hateoasResponse.Links.Select(x => x.Rel).Should()
            .BeEquivalentTo(EndpointNames.SelfRel,
                EndpointNames.UpdateCat.Rel,
                EndpointNames.DeleteCat.Rel,
                EndpointNames.UpdateCatThumbnail.Rel);
        hateoasResponse.Links.Select(x => x.Href).All(x => x.Contains("://")).Should().BeTrue();
        
        CatResponse catAfterUpdate =
            await _httpClient.GetFromJsonAsync<CatResponse>(
                $"api/v1/persons/{personRegisterResponse.Id}/cats/{catCreateResponse.Id}")
            ?? throw new JsonException();
        
        catBeforeUpdate.Should().NotBeEquivalentTo(catAfterUpdate);
        catAfterUpdate.AdditionalRequirements.Should().Be(string.Empty);
        catAfterUpdate.Name.Should().Be(request.Name);
        catAfterUpdate.Behavior.Should().Be(request.Behavior);
        catAfterUpdate.HealthStatus.Should().Be(request.HealthStatus);
        catAfterUpdate.AgeCategory.Should().Be(request.AgeCategory);
        catAfterUpdate.MedicalHelpUrgency.Should().Be(request.MedicalHelpUrgency);
        catAfterUpdate.IsCastrated.Should().Be(request.IsCastrated);
    }

    [Fact]
    public async Task UpdateCat_ShouldReturnSuccess_WhenEmptyAdditionalRequirementsAreProvided()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        ApiResponses.CreatedWithIdResponse personRegisterResponse =
            await personRegisterResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats", _createCatRequest);
        ApiResponses.CreatedWithIdResponse catCreateResponse =
            await catCreateResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();

        CatResponse catBeforeUpdate =
            await _httpClient.GetFromJsonAsync<CatResponse>(
                $"api/v1/persons/{personRegisterResponse.Id}/cats/{catCreateResponse.Id}")
            ?? throw new JsonException();

        //Act
        UpdateCat.UpdateCatRequest request = new Faker<UpdateCat.UpdateCatRequest>()
            .CustomInstantiator(faker =>
                new UpdateCat.UpdateCatRequest(
                    Name: faker.Name.FirstName(),
                    IsCastrated: false,
                    MedicalHelpUrgency: MedicalHelpUrgency.ShouldSeeVet.Name,
                    Behavior: Behavior.Unfriendly.Name,
                    HealthStatus: HealthStatus.Critical.Name,
                    AgeCategory: AgeCategory.Senior.Name,
                    AdditionalRequirements: " "
                )).Generate();

        HttpResponseMessage updateResponse =
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats/{catCreateResponse.Id}",
                request);

        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        CatHateoasResponse? hateoasResponse = await updateResponse.Content.ReadFromJsonAsync<CatHateoasResponse>();
        hateoasResponse.Should().NotBeNull();
        hateoasResponse!.Id.Should().Be(catCreateResponse.Id);
        hateoasResponse.Links.Select(x => x.Rel).Should()
            .BeEquivalentTo(EndpointNames.SelfRel,
                EndpointNames.UpdateCat.Rel,
                EndpointNames.DeleteCat.Rel,
                EndpointNames.UpdateCatThumbnail.Rel);
        hateoasResponse.Links.Select(x => x.Href).All(x => x.Contains("://")).Should().BeTrue();
        
        CatResponse catAfterUpdate =
            await _httpClient.GetFromJsonAsync<CatResponse>(
                $"api/v1/persons/{personRegisterResponse.Id}/cats/{catCreateResponse.Id}")
            ?? throw new JsonException();
        catBeforeUpdate.Should().NotBeEquivalentTo(catAfterUpdate);
        catAfterUpdate.AdditionalRequirements.Should().Be(string.Empty);
        catAfterUpdate.Name.Should().Be(request.Name);
        catAfterUpdate.Behavior.Should().Be(request.Behavior);
        catAfterUpdate.HealthStatus.Should().Be(request.HealthStatus);
        catAfterUpdate.AgeCategory.Should().Be(request.AgeCategory);
        catAfterUpdate.MedicalHelpUrgency.Should().Be(request.MedicalHelpUrgency);
        catAfterUpdate.IsCastrated.Should().Be(request.IsCastrated);
    }

    [Fact]
    public async Task UpdateCat_ShouldReturnNotFound_WhenNonExistingCatIsProvided()
    {
        //Arrange
        Guid randomId = Guid.NewGuid();

        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        ApiResponses.CreatedWithIdResponse personRegisterResponse =
            await personRegisterResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();

        //Act
        UpdateCat.UpdateCatRequest request = new Faker<UpdateCat.UpdateCatRequest>()
            .CustomInstantiator(faker =>
                new UpdateCat.UpdateCatRequest(
                    Name: faker.Name.FirstName(),
                    IsCastrated: false,
                    MedicalHelpUrgency: MedicalHelpUrgency.ShouldSeeVet.Name,
                    Behavior: Behavior.Unfriendly.Name,
                    HealthStatus: HealthStatus.Critical.Name,
                    AgeCategory: AgeCategory.Senior.Name,
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

        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
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
                    MedicalHelpUrgency: MedicalHelpUrgency.ShouldSeeVet.Name,
                    Behavior: Behavior.Unfriendly.Name,
                    HealthStatus: HealthStatus.Critical.Name,
                    AgeCategory: AgeCategory.Senior.Name,
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
    public async Task UpdateCat_ShouldReturnBadRequest_WhenEmptyDataAreProvided()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
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
                MedicalHelpUrgency: "",
                Behavior: "",
                HealthStatus: "",
                AgeCategory: ""
            );
        HttpResponseMessage response =
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats/{catCreateResponse.Id}",
                request);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(5);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo(
            nameof(UpdateCat.UpdateCatRequest.Name),
            nameof(UpdateCat.UpdateCatRequest.MedicalHelpUrgency),
            nameof(UpdateCat.UpdateCatRequest.Behavior),
            nameof(UpdateCat.UpdateCatRequest.AgeCategory),
            nameof(UpdateCat.UpdateCatRequest.HealthStatus)
        );
        validationProblemDetails.Errors.Values.Count.Should().Be(5);
        validationProblemDetails.Errors[nameof(UpdateCat.UpdateCatRequest.Name)][0]
            .Should()
            .Be("'Name' must not be empty.");

        validationProblemDetails.Errors[nameof(UpdateCat.UpdateCatRequest.MedicalHelpUrgency)][0]
            .Should()
            .Be("'Medical Help Urgency' must not be empty.");

        validationProblemDetails.Errors[nameof(UpdateCat.UpdateCatRequest.Behavior)][0]
            .Should()
            .Be("'Behavior' must not be empty.");

        validationProblemDetails.Errors[nameof(UpdateCat.UpdateCatRequest.AgeCategory)][0]
            .Should()
            .Be("'Age Category' must not be empty.");

        validationProblemDetails.Errors[nameof(UpdateCat.UpdateCatRequest.HealthStatus)][0]
            .Should()
            .Be("'Health Status' must not be empty.");
    }

    [Fact]
    public async Task UpdateCat_ShouldReturnBadRequest_WhenTooLongDataAreProvided()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
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
                    Name: faker.Person.FirstName.ClampLength(CatName.MaxLength + 1),
                    IsCastrated: true,
                    MedicalHelpUrgency: MedicalHelpUrgency.ShouldSeeVet.Name,
                    Behavior: Behavior.Unfriendly.Name,
                    HealthStatus: HealthStatus.Poor.Name,
                    AgeCategory: AgeCategory.Baby.Name,
                    AdditionalRequirements: faker.Address.State().ClampLength(Description.MaxLength + 1)
                )).Generate();

        HttpResponseMessage response =
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats/{catCreateResponse.Id}",
                request);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(2);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo(
            nameof(UpdateCat.UpdateCatRequest.Name),
            nameof(UpdateCat.UpdateCatRequest.AdditionalRequirements)
        );
        validationProblemDetails.Errors.Values.Count.Should().Be(2);

        validationProblemDetails.Errors[nameof(UpdateCat.UpdateCatRequest.Name)][0]
            .Should()
            .StartWith($"The length of 'Name' must be {CatName.MaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(UpdateCat.UpdateCatRequest.AdditionalRequirements)][0]
            .Should()
            .StartWith(
                $"The length of 'Additional Requirements' must be {Description.MaxLength} characters or fewer. You entered");
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
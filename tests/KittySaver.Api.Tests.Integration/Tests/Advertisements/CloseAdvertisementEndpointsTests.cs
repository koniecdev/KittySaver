using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Features.Advertisements;
using KittySaver.Api.Shared.Endpoints;
using KittySaver.Api.Tests.Integration.Helpers;
using KittySaver.Domain.Common.Primitives.Enums;
using KittySaver.Shared.Hateoas;
using KittySaver.Shared.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using KittySaver.Tests.Shared;

namespace KittySaver.Api.Tests.Integration.Tests.Advertisements;

[Collection("Api")]
public class CloseAdvertisementEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CleanupHelper _cleanup;
    public CloseAdvertisementEndpointsTests(KittySaverApiFactory appFactory)
    {
        _httpClient = appFactory.CreateClient();
        _cleanup = new CleanupHelper(_httpClient);
    }

    private readonly Faker<CreatePersonRequest> _createPersonRequestGenerator =
        new Faker<CreatePersonRequest>()
            .CustomInstantiator(faker =>
                new CreatePersonRequest(
                    Nickname: faker.Person.FirstName,
                    Email: faker.Person.Email,
                    PhoneNumber: faker.Person.Phone,
                    Password: "Default123$",
                    DefaultAdvertisementPickupAddressCountry: faker.Address.CountryCode(),
                    DefaultAdvertisementPickupAddressState: faker.Address.State(),
                    DefaultAdvertisementPickupAddressZipCode: faker.Address.ZipCode(),
                    DefaultAdvertisementPickupAddressCity: faker.Address.City(),
                    DefaultAdvertisementPickupAddressStreet: faker.Address.StreetName(),
                    DefaultAdvertisementPickupAddressBuildingNumber: faker.Address.BuildingNumber(),
                    DefaultAdvertisementContactInfoEmail: faker.Person.Email,
                    DefaultAdvertisementContactInfoPhoneNumber: faker.Person.Phone
                ));

    private readonly Faker<CreateCatRequest> _createCatRequestGenerator =
        new Faker<CreateCatRequest>()
            .CustomInstantiator(faker =>
                new CreateCatRequest(
                    Name: faker.Name.FirstName(),
                    IsCastrated: true,
                    MedicalHelpUrgency: MedicalHelpUrgency.NoNeed.Name,
                    Behavior: Behavior.Friendly.Name,
                    HealthStatus: HealthStatus.Good.Name,
                    AgeCategory: AgeCategory.Adult.Name,
                    AdditionalRequirements: "Lorem ipsum"
                ));
    
    [Fact]
    public async Task CloseAdvertisement_ShouldReturnSuccess_WhenValidDataIsProvided()
    {
        //Arrange
        CreatePersonRequest personRegisterRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", personRegisterRequest);
        ApiResponses.CreatedWithIdResponse personRegisterResponse = await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync();
        CreateCatRequest catCreateRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats", catCreateRequest);
        ApiResponses.CreatedWithIdResponse catCreateResponse = await catCreateResponseMessage.GetIdResponseFromResponseMessageAsync();
        
        CreateAdvertisementRequest request =
            new Faker<CreateAdvertisementRequest>()
                .CustomInstantiator(faker =>
                    new CreateAdvertisementRequest(
                        CatsIdsToAssign: [catCreateResponse.Id],
                        Description: faker.Lorem.Lines(2),
                        PickupAddressCountry: faker.Address.CountryCode(),
                        PickupAddressState: faker.Address.State(),
                        PickupAddressZipCode: faker.Address.ZipCode(),
                        PickupAddressCity: faker.Address.City(),
                        PickupAddressStreet: faker.Address.StreetName(),
                        PickupAddressBuildingNumber: faker.Address.BuildingNumber(),
                        ContactInfoEmail: faker.Person.Email,
                        ContactInfoPhoneNumber: faker.Person.Phone
                    ));

        HttpResponseMessage advertisementResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/advertisements", request);
        ApiResponses.CreatedWithIdResponse advertisementResponse = await advertisementResponseMessage.GetIdResponseFromResponseMessageAsync();
        
        await using Stream imageStream = CreateTestImageHelper.Create();
        using MultipartFormDataContent content = new();
        StreamContent imageContent = new(imageStream);
        imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        content.Add(imageContent, "thumbnail", "test.jpg");
        await _httpClient.PutAsync(
            $"/api/v1/persons/{personRegisterResponse.Id}/advertisements/{advertisementResponse.Id}/thumbnail", 
            content);
        
        //Act
        HttpResponseMessage closeResponseMessage = 
            await _httpClient.PostAsync($"api/v1/persons/{personRegisterResponse.Id}/advertisements/{advertisementResponse.Id}/close", null);
        
        //Assert
        closeResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        AdvertisementHateoasResponse? hateoasResponse = await closeResponseMessage.Content.ReadFromJsonAsync<AdvertisementHateoasResponse>();
        hateoasResponse.Should().NotBeNull();
        hateoasResponse!.Id.Should().Be(advertisementResponse.Id);
        hateoasResponse.PersonId.Should().Be(personRegisterResponse.Id);
        hateoasResponse.Status.Should().Be(AdvertisementStatus.Closed);
        hateoasResponse.Links.Count.Should().Be(2);
        hateoasResponse.Links.Select(x => x.Rel).Should()
            .BeEquivalentTo(EndpointRels.SelfRel, EndpointNames.GetAdvertisementThumbnail.Rel);
        hateoasResponse.Links.Select(x => x.Href).All(x => x.Contains("://")).Should().BeTrue();

        
        CatResponse catAfterClosure =
            await _httpClient.GetFromJsonAsync<CatResponse>($"api/v1/persons/{personRegisterResponse.Id}/cats/{catCreateResponse.Id}")
            ?? throw new JsonException();
        catAfterClosure.IsAdopted.Should().BeTrue();
        AdvertisementResponse advertisement =
            await _httpClient.GetFromJsonAsync<AdvertisementResponse>(
                $"api/v1/persons/{personRegisterResponse.Id}/advertisements/{advertisementResponse.Id}")
            ?? throw new JsonException();
        advertisement.Status.Should().Be(AdvertisementStatus.Closed);
    }
    
    [Fact]
    public async Task CloseAdvertisement_ShouldReturnBadRequest_WhenDuplicatedRequestOccur()
    {
        //Arrange
        CreatePersonRequest personRegisterRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", personRegisterRequest);
        ApiResponses.CreatedWithIdResponse personRegisterResponse = await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync();
        CreateCatRequest catCreateRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats", catCreateRequest);
        ApiResponses.CreatedWithIdResponse catCreateResponse = await catCreateResponseMessage.GetIdResponseFromResponseMessageAsync();
        
        CreateAdvertisementRequest request =
            new Faker<CreateAdvertisementRequest>()
                .CustomInstantiator(faker =>
                    new CreateAdvertisementRequest(
                        CatsIdsToAssign: [catCreateResponse.Id],
                        Description: faker.Lorem.Lines(2),
                        PickupAddressCountry: faker.Address.CountryCode(),
                        PickupAddressState: faker.Address.State(),
                        PickupAddressZipCode: faker.Address.ZipCode(),
                        PickupAddressCity: faker.Address.City(),
                        PickupAddressStreet: faker.Address.StreetName(),
                        PickupAddressBuildingNumber: faker.Address.BuildingNumber(),
                        ContactInfoEmail: faker.Person.Email,
                        ContactInfoPhoneNumber: faker.Person.Phone
                    ));

        HttpResponseMessage advertisementResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/advertisements", request);
        ApiResponses.CreatedWithIdResponse advertisementResponse = await advertisementResponseMessage.GetIdResponseFromResponseMessageAsync();
        await _httpClient.PostAsync($"api/v1/persons/{personRegisterResponse.Id}/advertisements/{advertisementResponse.Id}/close", null);
        
        //Act
        HttpResponseMessage duplicatedCloseResponseMessage = 
            await _httpClient.PostAsync($"api/v1/persons/{personRegisterResponse.Id}/advertisements/{advertisementResponse.Id}/close", null);
        
        //Assert
        duplicatedCloseResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ProblemDetails problemDetails = await duplicatedCloseResponseMessage.GetResponseFromResponseMessageAsync<ProblemDetails>()
                                        ?? throw new JsonException();
        problemDetails.Detail.Should().StartWith("Active advertisement status is required for that operation.");
    }
    
    [Fact]
    public async Task CloseAdvertisement_ShouldReturnBadRequest_WhenSameIdsAreProvided()
    {
        //Arrange
        Guid randomId = Guid.NewGuid();
        
        //Act
        HttpResponseMessage closeResponseMessage =
            await _httpClient.PostAsync($"api/v1/persons/{randomId}/advertisements/{randomId}/close", null);
        
        //Assert
        closeResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await closeResponseMessage.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(2);
        validationProblemDetails.Errors.Keys.Should()
            .BeEquivalentTo(nameof(CloseAdvertisement.CloseAdvertisementCommand.PersonId),
                nameof(CloseAdvertisement.CloseAdvertisementCommand.Id));
        validationProblemDetails.Errors.Values.Count.Should().Be(2);
        validationProblemDetails.Errors[nameof(CloseAdvertisement.CloseAdvertisementCommand.Id)][0]
            .Should().Be($"'Id' must not be equal to '{randomId}'.");
        validationProblemDetails.Errors[nameof(CloseAdvertisement.CloseAdvertisementCommand.PersonId)][0]
            .Should().Be($"'Person Id' must not be equal to '{randomId}'.");
    }
    
    [Fact]
    public async Task CloseAdvertisement_ShouldReturnNotFound_WhenInvaliIdIsProvided()
    {
        //Arrange
        Guid randomPersonId = Guid.NewGuid();
        Guid randomAdvertisementId = Guid.NewGuid();
        
        //Act
        HttpResponseMessage closeResponseMessage =
            await _httpClient.PostAsync($"api/v1/persons/{randomPersonId}/advertisements/{randomAdvertisementId}/close", null);
        
        //Assert
        closeResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails notFoundProblemDetails = await closeResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>()
                                                 ?? throw new JsonException();
        notFoundProblemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        notFoundProblemDetails.Detail.Should().Be($"'Person' with identifier '{randomPersonId}' was not found.");
    }
    
    [Fact]
    public async Task CloseAdvertisement_ShouldReturnBadRequest_WhenEmptyDataAreProvided()
    {
        //Arrange
        Guid randomId = Guid.Empty;
        // CreatePersonRequest? owner = _createPersonRequestGenerator.Generate();
        // HttpResponseMessage ownerResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/persons", owner);
        // ApiResponses.CreatedWithIdResponse ownerResponse = await ownerResponseMessage.GetIdResponseFromResponseMessageAsync();
        
        //Act
        HttpResponseMessage closeResponse = 
            await _httpClient.PostAsync($"api/v1/persons/{randomId}/advertisements/{randomId}/close", null);
        
        //Assert
        closeResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await closeResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(2);
        validationProblemDetails.Errors.Keys.Should()
            .BeEquivalentTo(nameof(CloseAdvertisement.CloseAdvertisementCommand.PersonId),
                nameof(CloseAdvertisement.CloseAdvertisementCommand.Id));
        validationProblemDetails.Errors.Values.Count.Should().Be(2);
        validationProblemDetails.Errors[nameof(CloseAdvertisement.CloseAdvertisementCommand.Id)][0]
            .Should().Be("'Id' must not be empty.");
        validationProblemDetails.Errors[nameof(CloseAdvertisement.CloseAdvertisementCommand.PersonId)][0]
            .Should().Be("'Person Id' must not be empty.");
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
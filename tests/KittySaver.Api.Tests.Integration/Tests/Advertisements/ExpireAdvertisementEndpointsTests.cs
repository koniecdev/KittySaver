using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Features.Advertisements;
using KittySaver.Api.Features.Advertisements.SharedContracts;
using KittySaver.Api.Features.Persons;
using KittySaver.Api.Shared.Hateoas;
using KittySaver.Api.Tests.Integration.Helpers;
using KittySaver.Domain.Common.Primitives.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace KittySaver.Api.Tests.Integration.Tests.Advertisements;

[Collection("Api")]
public class ExpireAdvertisementEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CleanupHelper _cleanup;

    public ExpireAdvertisementEndpointsTests(KittySaverApiFactory appFactory)
    {
        _httpClient = appFactory.CreateClient();
        _cleanup = new CleanupHelper(_httpClient);
    }

    private readonly Faker<CreatePerson.CreatePersonRequest> _createPersonRequestGenerator =
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
                ));

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
    public async Task ExpireAdvertisement_ShouldReturnSuccess_WhenValidDataIsProvided()
    {
        //Arrange
        CreatePerson.CreatePersonRequest personRegisterRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", personRegisterRequest);
        ApiResponses.CreatedWithIdResponse personRegisterResponse =
            await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync();
        CreateCat.CreateCatRequest catCreateRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats", catCreateRequest);
        ApiResponses.CreatedWithIdResponse catCreateResponse =
            await catCreateResponseMessage.GetIdResponseFromResponseMessageAsync();

        CreateAdvertisement.CreateAdvertisementRequest request =
            new Faker<CreateAdvertisement.CreateAdvertisementRequest>()
                .CustomInstantiator(faker =>
                    new CreateAdvertisement.CreateAdvertisementRequest(
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
        ApiResponses.CreatedWithIdResponse advertisementResponse =
            await advertisementResponseMessage.GetIdResponseFromResponseMessageAsync();

        //Act
        HttpResponseMessage expireResponseMessage =
            await _httpClient.PostAsync($"api/v1/persons/{personRegisterResponse.Id}/advertisements/{advertisementResponse.Id}/expire", null);

        //Assert
        expireResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        AdvertisementHateoasResponse? hateoasResponse = await expireResponseMessage.Content.ReadFromJsonAsync<AdvertisementHateoasResponse>();
        hateoasResponse.Should().NotBeNull();
        hateoasResponse!.Id.Should().Be(advertisementResponse.Id);
        hateoasResponse.PersonId.Should().Be(personRegisterResponse.Id);
        hateoasResponse.Status.Should().Be(AdvertisementResponse.AdvertisementStatus.Expired);
        hateoasResponse.Links.Count.Should().Be(3);
        AdvertisementResponse advertisement =
            await _httpClient.GetFromJsonAsync<AdvertisementResponse>(
                $"api/v1/advertisements/{advertisementResponse.Id}") ?? throw new JsonException();
        advertisement.Status.Should().Be(AdvertisementResponse.AdvertisementStatus.Expired);
    }

    [Fact]
    public async Task ExpireAdvertisement_ShouldReturnBadRequest_WhenDuplicatedRequestOccur()
    {
        //Arrange
        CreatePerson.CreatePersonRequest personRegisterRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", personRegisterRequest);
        ApiResponses.CreatedWithIdResponse personRegisterResponse =
            await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync();
        CreateCat.CreateCatRequest catCreateRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats", catCreateRequest);
        ApiResponses.CreatedWithIdResponse catCreateResponse =
            await catCreateResponseMessage.GetIdResponseFromResponseMessageAsync();

        CreateAdvertisement.CreateAdvertisementRequest request =
            new Faker<CreateAdvertisement.CreateAdvertisementRequest>()
                .CustomInstantiator(faker =>
                    new CreateAdvertisement.CreateAdvertisementRequest(
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
        ApiResponses.CreatedWithIdResponse advertisementResponse =
            await advertisementResponseMessage.GetIdResponseFromResponseMessageAsync();
        await _httpClient.PostAsync($"api/v1/persons/{personRegisterResponse.Id}/advertisements/{advertisementResponse.Id}/expire", null);

        //Act
        HttpResponseMessage duplicatedExpireResponseMessage =
            await _httpClient.PostAsync($"api/v1/persons/{personRegisterResponse.Id}/advertisements/{advertisementResponse.Id}/expire", null);

        //Assert
        duplicatedExpireResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ProblemDetails problemDetails =
            await duplicatedExpireResponseMessage.GetResponseFromResponseMessageAsync<ProblemDetails>()
            ?? throw new JsonException();
        problemDetails.Detail.Should().StartWith("Active advertisement status is required for that operation.");
    }

    [Fact]
    public async Task ExpireAdvertisement_ShouldReturnNotFound_WhenInvalidPersonIdIsProvided()
    {
        //Arrange
        Guid randomPersonId = Guid.NewGuid();
        Guid randomAdvertisementId = Guid.NewGuid();

        //Act
        HttpResponseMessage expireResponseMessage =
            await _httpClient.PostAsync($"api/v1/persons/{randomPersonId}/advertisements/{randomAdvertisementId}/expire", null);

        //Assert
        expireResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails notFoundProblemDetails = await expireResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>()
                                                ?? throw new JsonException();
        notFoundProblemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
    }
    
    [Fact]
    public async Task ExpireAdvertisement_ShouldReturnNotFound_WhenInvalidAdvertisementIdIsProvided()
    {
        //Arrange
        CreatePerson.CreatePersonRequest personRegisterRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", personRegisterRequest);
        ApiResponses.CreatedWithIdResponse personRegisterResponse =
            await personRegisterResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        Guid randomAdvertisementId = Guid.NewGuid();

        //Act
        HttpResponseMessage expireResponseMessage =
            await _httpClient.PostAsync($"api/v1/persons/{personRegisterResponse.Id}/advertisements/{randomAdvertisementId}/expire", null);

        //Assert
        expireResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails notFoundProblemDetails = await expireResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>()
                                                ?? throw new JsonException();
        notFoundProblemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task ExpireAdvertisement_ShouldReturnBadRequest_WhenEmptyDataAreProvided()
    {
        //Arrange
        Guid emptyAdvertisementId = Guid.Empty;
        Guid randomPersonId = Guid.NewGuid();

        //Act
        HttpResponseMessage expireResponse =
            await _httpClient.PostAsync($"api/v1/persons/{randomPersonId}/advertisements/{emptyAdvertisementId}/expire", null);

        //Assert
        expireResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await expireResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(1);
        validationProblemDetails.Errors.Keys.Should()
            .BeEquivalentTo(nameof(ExpireAdvertisement.ExpireAdvertisementCommand.Id));
        validationProblemDetails.Errors.Values.Count.Should().Be(1);
        validationProblemDetails.Errors[nameof(ExpireAdvertisement.ExpireAdvertisementCommand.Id)][0]
            .Should().Be("'Id' must not be empty.");
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
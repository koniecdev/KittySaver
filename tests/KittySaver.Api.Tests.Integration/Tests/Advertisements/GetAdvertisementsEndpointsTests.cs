using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Tests.Integration.Helpers;
using KittySaver.Domain.Common.Primitives.Enums;
using KittySaver.Shared.Hateoas;
using KittySaver.Shared.Pagination;
using KittySaver.Shared.Responses;
using KittySaver.Tests.Shared;

namespace KittySaver.Api.Tests.Integration.Tests.Advertisements;

[Collection("Api")]
public class GetAdvertisementsEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CleanupHelper _cleanup;

    public GetAdvertisementsEndpointsTests(KittySaverApiFactory appFactory)
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
    public async Task GetAdvertisements_ShouldReturnTwoAdvertisements_WhenTwoAdvertisementExists()
    {
        //Arrange
        CreatePersonRequest personRegisterRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", personRegisterRequest);
        ApiResponses.CreatedWithIdResponse personRegisterResponse =
            await personRegisterResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        
        CreateCatRequest catCreateRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats", catCreateRequest);
        ApiResponses.CreatedWithIdResponse catCreateResponse =
            await catCreateResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();

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

        await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/advertisements", request);

        CreatePersonRequest secondPersonRegisterRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage secondPersonRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", secondPersonRegisterRequest);
        ApiResponses.CreatedWithIdResponse secondPersonRegisterResponse =
            await secondPersonRegisterResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        CreateCatRequest secondCatCreateRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage secondCatCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{secondPersonRegisterResponse.Id}/cats",
                secondCatCreateRequest);
        ApiResponses.CreatedWithIdResponse secondCatCreateResponse =
            await secondCatCreateResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();

        CreateAdvertisementRequest secondRequest =
            new Faker<CreateAdvertisementRequest>()
                .CustomInstantiator(faker =>
                    new CreateAdvertisementRequest(
                        CatsIdsToAssign: [secondCatCreateResponse.Id],
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

        await _httpClient.PostAsJsonAsync($"api/v1/persons/{secondPersonRegisterResponse.Id}/advertisements", secondRequest);

        //Act
        HttpResponseMessage response = await _httpClient.GetAsync($"/api/v1/persons/{secondPersonRegisterResponse.Id}/advertisements");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PagedList<AdvertisementResponse>? advertisements =
            await response.Content.ReadFromJsonAsync<PagedList<AdvertisementResponse>>();
        advertisements.Should().NotBeNull();
        advertisements!.Items.Count.Should().Be(1);
        advertisements.Total.Should().Be(1);
        advertisements.Links.Count.Should().Be(2);
        advertisements.Links.Select(x=>x.Href).All(x=>x.Contains("://")).Should().BeTrue();
    }

    [Fact]
    public async Task GetAdvertisements_ShouldReturnEmptyList_WhenNoAdvertisementsExist()
    {
        //Arrange
        CreatePersonRequest personRegisterRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", personRegisterRequest);
        ApiResponses.CreatedWithIdResponse personRegisterResponse =
            await personRegisterResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        
        //Act
        HttpResponseMessage response = await _httpClient.GetAsync($"/api/v1/persons/{personRegisterResponse.Id}/advertisements");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PagedList<AdvertisementResponse>? advertisements =
            await response.Content.ReadFromJsonAsync<PagedList<AdvertisementResponse>>();
        advertisements.Should().NotBeNull();
        advertisements!.Items.Count.Should().Be(0);
        advertisements.Total.Should().Be(0);
        advertisements.Links.Count.Should().Be(2);
        advertisements.Links.First(x => x.Rel == EndpointRels.SelfRel).Href.Should().Contain("://");
        advertisements.Links.First(x => x.Rel == "by-page").Href.Should().Contain("://");
    }

    [Fact]
    public async Task GetAdvertisements_ShouldReturnOneOfTwoAdvertisements_WhenPersonIdFilterExists()
    {
        //Arrange
        CreatePersonRequest personRegisterRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", personRegisterRequest);
        ApiResponses.CreatedWithIdResponse personRegisterResponse =
            await personRegisterResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        
        CreateCatRequest catCreateRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats", catCreateRequest);
        ApiResponses.CreatedWithIdResponse catCreateResponse =
            await catCreateResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();

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
        await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/advertisements", request);

        CreatePersonRequest secondPersonRegisterRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage secondPersonRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", secondPersonRegisterRequest);
        ApiResponses.CreatedWithIdResponse secondPersonRegisterResponse =
            await secondPersonRegisterResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        
        CreateCatRequest secondCatCreateRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage secondCatCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{secondPersonRegisterResponse.Id}/cats",
                secondCatCreateRequest);
        ApiResponses.CreatedWithIdResponse secondCatCreateResponse =
            await secondCatCreateResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();

        CreateAdvertisementRequest secondRequest =
            new Faker<CreateAdvertisementRequest>()
                .CustomInstantiator(faker =>
                    new CreateAdvertisementRequest(
                        CatsIdsToAssign: [secondCatCreateResponse.Id],
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

        await _httpClient.PostAsJsonAsync($"api/v1/persons/{secondPersonRegisterResponse.Id}/advertisements", secondRequest);

        //Act
        HttpResponseMessage response = await _httpClient.GetAsync($"/api/v1/persons/{personRegisterResponse.Id}/advertisements");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PagedList<AdvertisementResponse>? advertisements =
            await response.Content.ReadFromJsonAsync<PagedList<AdvertisementResponse>>();
        advertisements?.Items.Count.Should().Be(1);
        advertisements?.Items.Count(x=>x.PersonId == personRegisterResponse.Id).Should().Be(1);
        advertisements?.Total.Should().Be(1);
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
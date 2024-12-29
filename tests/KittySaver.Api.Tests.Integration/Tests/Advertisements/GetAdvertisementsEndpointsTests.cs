using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Features.Advertisements;
using KittySaver.Api.Features.Advertisements.SharedContracts;
using KittySaver.Api.Features.Persons;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Tests.Integration.Helpers;
using KittySaver.Domain.Common.Primitives.Enums;
using Shared;

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
    public async Task GetAdvertisements_ShouldReturnTwoAdvertisements_WhenTwoAdvertisementExists()
    {
        //Arrange
        CreatePerson.CreatePersonRequest personRegisterRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", personRegisterRequest);
        ApiResponses.CreatedWithIdResponse personRegisterResponse =
            await personRegisterResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        CreateCat.CreateCatRequest catCreateRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats", catCreateRequest);
        ApiResponses.CreatedWithIdResponse catCreateResponse =
            await catCreateResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();

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

        await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/advertisements", request);

        CreatePerson.CreatePersonRequest secondPersonRegisterRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage secondPersonRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", secondPersonRegisterRequest);
        ApiResponses.CreatedWithIdResponse secondPersonRegisterResponse =
            await secondPersonRegisterResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        CreateCat.CreateCatRequest secondCatCreateRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage secondCatCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{secondPersonRegisterResponse.Id}/cats",
                secondCatCreateRequest);
        ApiResponses.CreatedWithIdResponse secondCatCreateResponse =
            await secondCatCreateResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();

        CreateAdvertisement.CreateAdvertisementRequest secondRequest =
            new Faker<CreateAdvertisement.CreateAdvertisementRequest>()
                .CustomInstantiator(faker =>
                    new CreateAdvertisement.CreateAdvertisementRequest(
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
        HttpResponseMessage response = await _httpClient.GetAsync("/api/v1/advertisements");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PagedList<AdvertisementResponse>? advertisements =
            await response.Content.ReadFromJsonAsync<PagedList<AdvertisementResponse>>();
        advertisements.Should().NotBeNull();
        advertisements!.Items.Count.Should().Be(2);
        AdvertisementResponse firstPersonAdvertisement =
            advertisements.Items.First(x => x.PersonId == personRegisterResponse.Id);
        firstPersonAdvertisement.Id.Should().NotBeEmpty();
        firstPersonAdvertisement.Cats.Count.Should().Be(1);
        firstPersonAdvertisement.Title.Should().Be(firstPersonAdvertisement.Cats.First().Name);
        firstPersonAdvertisement.PickupAddress.Country.Should().Be(request.PickupAddressCountry);
        firstPersonAdvertisement.PickupAddress.State.Should().Be(request.PickupAddressState);
        firstPersonAdvertisement.PickupAddress.ZipCode.Should().Be(request.PickupAddressZipCode);
        firstPersonAdvertisement.PickupAddress.City.Should().Be(request.PickupAddressCity);
        firstPersonAdvertisement.PickupAddress.Street.Should().Be(request.PickupAddressStreet);
        firstPersonAdvertisement.PickupAddress.BuildingNumber.Should().Be(request.PickupAddressBuildingNumber);
        firstPersonAdvertisement.PriorityScore.Should().BeGreaterThan(0);
        AdvertisementResponse secondPersonAdvertisement =
            advertisements.Items.First(x => x.PersonId == secondPersonRegisterResponse.Id);
        secondPersonAdvertisement.Id.Should().NotBeEmpty();
        secondPersonAdvertisement.Cats.Count.Should().Be(1);
        secondPersonAdvertisement.Title.Should().Be(secondPersonAdvertisement.Cats.First().Name);
        secondPersonAdvertisement.PickupAddress.Country.Should().Be(secondRequest.PickupAddressCountry);
        secondPersonAdvertisement.PickupAddress.State.Should().Be(secondRequest.PickupAddressState);
        secondPersonAdvertisement.PickupAddress.ZipCode.Should().Be(secondRequest.PickupAddressZipCode);
        secondPersonAdvertisement.PickupAddress.City.Should().Be(secondRequest.PickupAddressCity);
        secondPersonAdvertisement.PickupAddress.Street.Should().Be(secondRequest.PickupAddressStreet);
        secondPersonAdvertisement.PickupAddress.BuildingNumber.Should().Be(secondRequest.PickupAddressBuildingNumber);
        secondPersonAdvertisement.PriorityScore.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAdvertisements_ShouldReturnEmptyList_WhenNoAdvertisementsExist()
    {
        //Act
        HttpResponseMessage response = await _httpClient.GetAsync("/api/v1/advertisements");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PagedList<AdvertisementResponse>? advertisement =
            await response.Content.ReadFromJsonAsync<PagedList<AdvertisementResponse>>();
        advertisement.Should().NotBeNull();
        advertisement!.Items.Count.Should().Be(0);
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
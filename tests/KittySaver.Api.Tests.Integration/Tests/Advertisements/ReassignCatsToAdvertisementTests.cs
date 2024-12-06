using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Features.Advertisements;
using KittySaver.Api.Features.Advertisements.SharedContracts;
using KittySaver.Api.Features.Persons;
using KittySaver.Api.Tests.Integration.Helpers;
using KittySaver.Domain.Common.Primitives.Enums;
using Shared;

namespace KittySaver.Api.Tests.Integration.Tests.Advertisements;

[Collection("Api")]
public class ReassignCatsToAdvertisementTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CleanupHelper _cleanup;

    public ReassignCatsToAdvertisementTests(KittySaverApiFactory appFactory)
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
                    DefaultAdvertisementPickupAddressCountry: faker.Address.Country(),
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
    public async Task ReassignCatsToAdvertisement_ShouldReturnSuccess_WhenOneAdditionalCatIsProvided()
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
                        PersonId: personRegisterResponse.Id,
                        CatsIdsToAssign: [catCreateResponse.Id],
                        Description: faker.Lorem.Lines(2),
                        PickupAddressCountry: faker.Address.Country(),
                        PickupAddressState: faker.Address.State(),
                        PickupAddressZipCode: faker.Address.ZipCode(),
                        PickupAddressCity: faker.Address.City(),
                        PickupAddressStreet: faker.Address.StreetName(),
                        PickupAddressBuildingNumber: faker.Address.BuildingNumber(),
                        ContactInfoEmail: faker.Person.Email,
                        ContactInfoPhoneNumber: faker.Person.Phone
                    ));

        HttpResponseMessage createAdvertisementResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/advertisements", request);
        ApiResponses.CreatedWithIdResponse createAdvertisementResponse =
            await createAdvertisementResponseMessage.GetIdResponseFromResponseMessageAsync();

        //Act
        CreateCat.CreateCatRequest anotherCatCreateRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage anotherCatCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats",
                anotherCatCreateRequest);
        ApiResponses.CreatedWithIdResponse anotherCatCreateResponse =
            await anotherCatCreateResponseMessage.GetIdResponseFromResponseMessageAsync();

        ReassignCatsToAdvertisement.ReassignCatsToAdvertisementRequest reassignCatsRequest = new([
            catCreateResponse.Id, anotherCatCreateResponse.Id
        ]);

        HttpResponseMessage reassignCatsResponseMessage =
            await _httpClient.PutAsJsonAsync($"api/v1/advertisements/{createAdvertisementResponse.Id}/cats",
                reassignCatsRequest);

        //Assert
        reassignCatsResponseMessage.StatusCode.Should().Be(HttpStatusCode.NoContent);
        HttpResponseMessage getAdvertisementResponse =
            await _httpClient.GetAsync($"api/v1/advertisements/{createAdvertisementResponse.Id}");
        getAdvertisementResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        AdvertisementResponse? advertisement =
            await getAdvertisementResponse.Content.ReadFromJsonAsync<AdvertisementResponse>();
        advertisement.Should().NotBeNull();
        advertisement!.Cats.Should().BeEquivalentTo(new AdvertisementResponse.CatDto[]
        {
            new()
            {
                Id = catCreateResponse.Id,
                Name = catCreateRequest.Name
            },
            new()
            {
                Id = anotherCatCreateResponse.Id,
                Name = anotherCatCreateRequest.Name
            }
        });
    }

    [Fact]
    public async Task ReassignCatsToAdvertisement_ShouldReturnSuccess_WhenOneLessCatIsProvided()
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

        CreateCat.CreateCatRequest anotherCatCreateRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage anotherCatCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats",
                anotherCatCreateRequest);
        ApiResponses.CreatedWithIdResponse anotherCatCreateResponse =
            await anotherCatCreateResponseMessage.GetIdResponseFromResponseMessageAsync();

        CreateAdvertisement.CreateAdvertisementRequest request =
            new Faker<CreateAdvertisement.CreateAdvertisementRequest>()
                .CustomInstantiator(faker =>
                    new CreateAdvertisement.CreateAdvertisementRequest(
                        PersonId: personRegisterResponse.Id,
                        CatsIdsToAssign: [catCreateResponse.Id],
                        Description: faker.Lorem.Lines(2),
                        PickupAddressCountry: faker.Address.Country(),
                        PickupAddressState: faker.Address.State(),
                        PickupAddressZipCode: faker.Address.ZipCode(),
                        PickupAddressCity: faker.Address.City(),
                        PickupAddressStreet: faker.Address.StreetName(),
                        PickupAddressBuildingNumber: faker.Address.BuildingNumber(),
                        ContactInfoEmail: faker.Person.Email,
                        ContactInfoPhoneNumber: faker.Person.Phone
                    ));

        HttpResponseMessage createAdvertisementResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/advertisements", request);
        ApiResponses.CreatedWithIdResponse createAdvertisementResponse =
            await createAdvertisementResponseMessage.GetIdResponseFromResponseMessageAsync();

        //Act
        ReassignCatsToAdvertisement.ReassignCatsToAdvertisementRequest reassignCatsRequest =
            new([anotherCatCreateResponse.Id]);

        HttpResponseMessage reassignCatsResponseMessage =
            await _httpClient.PutAsJsonAsync($"api/v1/advertisements/{createAdvertisementResponse.Id}/cats",
                reassignCatsRequest);

        //Assert
        reassignCatsResponseMessage.StatusCode.Should().Be(HttpStatusCode.NoContent);
        HttpResponseMessage getAdvertisementResponse =
            await _httpClient.GetAsync($"api/v1/advertisements/{createAdvertisementResponse.Id}");
        getAdvertisementResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        AdvertisementResponse? advertisement =
            await getAdvertisementResponse.Content.ReadFromJsonAsync<AdvertisementResponse>();
        advertisement.Should().NotBeNull();
        advertisement!.Cats.Should().BeEquivalentTo(new AdvertisementResponse.CatDto[]
        {
            new()
            {
                Id = anotherCatCreateResponse.Id,
                Name = anotherCatCreateRequest.Name
            }
        });
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
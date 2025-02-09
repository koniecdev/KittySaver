using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Shared.Endpoints;
using KittySaver.Api.Tests.Integration.Helpers;
using KittySaver.Domain.Common.Primitives.Enums;
using KittySaver.Shared.Hateoas;
using KittySaver.Shared.Responses;
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
    public async Task ReassignCatsToAdvertisement_ShouldReturnSuccess_WhenOneAdditionalCatIsProvided()
    {
        //Arrange
        CreatePersonRequest personRegisterRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", personRegisterRequest);
        ApiResponses.CreatedWithIdResponse personRegisterResponse =
            await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync();
        
        CreateCatRequest catCreateRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats", catCreateRequest);
        ApiResponses.CreatedWithIdResponse catCreateResponse =
            await catCreateResponseMessage.GetIdResponseFromResponseMessageAsync();

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

        HttpResponseMessage createAdvertisementResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/advertisements", request);
        ApiResponses.CreatedWithIdResponse createAdvertisementResponse =
            await createAdvertisementResponseMessage.GetIdResponseFromResponseMessageAsync();
        
        await using Stream imageStream = CreateTestImageHelper.Create();
        using MultipartFormDataContent content = new();
        StreamContent imageContent = new(imageStream);
        imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        content.Add(imageContent, "thumbnail", "test.jpg");
        await _httpClient.PutAsync(
            $"/api/v1/persons/{personRegisterResponse.Id}/advertisements/{createAdvertisementResponse.Id}/thumbnail", 
            content);

        //Act
        CreateCatRequest anotherCatCreateRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage anotherCatCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats",
                anotherCatCreateRequest);
        ApiResponses.CreatedWithIdResponse anotherCatCreateResponse =
            await anotherCatCreateResponseMessage.GetIdResponseFromResponseMessageAsync();

        ReassignCatsToAdvertisementRequest reassignCatsRequest = new([
            catCreateResponse.Id, anotherCatCreateResponse.Id
        ]);

        HttpResponseMessage reassignCatsResponseMessage =
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/advertisements/{createAdvertisementResponse.Id}/cats",
                reassignCatsRequest);

        //Assert
        reassignCatsResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        AdvertisementHateoasResponse? hateoasResponse = await reassignCatsResponseMessage.Content.ReadFromJsonAsync<AdvertisementHateoasResponse>();
        hateoasResponse.Should().NotBeNull();
        hateoasResponse!.Id.Should().Be(createAdvertisementResponse.Id);
        hateoasResponse.PersonId.Should().Be(personRegisterResponse.Id);
        hateoasResponse.Status.Should().Be(AdvertisementStatus.Active);
        hateoasResponse.Links.Select(x => x.Rel).Should()
            .BeEquivalentTo(EndpointRels.SelfRel,
                EndpointNames.GetAdvertisementThumbnail.Rel,
                EndpointNames.DeleteAdvertisement.Rel,
                EndpointNames.CloseAdvertisement.Rel,
                EndpointNames.UpdateAdvertisement.Rel,
                EndpointNames.UpdateAdvertisementThumbnail.Rel,
                EndpointNames.ReassignCatsToAdvertisement.Rel,
                EndpointNames.ExpireAdvertisement.Rel); //TODO: Investigate if ExpireAdvertisement is valid at this point
        hateoasResponse.Links.Select(x => x.Href).All(x => x.Contains("://")).Should().BeTrue();
        
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
        CreatePersonRequest personRegisterRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", personRegisterRequest);
        ApiResponses.CreatedWithIdResponse personRegisterResponse =
            await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync();
        CreateCatRequest catCreateRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats", catCreateRequest);
        ApiResponses.CreatedWithIdResponse catCreateResponse =
            await catCreateResponseMessage.GetIdResponseFromResponseMessageAsync();

        CreateCatRequest anotherCatCreateRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage anotherCatCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats",
                anotherCatCreateRequest);
        ApiResponses.CreatedWithIdResponse anotherCatCreateResponse =
            await anotherCatCreateResponseMessage.GetIdResponseFromResponseMessageAsync();

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

        HttpResponseMessage createAdvertisementResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/advertisements", request);
        ApiResponses.CreatedWithIdResponse createAdvertisementResponse =
            await createAdvertisementResponseMessage.GetIdResponseFromResponseMessageAsync();
        
        await using Stream imageStream = CreateTestImageHelper.Create();
        using MultipartFormDataContent content = new();
        StreamContent imageContent = new(imageStream);
        imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        content.Add(imageContent, "thumbnail", "test.jpg");
        await _httpClient.PutAsync(
            $"/api/v1/persons/{personRegisterResponse.Id}/advertisements/{createAdvertisementResponse.Id}/thumbnail", 
            content);

        //Act
        ReassignCatsToAdvertisementRequest reassignCatsRequest =
            new([anotherCatCreateResponse.Id]);

        HttpResponseMessage reassignCatsResponseMessage =
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/advertisements/{createAdvertisementResponse.Id}/cats",
                reassignCatsRequest);

        //Assert
        reassignCatsResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        AdvertisementHateoasResponse? hateoasResponse = await reassignCatsResponseMessage.Content.ReadFromJsonAsync<AdvertisementHateoasResponse>();
        hateoasResponse.Should().NotBeNull();
        hateoasResponse!.Id.Should().Be(createAdvertisementResponse.Id);
        hateoasResponse.PersonId.Should().Be(personRegisterResponse.Id);
        hateoasResponse.Status.Should().Be(AdvertisementStatus.Active);
        hateoasResponse.Links.Select(x => x.Rel).Should()
            .BeEquivalentTo(EndpointRels.SelfRel,
                EndpointNames.GetAdvertisementThumbnail.Rel,
                EndpointNames.DeleteAdvertisement.Rel,
                EndpointNames.CloseAdvertisement.Rel,
                EndpointNames.UpdateAdvertisement.Rel,
                EndpointNames.UpdateAdvertisementThumbnail.Rel,
                EndpointNames.ReassignCatsToAdvertisement.Rel,
                EndpointNames.ExpireAdvertisement.Rel);
        hateoasResponse.Links.Select(x => x.Href).All(x => x.Contains("://")).Should().BeTrue();
        
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
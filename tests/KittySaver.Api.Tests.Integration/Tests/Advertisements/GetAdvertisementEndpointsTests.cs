
using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Infrastructure.Endpoints;
using KittySaver.Api.Tests.Integration.Helpers;
using KittySaver.Shared.Common.Enums;
using KittySaver.Shared.Hateoas;
using KittySaver.Shared.Responses;
using KittySaver.Shared.TypedIds;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KittySaver.Api.Tests.Integration.Tests.Advertisements;

[Collection("Api")]
public class GetAdvertisementEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CleanupHelper _cleanup;

    public GetAdvertisementEndpointsTests(KittySaverApiFactory appFactory)
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
    public async Task GetAdvertisement_ShouldReturnExpectedState_WhenAdvertisementIsCreated()
    {
        //Arrange
        CreatePersonRequest createPersonRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage createPersonResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", createPersonRequest);
        IdResponse<PersonId> personId = await createPersonResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();
        
        CreateCatRequest createCatRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage createCatResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/cats", createCatRequest);
        IdResponse<CatId> catId = await createCatResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<CatId>>();
        
        CreateAdvertisementRequest createAdvertisementRequest =
            new Faker<CreateAdvertisementRequest>()
                .CustomInstantiator(faker =>
                    new CreateAdvertisementRequest(
                        CatsIdsToAssign: [catId],
                        Description: faker.Lorem.Lines(2),
                        PickupAddressCountry: faker.Address.CountryCode(),
                        PickupAddressState: faker.Address.State(),
                        PickupAddressZipCode: faker.Address.ZipCode(),
                        PickupAddressCity: faker.Address.City(),
                        PickupAddressStreet: faker.Address.StreetName(),
                        PickupAddressBuildingNumber: faker.Address.BuildingNumber(),
                        ContactInfoEmail: faker.Person.Email,
                        ContactInfoPhoneNumber: faker.Person.Phone
                    )).Generate();
        HttpResponseMessage createAdvertisementResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/advertisements", createAdvertisementRequest);
        IdResponse<AdvertisementId> advertisementId = await createAdvertisementResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<AdvertisementId>>();

        //Act
        HttpResponseMessage response =
            await _httpClient.GetAsync($"api/v1/persons/{personId}/advertisements/{advertisementId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        AdvertisementResponse advertisement =
            await response.GetResponseFromResponseMessageAsync<AdvertisementResponse>();
        advertisement.Id.Should().Be(advertisementId);
        advertisement.PersonId.Should().Be(personId);
        advertisement.PersonName.Should().Be(createPersonRequest.Nickname);
        advertisement.Description.Should().Be(createAdvertisementRequest.Description);
        advertisement.Title.Should().Be(createCatRequest.Name);
        advertisement.Status.Should().Be(AdvertisementStatus.ThumbnailNotUploaded);
        advertisement.ContactInfoEmail.Should().Be(createAdvertisementRequest.ContactInfoEmail);
        advertisement.ContactInfoPhoneNumber.Should().Be(createAdvertisementRequest.ContactInfoPhoneNumber);
        advertisement.PickupAddress.Country.Should().Be(createAdvertisementRequest.PickupAddressCountry);
        advertisement.PickupAddress.State.Should().Be(createAdvertisementRequest.PickupAddressState);
        advertisement.PickupAddress.ZipCode.Should().Be(createAdvertisementRequest.PickupAddressZipCode);
        advertisement.PickupAddress.City.Should().Be(createAdvertisementRequest.PickupAddressCity);
        advertisement.PickupAddress.Street.Should().Be(createAdvertisementRequest.PickupAddressStreet);
        advertisement.PickupAddress.BuildingNumber.Should().Be(createAdvertisementRequest.PickupAddressBuildingNumber);
        advertisement.PriorityScore.Should().BeGreaterThan(0);
        advertisement.Cats.Should().BeEquivalentTo([
            new AdvertisementResponse.CatDto
            {
                Id = catId,
                Name = createCatRequest.Name
            }
        ]);
        advertisement.Links.Select(x => x.Rel).Should().BeEquivalentTo(
            EndpointRels.SelfRel,
            EndpointNames.Advertisements.UpdateThumbnail.Rel,
            EndpointNames.Advertisements.Update.Rel,
            EndpointNames.Advertisements.Delete.Rel,
            EndpointNames.Advertisements.ReassignCats.Rel,
            EndpointNames.Advertisements.GetAdvertisementCats.Rel);
        advertisement.Links.All(x => !string.IsNullOrWhiteSpace(x.Href)).Should().BeTrue();
    }

    [Fact]
    public async Task GetAdvertisement_ShouldReturnExpectedState_WhenAdvertisementIsUpdated()
    {
        //Arrange
        CreatePersonRequest createPersonRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage createPersonResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", createPersonRequest);
        IdResponse<PersonId> personId = await createPersonResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();
        
        CreateCatRequest createCatRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage createCatResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/cats", createCatRequest);
        IdResponse<CatId> catId = await createCatResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<CatId>>();
        
        CreateAdvertisementRequest createAdvertisementRequest =
            new Faker<CreateAdvertisementRequest>()
                .CustomInstantiator(faker =>
                    new CreateAdvertisementRequest(
                        CatsIdsToAssign: [catId],
                        Description: faker.Lorem.Lines(2),
                        PickupAddressCountry: faker.Address.CountryCode(),
                        PickupAddressState: faker.Address.State(),
                        PickupAddressZipCode: faker.Address.ZipCode(),
                        PickupAddressCity: faker.Address.City(),
                        PickupAddressStreet: faker.Address.StreetName(),
                        PickupAddressBuildingNumber: faker.Address.BuildingNumber(),
                        ContactInfoEmail: faker.Person.Email,
                        ContactInfoPhoneNumber: faker.Person.Phone
                    )).Generate();
        HttpResponseMessage createAdvertisementResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/advertisements", createAdvertisementRequest);
        IdResponse<AdvertisementId> advertisementId = await createAdvertisementResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<AdvertisementId>>();

        UpdateAdvertisementRequest updateAdvertisementRequest =
            new Faker<UpdateAdvertisementRequest>()
                .CustomInstantiator(faker =>
                    new UpdateAdvertisementRequest(
                        Description: faker.Lorem.Lines(2),
                        PickupAddressCountry: faker.Address.CountryCode(),
                        PickupAddressState: faker.Address.State(),
                        PickupAddressZipCode: faker.Address.ZipCode(),
                        PickupAddressCity: faker.Address.City(),
                        PickupAddressStreet: faker.Address.StreetName(),
                        PickupAddressBuildingNumber: faker.Address.BuildingNumber(),
                        ContactInfoEmail: faker.Person.Email,
                        ContactInfoPhoneNumber: faker.Person.Phone
                    )).Generate();

        await _httpClient.PutAsJsonAsync($"api/v1/persons/{personId}/advertisements/{advertisementId}",
            updateAdvertisementRequest);

        //Act
        HttpResponseMessage response =
            await _httpClient.GetAsync($"api/v1/persons/{personId}/advertisements/{advertisementId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        AdvertisementResponse advertisement =
            await response.GetResponseFromResponseMessageAsync<AdvertisementResponse>();
        advertisement.Id.Should().Be(advertisementId);
        advertisement.PersonId.Should().Be(personId);
        advertisement.PersonName.Should().Be(createPersonRequest.Nickname);
        advertisement.Description.Should().Be(updateAdvertisementRequest.Description);
        advertisement.Title.Should().Be(createCatRequest.Name);
        advertisement.ContactInfoEmail.Should().Be(updateAdvertisementRequest.ContactInfoEmail);
        advertisement.ContactInfoPhoneNumber.Should().Be(updateAdvertisementRequest.ContactInfoPhoneNumber);
        advertisement.PickupAddress.Country.Should().Be(updateAdvertisementRequest.PickupAddressCountry);
        advertisement.PickupAddress.State.Should().Be(updateAdvertisementRequest.PickupAddressState);
        advertisement.PickupAddress.ZipCode.Should().Be(updateAdvertisementRequest.PickupAddressZipCode);
        advertisement.PickupAddress.City.Should().Be(updateAdvertisementRequest.PickupAddressCity);
        advertisement.PickupAddress.Street.Should().Be(updateAdvertisementRequest.PickupAddressStreet);
        advertisement.PickupAddress.BuildingNumber.Should().Be(updateAdvertisementRequest.PickupAddressBuildingNumber);
        advertisement.PriorityScore.Should().BeGreaterThan(0);
        advertisement.Status.Should().Be(AdvertisementStatus.ThumbnailNotUploaded);
        advertisement.Cats.Should().BeEquivalentTo([
            new AdvertisementResponse.CatDto
            {
                Id = catId,
                Name = createCatRequest.Name
            }
        ]);
        advertisement.Links.Select(x => x.Rel).Should().BeEquivalentTo(
            EndpointRels.SelfRel,
            EndpointNames.Advertisements.UpdateThumbnail.Rel,
            EndpointNames.Advertisements.Update.Rel,
            EndpointNames.Advertisements.Delete.Rel,
            EndpointNames.Advertisements.ReassignCats.Rel,
            EndpointNames.Advertisements.GetAdvertisementCats.Rel);
        advertisement.Links.All(x => !string.IsNullOrWhiteSpace(x.Href)).Should().BeTrue();
    }

    [Fact]
    public async Task GetAdvertisement_ShouldReturnNotFound_WhenRandomAdvertisementIdIsProvided()
    {
        //Act
        AdvertisementId randomId = AdvertisementId.New();
        HttpResponseMessage response = await _httpClient.GetAsync($"api/v1/advertisements/{randomId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails problemDetails = await response.GetResponseFromResponseMessageAsync<ProblemDetails>();
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetAdvertisement_ShouldReturnNotFound_WhenAdvertisementDoNotExist()
    {
        //Arrange
        CreatePersonRequest personRegisterRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", personRegisterRequest);
        IdResponse<PersonId> personId = await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();
        
        CreateCatRequest catCreateRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/cats", catCreateRequest);
        IdResponse<CatId> catId = await catCreateResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<CatId>>();

        CreateAdvertisementRequest request =
            new Faker<CreateAdvertisementRequest>()
                .CustomInstantiator(faker =>
                    new CreateAdvertisementRequest(
                        CatsIdsToAssign: [catId],
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
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/advertisements", request);
        IdResponse<AdvertisementId> advertisementId = await advertisementResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<AdvertisementId>>();
        await _httpClient.DeleteAsync($"api/v1/persons/{personId}/advertisements/{advertisementId}");

        //Act
        HttpResponseMessage response = await _httpClient.GetAsync($"api/v1/advertisements/{advertisementId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails problemDetails = await response.GetResponseFromResponseMessageAsync<ProblemDetails>();
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
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
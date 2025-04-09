using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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
using KittySaver.Tests.Shared;

namespace KittySaver.Api.Tests.Integration.Tests.Cats;

[Collection("Api")]
public class GetCatEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CleanupHelper _cleanup;

    public GetCatEndpointsTests(KittySaverApiFactory appFactory)
    {
        _httpClient = appFactory.CreateClient();
        _cleanup = new CleanupHelper(_httpClient);
    }

    private readonly CreatePersonRequest _createPersonRequest =
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
                )).Generate();

    private readonly CreateCatRequest _createCatRequest =
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
                )).Generate();

    [Fact]
    public async Task GetCat_ShouldReturnCat_WhenCatExist()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        IdResponse<PersonId> personId = await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();

        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/cats", _createCatRequest);
        IdResponse<CatId> catId = await catCreateResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<CatId>>();

        //Act
        HttpResponseMessage response =
            await _httpClient.GetAsync($"api/v1/persons/{personId}/cats/{catId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        CatResponse cat = await response.Content.ReadFromJsonAsync<CatResponse>() ?? throw new JsonException();
        cat.Id.Should().Be(catId);
        cat.PersonId.Should().Be(personId.Id);
        cat.AdvertisementId.Should().Be(null);
        cat.Name.Should().Be(_createCatRequest.Name);
        cat.AdditionalRequirements.Should().Be(_createCatRequest.AdditionalRequirements);
        cat.Behavior.Should().Be(_createCatRequest.Behavior);
        cat.AgeCategory.Should().Be(_createCatRequest.AgeCategory);
        cat.MedicalHelpUrgency.Should().Be(_createCatRequest.MedicalHelpUrgency);
        cat.HealthStatus.Should().Be(_createCatRequest.HealthStatus);
        cat.IsCastrated.Should().Be(_createCatRequest.IsCastrated);
        cat.PriorityScore.Should().BeGreaterThan(0);
        cat.IsAssignedToAdvertisement.Should().BeFalse();
        cat.Links.Select(x => x.Rel).Should()
            .BeEquivalentTo(EndpointRels.SelfRel,
                EndpointNames.UpdateCat.Rel,
                EndpointNames.DeleteCat.Rel,
                EndpointNames.UpdateCatThumbnail.Rel,
                EndpointNames.GetCatGallery.Rel,
                EndpointNames.AddPicturesToCatGallery.Rel,
                EndpointNames.RemovePictureFromCatGallery.Rel,
                EndpointNames.GetCatGalleryPicture.Rel);
        cat.Links.Select(x => x.Href).All(x => x.Contains("://")).Should().BeTrue();
    }

    [Fact]
    public async Task GetCat_ShouldReturnCatWithPositiveIsAssignedToAdvertisementFlag_WhenCatIsAssignedToAdvertisement()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        IdResponse<PersonId> personId = await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();

        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/cats", _createCatRequest);
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
        await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/advertisements", request);

        //Act
        HttpResponseMessage response =
            await _httpClient.GetAsync($"api/v1/persons/{personId}/cats/{catId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        CatResponse cat = await response.Content.ReadFromJsonAsync<CatResponse>() ?? throw new JsonException();
        cat.Id.Should().Be(catId);
        cat.IsAssignedToAdvertisement.Should().BeTrue();
        cat.Links.Select(x => x.Rel).Should()
            .BeEquivalentTo(EndpointRels.SelfRel,
                EndpointNames.UpdateCat.Rel,
                EndpointNames.UpdateCatThumbnail.Rel,
                EndpointNames.GetAdvertisement.Rel,
                EndpointNames.GetCatGallery.Rel,
                EndpointNames.AddPicturesToCatGallery.Rel,
                EndpointNames.RemovePictureFromCatGallery.Rel,
                EndpointNames.GetCatGalleryPicture.Rel);
        cat.Links.Select(x => x.Href).All(x => x.Contains("://")).Should().BeTrue();
    }

    [Fact]
    public async Task GetCat_ShouldReturnCatWithPositiveIsAssignedToAdvertisementFlag_WhenCatIsReassignedToAdvertisement()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        IdResponse<PersonId> personId = await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();

        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/cats", _createCatRequest);
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
        
        CreateCatRequest anotherCatCreateRequest =
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
                    )).Generate();
        HttpResponseMessage anotherCatCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/cats",
                anotherCatCreateRequest);
        IdResponse<CatId> anotherCatId = await anotherCatCreateResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<CatId>>();

        ReassignCatsToAdvertisementRequest reassignCatsRequest = new([
            anotherCatId
        ]);

        await _httpClient.PutAsJsonAsync($"api/v1/persons/{personId}/advertisements/{advertisementId}/cats", reassignCatsRequest);

        //Act
        HttpResponseMessage response =
            await _httpClient.GetAsync($"api/v1/persons/{personId}/cats/{catId}");
        HttpResponseMessage responseOfAnotherCat =
            await _httpClient.GetAsync(
                $"api/v1/persons/{personId}/cats/{anotherCatId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        CatResponse cat = await response.Content.ReadFromJsonAsync<CatResponse>() ?? throw new JsonException();
        cat.Id.Should().Be(catId);
        cat.IsAssignedToAdvertisement.Should().BeFalse();
        cat.Links.Select(x => x.Rel).Should()
            .BeEquivalentTo(EndpointRels.SelfRel,
                EndpointNames.UpdateCat.Rel,
                EndpointNames.DeleteCat.Rel,
                EndpointNames.UpdateCatThumbnail.Rel,
                EndpointNames.GetCatGallery.Rel,
                EndpointNames.AddPicturesToCatGallery.Rel,
                EndpointNames.RemovePictureFromCatGallery.Rel,
                EndpointNames.GetCatGalleryPicture.Rel);
        cat.Links.Select(x => x.Href).All(x => x.Contains("://")).Should().BeTrue();

        responseOfAnotherCat.StatusCode.Should().Be(HttpStatusCode.OK);
        CatResponse anotherCat = await responseOfAnotherCat.Content.ReadFromJsonAsync<CatResponse>() ??
                                 throw new JsonException();
        anotherCat.Id.Should().Be(anotherCatId);
        anotherCat.IsAssignedToAdvertisement.Should().BeTrue();
        cat.Links.Select(x => x.Rel).Should()
            .BeEquivalentTo(EndpointRels.SelfRel,
                EndpointNames.UpdateCat.Rel,
                EndpointNames.DeleteCat.Rel,
                EndpointNames.UpdateCatThumbnail.Rel,
                EndpointNames.AddPicturesToCatGallery.Rel,
                EndpointNames.RemovePictureFromCatGallery.Rel,
                EndpointNames.GetCatGallery.Rel,
                EndpointNames.GetCatGalleryPicture.Rel);
        cat.Links.Select(x => x.Href).All(x => x.Contains("://")).Should().BeTrue();
    }

    [Fact]
    public async Task GetPerson_ShouldReturnNotFound_WhenNonExistingCatIsProvided()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        IdResponse<PersonId> personId = await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();
        CatId randomCatId = CatId.New();
        
        //Act
        HttpResponseMessage response =
            await _httpClient.GetAsync($"api/v1/persons/{personId}/cats/{randomCatId}");
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails? problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetCat_ShouldReturnNotFound_WhenNonExistingPersonIsProvided()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        IdResponse<PersonId> personId = await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();
        
        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/cats", _createCatRequest);
        IdResponse<CatId> catId = await catCreateResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<CatId>>();
        PersonId randomPersonId = PersonId.New();

        //Act
        HttpResponseMessage response =
            await _httpClient.GetAsync($"api/v1/persons/{randomPersonId}/cats/{catId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails? problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
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
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

namespace KittySaver.Api.Tests.Integration.Tests.Advertisements;

[Collection("Api")]
public class GetPublicAdvertisementsEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CleanupHelper _cleanup;

    public GetPublicAdvertisementsEndpointsTests(KittySaverApiFactory appFactory)
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
        
        await using Stream imageStream = CreateTestImageHelper.Create();
        using MultipartFormDataContent content = new();
        using StreamContent imageContent = new(imageStream);
        imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        content.Add(imageContent, "thumbnail", "test.jpg");
        await _httpClient.PutAsync(
            $"/api/v1/persons/{personId}/advertisements/{advertisementId}/thumbnail", 
            content);

        CreatePersonRequest secondPersonRegisterRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage secondPersonRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", secondPersonRegisterRequest);
        IdResponse<PersonId> secondPersonId = await secondPersonRegisterResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();
        
        CreateCatRequest secondCatCreateRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage secondCatCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{secondPersonId}/cats",
                secondCatCreateRequest);
        IdResponse<CatId> secondCatId = await secondCatCreateResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<CatId>>();

        CreateAdvertisementRequest secondRequest =
            new Faker<CreateAdvertisementRequest>()
                .CustomInstantiator(faker =>
                    new CreateAdvertisementRequest(
                        CatsIdsToAssign: [secondCatId],
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
        HttpResponseMessage secondAdvertisementResponseMessage = 
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{secondPersonId}/advertisements", secondRequest);
        IdResponse<AdvertisementId> secondAdvertisementId = await secondAdvertisementResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<AdvertisementId>>();
        
        await using Stream secondImageStream = CreateTestImageHelper.Create();
        using MultipartFormDataContent secondContent = new();
        using StreamContent secondImageContent = new(imageStream);
        secondImageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        secondContent.Add(secondImageContent, "thumbnail", "test.jpg");
        await _httpClient.PutAsync(
            $"/api/v1/persons/{secondPersonId}/advertisements/{secondAdvertisementId}/thumbnail", 
            secondContent);
        
        //Act
        HttpResponseMessage response = await _httpClient.GetAsync("/api/v1/advertisements");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PagedList<AdvertisementResponse>? advertisements =
            await response.Content.ReadFromJsonAsync<PagedList<AdvertisementResponse>>();
        advertisements.Should().NotBeNull();
        advertisements!.Items.Count.Should().Be(2);
        advertisements.Total.Should().Be(2);
        advertisements.Links.Count.Should().Be(2);
        advertisements.Links.First(x => x.Rel == EndpointRels.SelfRel).Href.Should().Contain("://");
        advertisements.Links.First(x => x.Rel == "by-page").Href.Should().Contain("://");
        
        AdvertisementResponse firstPersonAdvertisement =
            advertisements.Items.First(x => x.PersonId == personId.Id);
        firstPersonAdvertisement.Cats.Count.Should().Be(1);
        firstPersonAdvertisement.Title.Should().Be(firstPersonAdvertisement.Cats.First().Name);
        firstPersonAdvertisement.PickupAddress.Country.Should().Be(request.PickupAddressCountry);
        firstPersonAdvertisement.PickupAddress.State.Should().Be(request.PickupAddressState);
        firstPersonAdvertisement.PickupAddress.ZipCode.Should().Be(request.PickupAddressZipCode);
        firstPersonAdvertisement.PickupAddress.City.Should().Be(request.PickupAddressCity);
        firstPersonAdvertisement.PickupAddress.Street.Should().Be(request.PickupAddressStreet);
        firstPersonAdvertisement.PickupAddress.BuildingNumber.Should().Be(request.PickupAddressBuildingNumber);
        firstPersonAdvertisement.PriorityScore.Should().BeGreaterThan(0);
        firstPersonAdvertisement.Links.Select(x => x.Rel).Should().BeEquivalentTo(
            EndpointRels.SelfRel,
            EndpointNames.Advertisements.GetThumbnail.Rel,
            EndpointNames.Advertisements.GetAdvertisementCats.Rel
        );
        firstPersonAdvertisement.Links.Select(x => x.Href).All(x => x.Contains("://")).Should().BeTrue();        

        AdvertisementResponse secondPersonAdvertisement =
            advertisements.Items.First(x => x.PersonId == secondPersonId);
        secondPersonAdvertisement.Cats.Count.Should().Be(1);
        secondPersonAdvertisement.Title.Should().Be(secondPersonAdvertisement.Cats.First().Name);
        secondPersonAdvertisement.PickupAddress.Country.Should().Be(secondRequest.PickupAddressCountry);
        secondPersonAdvertisement.PickupAddress.State.Should().Be(secondRequest.PickupAddressState);
        secondPersonAdvertisement.PickupAddress.ZipCode.Should().Be(secondRequest.PickupAddressZipCode);
        secondPersonAdvertisement.PickupAddress.City.Should().Be(secondRequest.PickupAddressCity);
        secondPersonAdvertisement.PickupAddress.Street.Should().Be(secondRequest.PickupAddressStreet);
        secondPersonAdvertisement.PickupAddress.BuildingNumber.Should().Be(secondRequest.PickupAddressBuildingNumber);
        secondPersonAdvertisement.PriorityScore.Should().BeGreaterThan(0);
        secondPersonAdvertisement.Links.Select(x => x.Rel).Should().BeEquivalentTo(
            EndpointRels.SelfRel,
            EndpointNames.Advertisements.GetThumbnail.Rel,
            EndpointNames.Advertisements.GetAdvertisementCats.Rel
        );
        secondPersonAdvertisement.Links.Select(x => x.Href).All(x => x.Contains("://")).Should().BeTrue();
    }

    [Fact]
    public async Task GetAdvertisements_ShouldReturnEmptyList_WhenNoAdvertisementsExist()
    {
        //Act
        HttpResponseMessage response = await _httpClient.GetAsync("/api/v1/advertisements");

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
        
        await using Stream imageStream = CreateTestImageHelper.Create();
        using MultipartFormDataContent content = new();
        using StreamContent imageContent = new(imageStream);
        imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        content.Add(imageContent, "thumbnail", "test.jpg");
        HttpResponseMessage firstPicUploadMessage = await _httpClient.PutAsync(
            $"api/v1/persons/{personId}/advertisements/{advertisementId}/thumbnail", 
            content);
        
        CreatePersonRequest secondPersonRegisterRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage secondPersonRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", secondPersonRegisterRequest);
        IdResponse<PersonId> secondPersonId = await secondPersonRegisterResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();
        
        CreateCatRequest secondCatCreateRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage secondCatCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{secondPersonId}/cats",
                secondCatCreateRequest);
        IdResponse<CatId> secondCatId = await secondCatCreateResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<CatId>>();

        CreateAdvertisementRequest secondRequest =
            new Faker<CreateAdvertisementRequest>()
                .CustomInstantiator(faker =>
                    new CreateAdvertisementRequest(
                        CatsIdsToAssign: [secondCatId],
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

        HttpResponseMessage secondAdvertisementResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{secondPersonId}/advertisements", secondRequest);
        IdResponse<AdvertisementId> secondAdvertisementId = await secondAdvertisementResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<AdvertisementId>>();
        
        await using Stream secondImageStream = CreateTestImageHelper.Create();
        using MultipartFormDataContent secondContent = new();
        using StreamContent secondImageContent = new(secondImageStream);
        secondImageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        secondContent.Add(secondImageContent, "thumbnail", "test.jpg");
        HttpResponseMessage secondPicUploadMessage = await _httpClient.PutAsync(
            $"api/v1/persons/{secondPersonId}/advertisements/{secondAdvertisementId}/thumbnail", 
            secondContent);
        
        //Act
        HttpResponseMessage response = await _httpClient.GetAsync($"/api/v1/advertisements?searchTerm=personid-eq-{personId}");

        //Assert
        firstPicUploadMessage.IsSuccessStatusCode.Should().BeTrue();
        secondPicUploadMessage.IsSuccessStatusCode.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PagedList<AdvertisementResponse>? advertisements =
            await response.Content.ReadFromJsonAsync<PagedList<AdvertisementResponse>>();
        advertisements?.Items.Count.Should().Be(1);
        advertisements?.Items.Count(x=>x.PersonId == personId.Id).Should().Be(1);
        advertisements?.Total.Should().Be(2);
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
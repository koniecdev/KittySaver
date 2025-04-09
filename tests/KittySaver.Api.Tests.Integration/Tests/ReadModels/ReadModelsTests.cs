using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Tests.Integration.Helpers;
using KittySaver.Shared.Common.Enums;
using KittySaver.Shared.Responses;
using KittySaver.Shared.TypedIds;

namespace KittySaver.Api.Tests.Integration.Tests.ReadModels;

[Collection("Api")]
public class ReadModelsIntegrationTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CleanupHelper _cleanup;

    public ReadModelsIntegrationTests(KittySaverApiFactory appFactory)
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
                    IsCastrated: faker.Random.Bool(),
                    MedicalHelpUrgency: MedicalHelpUrgency.NoNeed.Name,
                    Behavior: Behavior.Friendly.Name,
                    HealthStatus: HealthStatus.Good.Name,
                    AgeCategory: AgeCategory.Adult.Name,
                    AdditionalRequirements: faker.Lorem.Sentence()
                ));

    [Fact]
    public async Task PersonReadModel_ShouldMapAllProperties_WhenPersonIsCreated()
    {
        // Arrange
        CreatePersonRequest createRequest = _createPersonRequestGenerator.Generate();

        // Act
        HttpResponseMessage createResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/persons", createRequest);
        IdResponse<PersonId> personId = await createResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();

        // Assert
        HttpResponseMessage getResponse = await _httpClient.GetAsync($"api/v1/persons/{personId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        PersonResponse person = await getResponse.GetResponseFromResponseMessageAsync<PersonResponse>();
        
        // Verify all PersonReadModel properties are correctly mapped
        person.Id.Should().Be(personId.Id);
        person.Nickname.Should().Be(createRequest.Nickname);
        person.Email.Should().Be(createRequest.Email);
        person.PhoneNumber.Should().Be(createRequest.PhoneNumber);
        person.DefaultAdvertisementsContactInfoEmail.Should().Be(createRequest.DefaultAdvertisementContactInfoEmail);
        person.DefaultAdvertisementsContactInfoPhoneNumber.Should().Be(createRequest.DefaultAdvertisementContactInfoPhoneNumber);
        person.DefaultAdvertisementsPickupAddress.Should().NotBeNull();
        person.DefaultAdvertisementsPickupAddress.Country.Should().Be(createRequest.DefaultAdvertisementPickupAddressCountry);
        person.DefaultAdvertisementsPickupAddress.State.Should().Be(createRequest.DefaultAdvertisementPickupAddressState);
        person.DefaultAdvertisementsPickupAddress.ZipCode.Should().Be(createRequest.DefaultAdvertisementPickupAddressZipCode);
        person.DefaultAdvertisementsPickupAddress.City.Should().Be(createRequest.DefaultAdvertisementPickupAddressCity);
        person.DefaultAdvertisementsPickupAddress.Street.Should().Be(createRequest.DefaultAdvertisementPickupAddressStreet);
        person.DefaultAdvertisementsPickupAddress.BuildingNumber.Should().Be(createRequest.DefaultAdvertisementPickupAddressBuildingNumber);
    }

    [Fact]
    public async Task CatReadModel_ShouldMapAllProperties_WhenCatIsCreated()
    {
        // Arrange
        CreatePersonRequest createPersonRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage createPersonResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/persons", createPersonRequest);
        IdResponse<PersonId> personId = await createPersonResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();

        CreateCatRequest createCatRequest = _createCatRequestGenerator.Generate();

        // Act
        HttpResponseMessage createCatResponseMessage = await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/cats", createCatRequest);
        IdResponse<CatId> catId = await createCatResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<CatId>>();

        // Assert
        HttpResponseMessage getResponse = await _httpClient.GetAsync($"api/v1/persons/{personId}/cats/{catId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        CatResponse cat = await getResponse.GetResponseFromResponseMessageAsync<CatResponse>();
        
        // Verify all CatReadModel properties are correctly mapped
        cat.Id.Should().Be(catId.Id);
        cat.Name.Should().Be(createCatRequest.Name);
        cat.AdditionalRequirements.Should().Be(createCatRequest.AdditionalRequirements);
        cat.IsCastrated.Should().Be(createCatRequest.IsCastrated);
        cat.IsAdopted.Should().BeFalse();
        cat.IsAssignedToAdvertisement.Should().BeFalse();
        cat.MedicalHelpUrgency.Should().Be(createCatRequest.MedicalHelpUrgency);
        cat.AgeCategory.Should().Be(createCatRequest.AgeCategory);
        cat.Behavior.Should().Be(createCatRequest.Behavior);
        cat.HealthStatus.Should().Be(createCatRequest.HealthStatus);
        cat.PriorityScore.Should().BeGreaterThan(0);
    }

    [Fact] 
    public async Task AdvertisementReadModel_ShouldMapAllProperties_WhenAdvertisementIsCreated()
    {
        // Arrange
        CreatePersonRequest createPersonRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage createPersonResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/persons", createPersonRequest);
        IdResponse<PersonId> personId = await createPersonResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();

        CreateCatRequest createCatRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage createCatResponseMessage = await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/cats", createCatRequest);
        IdResponse<CatId> catId = await createCatResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<CatId>>();

        // Act
        CreateAdvertisementRequest createAdvertisementRequest = 
            new Faker<CreateAdvertisementRequest>()
                .CustomInstantiator(faker =>
                    new CreateAdvertisementRequest(
                        CatsIdsToAssign: [catId.Id],
                        Description: faker.Lorem.Paragraph(),
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
            await _httpClient.PostAsJsonAsync(
                $"api/v1/persons/{personId}/advertisements",
                createAdvertisementRequest);
        IdResponse<AdvertisementId> advertisementId = await createAdvertisementResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<AdvertisementId>>();

        // Assert
        HttpResponseMessage getResponse = await _httpClient.GetAsync(
            $"api/v1/persons/{personId}/advertisements/{advertisementId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        AdvertisementResponse advertisement = await getResponse.GetResponseFromResponseMessageAsync<AdvertisementResponse>();
        
        // Verify all AdvertisementReadModel properties are correctly mapped
        advertisement.Id.Should().Be(advertisementId.Id);
        advertisement.PersonId.Should().Be(personId.Id);
        advertisement.PersonName.Should().Be(createPersonRequest.Nickname);
        advertisement.Description.Should().Be(createAdvertisementRequest.Description);
        advertisement.ContactInfoEmail.Should().Be(createAdvertisementRequest.ContactInfoEmail);
        advertisement.ContactInfoPhoneNumber.Should().Be(createAdvertisementRequest.ContactInfoPhoneNumber);
        advertisement.Status.Should().Be(AdvertisementStatus.ThumbnailNotUploaded);
        advertisement.PriorityScore.Should().BeGreaterThan(0);
        
        advertisement.PickupAddress.Should().NotBeNull();
        advertisement.PickupAddress.Country.Should().Be(createAdvertisementRequest.PickupAddressCountry);
        advertisement.PickupAddress.State.Should().Be(createAdvertisementRequest.PickupAddressState);
        advertisement.PickupAddress.ZipCode.Should().Be(createAdvertisementRequest.PickupAddressZipCode); 
        advertisement.PickupAddress.City.Should().Be(createAdvertisementRequest.PickupAddressCity);
        advertisement.PickupAddress.Street.Should().Be(createAdvertisementRequest.PickupAddressStreet);
        advertisement.PickupAddress.BuildingNumber.Should().Be(createAdvertisementRequest.PickupAddressBuildingNumber);
        
        advertisement.Cats.Should().ContainSingle();
        AdvertisementResponse.CatDto cat = advertisement.Cats.Single();
        cat.Id.Should().Be(catId.Id);
        cat.Name.Should().Be(createCatRequest.Name);
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
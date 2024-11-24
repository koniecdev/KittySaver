using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Features.Advertisements;
using KittySaver.Api.Features.Persons;
using KittySaver.Api.Tests.Integration.Helpers;
using KittySaver.Domain.Common.Primitives.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace KittySaver.Api.Tests.Integration.Tests.Advertisements;

[Collection("Api")]
public class DeleteAdvertisementEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CleanupHelper _cleanup;
    public DeleteAdvertisementEndpointsTests(KittySaverApiFactory appFactory)
    {
        _httpClient = appFactory.CreateClient();
        _cleanup = new CleanupHelper(_httpClient);
    }

    private readonly Faker<CreatePerson.CreatePersonRequest> _createPersonRequestGenerator =
        new Faker<CreatePerson.CreatePersonRequest>()
            .CustomInstantiator(faker =>
                new CreatePerson.CreatePersonRequest(
                    FirstName: faker.Person.FirstName,
                    LastName: faker.Person.LastName,
                    Email: faker.Person.Email,
                    PhoneNumber: faker.Person.Phone,
                    UserIdentityId: Guid.NewGuid(),
                    AddressCountry: faker.Address.Country(),
                    AddressState: faker.Address.State(),
                    AddressZipCode: faker.Address.ZipCode(),
                    AddressCity: faker.Address.City(),
                    AddressStreet: faker.Address.StreetName(),
                    AddressBuildingNumber: faker.Address.BuildingNumber(),
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
    public async Task DeleteAdvertisement_ShouldReturnSuccess_WhenValidDataIsProvided()
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

        HttpResponseMessage advertisementResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/advertisements", request);
        ApiResponses.CreatedWithIdResponse advertisementResponse = await advertisementResponseMessage.GetIdResponseFromResponseMessageAsync();
        
        //Act
        HttpResponseMessage deleteResponseMessage = await _httpClient.DeleteAsync($"api/v1/advertisements/{advertisementResponse.Id}");
        
        //Assert
        deleteResponseMessage.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
    
    [Fact]
    public async Task DeleteAdvertisement_ShouldReturnSuccess_WhenValidDataIsProvidedWithMoreCats()
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
        CreateCat.CreateCatRequest secondCatCreateRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage secondCatCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats",
                secondCatCreateRequest);
        ApiResponses.CreatedWithIdResponse secondCatCreateResponse =
            await secondCatCreateResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
    
        //Act
        CreateAdvertisement.CreateAdvertisementRequest request =
            new Faker<CreateAdvertisement.CreateAdvertisementRequest>()
                .CustomInstantiator(faker =>
                    new CreateAdvertisement.CreateAdvertisementRequest(
                        PersonId: personRegisterResponse.Id,
                        CatsIdsToAssign: [catCreateResponse.Id, secondCatCreateResponse.Id],
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
    
        HttpResponseMessage responseMessage = await _httpClient.PostAsJsonAsync("api/v1/advertisements", request);
        ApiResponses.CreatedWithIdResponse advertisementResponse =
            await responseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        
        //Act
        HttpResponseMessage deleteResponseMessage = await _httpClient.DeleteAsync($"api/v1/advertisements/{advertisementResponse.Id}");
        
        //Assert
        deleteResponseMessage.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
    
    [Fact]
    public async Task DeleteAdvertisement_ShouldReturnNotFound_WhenInvaliIdIsProvided()
    {
        //Arrange
        Guid randomId = Guid.NewGuid();
        
        //Act
        HttpResponseMessage deleteResponse = await _httpClient.DeleteAsync($"api/v1/advertisements/{randomId}");
        
        //Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails? notFoundProblemDetails = await deleteResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        notFoundProblemDetails.Should().NotBeNull();
        notFoundProblemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
    }
    
    [Fact]
    public async Task DeleteAdvertisement_ShouldReturnBadRequest_WhenEmptyDataAreProvided()
    {
        //Arrange
        Guid randomId = Guid.Empty;
        
        //Act
        HttpResponseMessage deleteResponse = await _httpClient.DeleteAsync($"api/v1/advertisements/{randomId}");
        
        //Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await deleteResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(1);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo(nameof(DeleteAdvertisement.DeleteAdvertisementCommand.Id));
        validationProblemDetails.Errors.Values.Count.Should().Be(1);
        validationProblemDetails.Errors[nameof(DeleteAdvertisement.DeleteAdvertisementCommand.Id)][0]
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
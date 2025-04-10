using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Features.Persons;
using KittySaver.Api.Tests.Integration.Helpers;
using KittySaver.Shared.Common.Enums;
using KittySaver.Shared.Responses;
using KittySaver.Shared.TypedIds;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KittySaver.Api.Tests.Integration.Tests.Persons;

[Collection("Api")]
public class DeletePersonEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CleanupHelper _cleanup;

    public DeletePersonEndpointsTests(KittySaverApiFactory appFactory)
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
    public async Task DeletePerson_ShouldReturnSuccess_WhenValidDataIsProvided()
    {
        //Arrange
        CreatePersonRequest createRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", createRequest);
        IdResponse<PersonId> personId = await response.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();

        //Act
        HttpResponseMessage deleteResponse =
            await _httpClient.DeleteAsync($"api/v1/persons/{personId}");

        //Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        HttpResponseMessage issuedDeletedUserResponseMessage =
            await _httpClient.GetAsync($"api/v1/persons/{personId}");
        issuedDeletedUserResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails? notFoundProblemDetails =
            await issuedDeletedUserResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>();
        notFoundProblemDetails.Should().NotBeNull();
        notFoundProblemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task DeletePerson_ShouldReturnSuccess_WhenUserHaveAdvertisement()
    {
        //Arrange
        CreatePersonRequest createPersonRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage createPersonResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", createPersonRequest);
        IdResponse<PersonId> personId = await createPersonResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();
        
        CreateCatRequest catCreateRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/cats", catCreateRequest);
        IdResponse<CatId> catId = await catCreateResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<CatId>>();

        CreateAdvertisementRequest request =
            new Faker<CreateAdvertisementRequest>()
                .CustomInstantiator(faker =>
                    new CreateAdvertisementRequest(
                        CatsIdsToAssign: [catId.Id],
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

        //Act
        HttpResponseMessage deleteResponse = await _httpClient.DeleteAsync($"api/v1/persons/{personId}");

        //Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        HttpResponseMessage issuedDeletedUserResponseMessage =
            await _httpClient.GetAsync($"api/v1/persons/{personId}");
        issuedDeletedUserResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        HttpResponseMessage issuedDeletedUserAdvertisementResponseMessage =
            await _httpClient.GetAsync($"api/v1/advertisements/{advertisementId}");
        issuedDeletedUserAdvertisementResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletePerson_ShouldReturnNotFound_WhenNonRegisteredUserIdProvided()
    {
        //Arrange
        PersonId randomId = PersonId.New();
        //Act
        HttpResponseMessage deleteResponse = await _httpClient.DeleteAsync($"api/v1/persons/{randomId}");

        //Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails? notFoundProblemDetails = await deleteResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        notFoundProblemDetails.Should().NotBeNull();
        notFoundProblemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task DeletePerson_ShouldReturnBadRequest_WhenEmptyDataAreProvided()
    {
        //Arrange
        PersonId emptyId = default;
        //Act
        HttpResponseMessage deleteResponse = await _httpClient.DeleteAsync($"api/v1/persons/{emptyId}");

        //Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await deleteResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(1);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo([
            nameof(DeletePerson.DeletePersonCommand.Id)
        ]);
        validationProblemDetails.Errors.Values.Count.Should().Be(1);
        // validationProblemDetails.Errors[nameof(DeletePerson.DeletePersonCommand.Id)][0]
        //     .Should().Be("'Id' must not be empty.");
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
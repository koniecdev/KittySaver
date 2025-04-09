using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Features.Cats;
using KittySaver.Api.Tests.Integration.Helpers;
using KittySaver.Shared.Common.Enums;
using KittySaver.Shared.Responses;
using KittySaver.Shared.TypedIds;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KittySaver.Api.Tests.Integration.Tests.Cats;

[Collection("Api")]
public class DeleteCatEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CleanupHelper _cleanup;

    public DeleteCatEndpointsTests(KittySaverApiFactory appFactory)
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
    public async Task DeleteCat_ShouldReturnSuccess_WhenValidDataIsProvided()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        IdResponse<PersonId> personId = await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();
        
        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/cats", _createCatRequest);
        IdResponse<CatId> catId = await catCreateResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<CatId>>();

        //Act
        HttpResponseMessage deleteResponse =
            await _httpClient.DeleteAsync($"api/v1/persons/{personId}/cats/{catId}");

        //Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        HttpResponseMessage catNotFoundProblemDetailsMessage =
            await _httpClient.GetAsync($"api/v1/persons/{personId}/cats/{catId}");
        catNotFoundProblemDetailsMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails? notFoundProblemDetails =
            await catNotFoundProblemDetailsMessage.Content.ReadFromJsonAsync<ProblemDetails>();
        notFoundProblemDetails.Should().NotBeNull();
        notFoundProblemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
        HttpResponseMessage userResponseMessage =
            await _httpClient.GetAsync($"api/v1/persons/{personId}");
        userResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteCat_ShouldReturnNotFound_WhenNonExistingCatIdProvided()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        IdResponse<PersonId> personId = await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();
        CatId randomCatId = CatId.New();

        //Act
        HttpResponseMessage deleteResponse =
            await _httpClient.DeleteAsync($"api/v1/persons/{personId}/cats/{randomCatId}");

        //Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails? notFoundProblemDetails = await deleteResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        notFoundProblemDetails.Should().NotBeNull();
        notFoundProblemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task DeleteCat_ShouldReturnNotFound_WhenNonRegisteredUserIdProvided()
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
        HttpResponseMessage deleteResponse =
            await _httpClient.DeleteAsync($"api/v1/persons/{randomPersonId}/cats/{catId}");

        //Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails? notFoundProblemDetails = await deleteResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        notFoundProblemDetails.Should().NotBeNull();
        notFoundProblemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task DeleteCat_ShouldReturnBadRequest_WhenEmptyDataAreProvided()
    {
        //Arrange
        PersonId emptyPersonId = default;
        CatId emptyCatId = default;

        //Act
        HttpResponseMessage deleteResponse =
            await _httpClient.DeleteAsync($"api/v1/persons/{emptyPersonId}/cats/{emptyCatId}");

        //Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await deleteResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(2);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo(
            nameof(DeleteCat.DeleteCatCommand.PersonId),
            nameof(DeleteCat.DeleteCatCommand.Id)
        );
        validationProblemDetails.Errors.Values.Count.Should().Be(2);
        validationProblemDetails.Errors[nameof(DeleteCat.DeleteCatCommand.PersonId)][0]
            .Should().Be("'Person Id' must not be empty.");
        validationProblemDetails.Errors[nameof(DeleteCat.DeleteCatCommand.Id)][0]
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
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using Bogus.Extensions;
using FluentAssertions;
using KittySaver.Api.Infrastructure.Endpoints;
using KittySaver.Api.Tests.Integration.Helpers;
using KittySaver.Domain.Persons.ValueObjects;
using KittySaver.Domain.ValueObjects;
using KittySaver.Shared.Common.Enums;
using KittySaver.Shared.Hateoas;
using KittySaver.Shared.Responses;
using KittySaver.Shared.TypedIds;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KittySaver.Api.Tests.Integration.Tests.Cats;

[Collection("Api")]
public class CreateCatEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CleanupHelper _cleanup;

    public CreateCatEndpointsTests(KittySaverApiFactory appFactory)
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
    public async Task CreateCat_ShouldReturnSuccess_WhenValidDataIsProvided()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        IdResponse<PersonId> personId = await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();

        //Act
        CreateCatRequest request = _createCatRequestGenerator.Generate();
        HttpResponseMessage response =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/cats", request);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        CatHateoasResponse? hateoasResponse = await response.Content.ReadFromJsonAsync<CatHateoasResponse>();
        hateoasResponse.Should().NotBeNull();
        hateoasResponse!.Links.Select(x => x.Rel).Should()
            .BeEquivalentTo(EndpointRels.SelfRel,
                EndpointNames.Cats.Update.Rel,
                EndpointNames.Cats.Delete.Rel,
                EndpointNames.Cats.UpdateThumbnail.Rel,
                EndpointNames.Cats.AddPictures.Rel,
                EndpointNames.Cats.RemovePicture.Rel,
                EndpointNames.Cats.GetGallery.Rel,
                EndpointNames.Cats.GetGalleryPicture.Rel);
        hateoasResponse.Links.Select(x => x.Href).All(x => x.Contains("://")).Should().BeTrue();
        response.Headers.Location!.ToString().Should()
            .Contain($"/api/v1/persons/{hateoasResponse.PersonId}/cats/{hateoasResponse.Id}");
    }

    [Fact]
    public async Task CreateCat_ShouldReturnSuccess_WhenValidDataIsProvidedWithoutAdditionalParameters()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        IdResponse<PersonId> personId = await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();

        //Act
        CreateCatRequest request = new(
            Name: "Whiskers",
            IsCastrated: true,
            MedicalHelpUrgency: MedicalHelpUrgency.NoNeed.Name,
            Behavior: Behavior.Friendly.Name,
            HealthStatus: HealthStatus.Good.Name,
            AgeCategory: AgeCategory.Adult.Name
        );
        HttpResponseMessage response =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/cats", request);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        CatHateoasResponse? hateoasResponse = await response.Content.ReadFromJsonAsync<CatHateoasResponse>();
        hateoasResponse.Should().NotBeNull();
        hateoasResponse!.Links.Select(x => x.Rel).Should()
            .BeEquivalentTo(EndpointRels.SelfRel,
                EndpointNames.Cats.Update.Rel,
                EndpointNames.Cats.Delete.Rel,
                EndpointNames.Cats.UpdateThumbnail.Rel,
                EndpointNames.Cats.GetGallery.Rel,
                EndpointNames.Cats.AddPictures.Rel,
                EndpointNames.Cats.RemovePicture.Rel,
                EndpointNames.Cats.GetGalleryPicture.Rel);
        hateoasResponse.Links.Select(x => x.Href).All(x => x.Contains("://")).Should().BeTrue();
        response.Headers.Location!.ToString().Should()
            .Contain($"/api/v1/persons/{hateoasResponse.PersonId}/cats/{hateoasResponse.Id}");
    }

    [Fact]
    public async Task CreateCat_ShouldReturnSuccess_WhenEmptyAdditionalRequirementsAreProvided()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        IdResponse<PersonId> personId = await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();

        //Act
        CreateCatRequest request = new(
            Name: "Whiskers",
            IsCastrated: true,
            MedicalHelpUrgency: MedicalHelpUrgency.NoNeed.Name,
            Behavior: Behavior.Friendly.Name,
            HealthStatus: HealthStatus.Good.Name,
            AgeCategory: AgeCategory.Adult.Name,
            AdditionalRequirements: " "
        );
        HttpResponseMessage response =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/cats", request);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        CatHateoasResponse? hateoasResponse = await response.Content.ReadFromJsonAsync<CatHateoasResponse>();
        hateoasResponse.Should().NotBeNull();
        hateoasResponse!.Links.Select(x => x.Rel).Should()
            .BeEquivalentTo(EndpointRels.SelfRel,
                EndpointNames.Cats.Update.Rel,
                EndpointNames.Cats.Delete.Rel,
                EndpointNames.Cats.UpdateThumbnail.Rel,
                EndpointNames.Cats.GetGallery.Rel,
                EndpointNames.Cats.AddPictures.Rel,
                EndpointNames.Cats.RemovePicture.Rel,
                EndpointNames.Cats.GetGalleryPicture.Rel);
        hateoasResponse.Links.Select(x => x.Href).All(x => x.Contains("://")).Should().BeTrue();
        response.Headers.Location!.ToString().Should()
            .Contain($"/api/v1/persons/{hateoasResponse.PersonId}/cats/{hateoasResponse.Id}");
        
        CatResponse catAfterUpdate =
            await _httpClient.GetFromJsonAsync<CatResponse>(
                $"api/v1/persons/{personId}/cats/{hateoasResponse.Id}")
            ?? throw new JsonException();
        catAfterUpdate.AdditionalRequirements.Should().Be(string.Empty);
    }

    [Fact]
    public async Task CreateCat_ShouldReturnBadRequest_WhenTooLongDataAreProvided()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        IdResponse<PersonId> personId = await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();
        CreateCatRequest request = new Faker<CreateCatRequest>()
            .CustomInstantiator(faker =>
                new CreateCatRequest(
                    Name: faker.Person.FirstName.ClampLength(CatName.MaxLength + 1),
                    IsCastrated: true,
                    MedicalHelpUrgency: MedicalHelpUrgency.ShouldSeeVet.Name,
                    Behavior: Behavior.Unfriendly.Name,
                    HealthStatus: HealthStatus.ChronicMinor.Name,
                    AgeCategory: AgeCategory.Baby.Name,
                    AdditionalRequirements: faker.Address.State().ClampLength(Description.MaxLength + 1)
                )).Generate();

        //Act
        HttpResponseMessage response =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/cats", request);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(2);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo(
            nameof(CreateCatRequest.Name),
            nameof(CreateCatRequest.AdditionalRequirements)
        );
        validationProblemDetails.Errors.Values.Count.Should().Be(2);

        // validationProblemDetails.Errors[nameof(CreateCatRequest.Name)][0]
        //     .Should()
        //     .StartWith($"The length of 'Name' must be {CatName.MaxLength} characters or fewer. You entered");
        //
        // validationProblemDetails.Errors[nameof(CreateCatRequest.AdditionalRequirements)][0]
        //     .Should()
        //     .StartWith(
        //         $"The length of 'Additional Requirements' must be {Description.MaxLength} characters or fewer. You entered");
    }

    [Fact]
    public async Task CreateCat_ShouldReturnBadRequest_WhenEmptyDataAreProvided()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        IdResponse<PersonId> personId = await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();
        CreateCatRequest request = new(
            Name: "",
            IsCastrated: false,
            MedicalHelpUrgency: "",
            Behavior: "",
            HealthStatus: "",
            AgeCategory: ""
        );

        //Act
        HttpResponseMessage response =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/cats", request);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);

        validationProblemDetails.Errors.Count.Should().Be(5);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo(
            nameof(CreateCatRequest.Name),
            nameof(CreateCatRequest.MedicalHelpUrgency),
            nameof(CreateCatRequest.Behavior),
            nameof(CreateCatRequest.AgeCategory),
            nameof(CreateCatRequest.HealthStatus)
        );
        validationProblemDetails.Errors.Values.Count.Should().Be(5);

        // validationProblemDetails.Errors[nameof(CreateCatRequest.MedicalHelpUrgency)][0]
        //     .Should()
        //     .Be("'Medical Help Urgency' must not be empty.");
        //
        // validationProblemDetails.Errors[nameof(CreateCatRequest.Behavior)][0]
        //     .Should()
        //     .Be("'Behavior' must not be empty.");
        //
        // validationProblemDetails.Errors[nameof(CreateCatRequest.AgeCategory)][0]
        //     .Should()
        //     .Be("'Age Category' must not be empty.");
        //
        // validationProblemDetails.Errors[nameof(CreateCatRequest.HealthStatus)][0]
        //     .Should()
        //     .Be("'Health Status' must not be empty.");
        //
        // validationProblemDetails.Errors[nameof(CreateCatRequest.Name)][0]
        //     .Should()
        //     .Be("'Name' must not be empty.");
    }

    [Fact]
    public async Task CreateCat_ShouldReturnNotFound_WhenNotExistingPersonIsProvided()
    {
        //Arrange
        PersonId randomId = PersonId.New();
        CreateCatRequest request = _createCatRequestGenerator.Generate();

        //Act
        HttpResponseMessage response =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{randomId}/cats", request);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails? notFoundProblemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        notFoundProblemDetails.Should().NotBeNull();
        notFoundProblemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
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
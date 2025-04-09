using System.Net;
using System.Net.Http.Json;
using Bogus;
using Bogus.Extensions;
using FluentAssertions;
using KittySaver.Api.Infrastructure.Endpoints;
using KittySaver.Api.Tests.Integration.Helpers;
using KittySaver.Domain.Persons;
using KittySaver.Domain.Persons.ValueObjects;
using KittySaver.Domain.ValueObjects;
using KittySaver.Shared.Common.Enums;
using KittySaver.Shared.Hateoas;
using KittySaver.Shared.Responses;
using KittySaver.Shared.TypedIds;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using KittySaver.Tests.Shared;
using JsonException = System.Text.Json.JsonException;

namespace KittySaver.Api.Tests.Integration.Tests.Cats;

[Collection("Api")]
public class UpdateCatEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CleanupHelper _cleanup;

    public UpdateCatEndpointsTests(KittySaverApiFactory appFactory)
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
                    AgeCategory: AgeCategory.Baby.Name,
                    AdditionalRequirements: "Lorem ipsum"
                )).Generate();

    [Fact]
    public async Task UpdateCat_ShouldReturnSuccess_WhenValidDataIsProvided()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        IdResponse<PersonId> personId = await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();

        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/cats", _createCatRequest);
        IdResponse<CatId> catId = await catCreateResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<CatId>>();

        CatResponse catBeforeUpdate =
            await _httpClient.GetFromJsonAsync<CatResponse>(
                $"api/v1/persons/{personId}/cats/{catId}")
            ?? throw new JsonException();

        //Act
        UpdateCatRequest request = new Faker<UpdateCatRequest>()
            .CustomInstantiator(faker =>
                new UpdateCatRequest(
                    Name: faker.Name.FirstName(),
                    IsCastrated: false,
                    MedicalHelpUrgency: MedicalHelpUrgency.ShouldSeeVet.Name,
                    Behavior: Behavior.Unfriendly.Name,
                    HealthStatus: HealthStatus.Terminal.Name,
                    AgeCategory: AgeCategory.Senior.Name,
                    AdditionalRequirements: "Lorem ipsum dolor sit"
                )).Generate();

        HttpResponseMessage updateResponse =
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{personId}/cats/{catId}",
                request);

        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        CatHateoasResponse? hateoasResponse = await updateResponse.Content.ReadFromJsonAsync<CatHateoasResponse>();
        hateoasResponse.Should().NotBeNull();
        hateoasResponse!.Id.Should().Be(catId);
        hateoasResponse.Links.Select(x => x.Rel).Should()
            .BeEquivalentTo(EndpointRels.SelfRel,
                EndpointNames.UpdateCat.Rel,
                EndpointNames.DeleteCat.Rel,
                EndpointNames.UpdateCatThumbnail.Rel,
                EndpointNames.GetCatGallery.Rel,
                EndpointNames.AddPicturesToCatGallery.Rel,
                EndpointNames.RemovePictureFromCatGallery.Rel,
                EndpointNames.GetCatGalleryPicture.Rel);
        hateoasResponse.Links.Select(x => x.Href).All(x => x.Contains("://")).Should().BeTrue();
        CatResponse catAfterUpdate =
            await _httpClient.GetFromJsonAsync<CatResponse>(
                $"api/v1/persons/{personId}/cats/{catId}")
            ?? throw new JsonException();
        catBeforeUpdate.Should().NotBeEquivalentTo(catAfterUpdate);
        catAfterUpdate.Name.Should().Be(request.Name);
        catAfterUpdate.AdditionalRequirements.Should().Be(request.AdditionalRequirements);
        catAfterUpdate.Behavior.Should().Be(request.Behavior);
        catAfterUpdate.HealthStatus.Should().Be(request.HealthStatus);
        catAfterUpdate.AgeCategory.Should().Be(request.AgeCategory);
        catAfterUpdate.MedicalHelpUrgency.Should().Be(request.MedicalHelpUrgency);
        catAfterUpdate.IsCastrated.Should().Be(request.IsCastrated);
    }

    [Fact]
    public async Task UpdateCat_ShouldReturnSuccess_WhenCatWithAssignedAdvertisementIsProvided()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        IdResponse<PersonId> personId = await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();

        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/cats", _createCatRequest);
        IdResponse<CatId> catId = await catCreateResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<CatId>>();

        CreateAdvertisementRequest createAdvertisementRequest =
            new(
                CatsIdsToAssign: [catId],
                Description: _createCatRequest.AdditionalRequirements,
                PickupAddressCountry: _createPersonRequest.DefaultAdvertisementPickupAddressCountry,
                PickupAddressState: _createPersonRequest.DefaultAdvertisementPickupAddressState,
                PickupAddressZipCode: _createPersonRequest.DefaultAdvertisementPickupAddressZipCode,
                PickupAddressCity: _createPersonRequest.DefaultAdvertisementPickupAddressCity,
                PickupAddressStreet: _createPersonRequest.DefaultAdvertisementPickupAddressStreet,
                PickupAddressBuildingNumber: _createPersonRequest.DefaultAdvertisementPickupAddressBuildingNumber,
                ContactInfoEmail: _createPersonRequest.DefaultAdvertisementContactInfoEmail,
                ContactInfoPhoneNumber: _createPersonRequest.DefaultAdvertisementContactInfoPhoneNumber);
        HttpResponseMessage createAdvertisementResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/advertisements", createAdvertisementRequest);
        IdResponse<AdvertisementId> advertisementId = await createAdvertisementResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<AdvertisementId>>();

        CatResponse catBeforeUpdate =
            await _httpClient.GetFromJsonAsync<CatResponse>(
                $"api/v1/persons/{personId}/cats/{catId}")
            ?? throw new JsonException();

        //Act
        UpdateCatRequest request = new Faker<UpdateCatRequest>()
            .CustomInstantiator(faker =>
                new UpdateCatRequest(
                    Name: faker.Name.FirstName(),
                    IsCastrated: false,
                    MedicalHelpUrgency: MedicalHelpUrgency.ShouldSeeVet.Name,
                    Behavior: Behavior.Unfriendly.Name,
                    HealthStatus: HealthStatus.Terminal.Name,
                    AgeCategory: AgeCategory.Senior.Name,
                    AdditionalRequirements: "Lorem ipsum dolor sit"
                )).Generate();
        HttpResponseMessage updateResponse =
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{personId}/cats/{catId}",
                request);

        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        CatHateoasResponse? hateoasResponse = await updateResponse.Content.ReadFromJsonAsync<CatHateoasResponse>();
        hateoasResponse.Should().NotBeNull();
        hateoasResponse!.Id.Should().Be(catId);
        hateoasResponse.Links.Select(x => x.Rel).Should()
            .BeEquivalentTo(EndpointRels.SelfRel,
                EndpointNames.UpdateCat.Rel,
                EndpointNames.UpdateCatThumbnail.Rel,
                EndpointNames.GetAdvertisement.Rel,
                EndpointNames.GetCatGallery.Rel,
                EndpointNames.AddPicturesToCatGallery.Rel,
                EndpointNames.RemovePictureFromCatGallery.Rel,
                EndpointNames.GetCatGalleryPicture.Rel);
        hateoasResponse.Links.Select(x => x.Href).All(x => x.Contains("://")).Should().BeTrue();
        CatResponse catAfterUpdate =
            await _httpClient.GetFromJsonAsync<CatResponse>(
                $"api/v1/persons/{personId}/cats/{catId}")
            ?? throw new JsonException();
        catBeforeUpdate.Should().NotBeEquivalentTo(catAfterUpdate);
        catAfterUpdate.Name.Should().Be(request.Name);
        catAfterUpdate.AdditionalRequirements.Should().Be(request.AdditionalRequirements);
        catAfterUpdate.Behavior.Should().Be(request.Behavior);
        catAfterUpdate.HealthStatus.Should().Be(request.HealthStatus);
        catAfterUpdate.AgeCategory.Should().Be(request.AgeCategory);
        catAfterUpdate.MedicalHelpUrgency.Should().Be(request.MedicalHelpUrgency);
        catAfterUpdate.IsCastrated.Should().Be(request.IsCastrated);
        AdvertisementResponse advertisementAfterUpdate =
            await _httpClient.GetFromJsonAsync<AdvertisementResponse>(
                $"api/v1/persons/{personId}/advertisements/{advertisementId}")
            ?? throw new JsonException();
        advertisementAfterUpdate.PriorityScore.Should().Be(catAfterUpdate.PriorityScore);
    }

    [Fact]
    public async Task UpdateCat_ShouldReturnSuccess_WhenValidDataIsProvidedWithoutAdditionalParameters()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        IdResponse<PersonId> personId = await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();
        
        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/cats", _createCatRequest);
        IdResponse<CatId> catId = await catCreateResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<CatId>>();

        CatResponse catBeforeUpdate =
            await _httpClient.GetFromJsonAsync<CatResponse>(
                $"api/v1/persons/{personId}/cats/{catId}")
            ?? throw new JsonException();

        //Act
        UpdateCatRequest request = new Faker<UpdateCatRequest>()
            .CustomInstantiator(faker =>
                new UpdateCatRequest(
                    Name: faker.Name.FirstName(),
                    IsCastrated: false,
                    MedicalHelpUrgency: MedicalHelpUrgency.ShouldSeeVet.Name,
                    Behavior: Behavior.Unfriendly.Name,
                    HealthStatus: HealthStatus.Terminal.Name,
                    AgeCategory: AgeCategory.Senior.Name,
                    AdditionalRequirements: null
                )).Generate();

        HttpResponseMessage updateResponse =
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{personId}/cats/{catId}",
                request);

        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        CatHateoasResponse? hateoasResponse = await updateResponse.Content.ReadFromJsonAsync<CatHateoasResponse>();
        hateoasResponse.Should().NotBeNull();
        hateoasResponse!.Id.Should().Be(catId);
        hateoasResponse.Links.Select(x => x.Rel).Should()
            .BeEquivalentTo(EndpointRels.SelfRel,
                EndpointNames.UpdateCat.Rel,
                EndpointNames.DeleteCat.Rel,
                EndpointNames.UpdateCatThumbnail.Rel,
                EndpointNames.GetCatGallery.Rel,
                EndpointNames.AddPicturesToCatGallery.Rel,
                EndpointNames.RemovePictureFromCatGallery.Rel,
                EndpointNames.GetCatGalleryPicture.Rel);
        hateoasResponse.Links.Select(x => x.Href).All(x => x.Contains("://")).Should().BeTrue();
        
        CatResponse catAfterUpdate =
            await _httpClient.GetFromJsonAsync<CatResponse>(
                $"api/v1/persons/{personId}/cats/{catId}")
            ?? throw new JsonException();
        
        catBeforeUpdate.Should().NotBeEquivalentTo(catAfterUpdate);
        catAfterUpdate.AdditionalRequirements.Should().Be(string.Empty);
        catAfterUpdate.Name.Should().Be(request.Name);
        catAfterUpdate.Behavior.Should().Be(request.Behavior);
        catAfterUpdate.HealthStatus.Should().Be(request.HealthStatus);
        catAfterUpdate.AgeCategory.Should().Be(request.AgeCategory);
        catAfterUpdate.MedicalHelpUrgency.Should().Be(request.MedicalHelpUrgency);
        catAfterUpdate.IsCastrated.Should().Be(request.IsCastrated);
    }

    [Fact]
    public async Task UpdateCat_ShouldReturnSuccess_WhenEmptyAdditionalRequirementsAreProvided()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        IdResponse<PersonId> personId = await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();
        
        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/cats", _createCatRequest);
        IdResponse<CatId> catId = await catCreateResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<CatId>>();

        CatResponse catBeforeUpdate =
            await _httpClient.GetFromJsonAsync<CatResponse>(
                $"api/v1/persons/{personId}/cats/{catId}")
            ?? throw new JsonException();

        //Act
        UpdateCatRequest request = new Faker<UpdateCatRequest>()
            .CustomInstantiator(faker =>
                new UpdateCatRequest(
                    Name: faker.Name.FirstName(),
                    IsCastrated: false,
                    MedicalHelpUrgency: MedicalHelpUrgency.ShouldSeeVet.Name,
                    Behavior: Behavior.Unfriendly.Name,
                    HealthStatus: HealthStatus.Terminal.Name,
                    AgeCategory: AgeCategory.Senior.Name,
                    AdditionalRequirements: " "
                )).Generate();

        HttpResponseMessage updateResponse =
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{personId}/cats/{catId}",
                request);

        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        CatHateoasResponse? hateoasResponse = await updateResponse.Content.ReadFromJsonAsync<CatHateoasResponse>();
        hateoasResponse.Should().NotBeNull();
        hateoasResponse!.Id.Should().Be(catId);
        hateoasResponse.Links.Select(x => x.Rel).Should()
            .BeEquivalentTo(EndpointRels.SelfRel,
                EndpointNames.UpdateCat.Rel,
                EndpointNames.DeleteCat.Rel,
                EndpointNames.UpdateCatThumbnail.Rel,
                EndpointNames.GetCatGallery.Rel,
                EndpointNames.AddPicturesToCatGallery.Rel,
                EndpointNames.RemovePictureFromCatGallery.Rel,
                EndpointNames.GetCatGalleryPicture.Rel);
        hateoasResponse.Links.Select(x => x.Href).All(x => x.Contains("://")).Should().BeTrue();
        
        CatResponse catAfterUpdate =
            await _httpClient.GetFromJsonAsync<CatResponse>(
                $"api/v1/persons/{personId}/cats/{catId}")
            ?? throw new JsonException();
        catBeforeUpdate.Should().NotBeEquivalentTo(catAfterUpdate);
        catAfterUpdate.AdditionalRequirements.Should().Be(string.Empty);
        catAfterUpdate.Name.Should().Be(request.Name);
        catAfterUpdate.Behavior.Should().Be(request.Behavior);
        catAfterUpdate.HealthStatus.Should().Be(request.HealthStatus);
        catAfterUpdate.AgeCategory.Should().Be(request.AgeCategory);
        catAfterUpdate.MedicalHelpUrgency.Should().Be(request.MedicalHelpUrgency);
        catAfterUpdate.IsCastrated.Should().Be(request.IsCastrated);
    }

    [Fact]
    public async Task UpdateCat_ShouldReturnNotFound_WhenNonExistingCatIsProvided()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        IdResponse<PersonId> personId = await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();
        CatId randomCatId = CatId.New();

        //Act
        UpdateCatRequest request = new Faker<UpdateCatRequest>()
            .CustomInstantiator(faker =>
                new UpdateCatRequest(
                    Name: faker.Name.FirstName(),
                    IsCastrated: false,
                    MedicalHelpUrgency: MedicalHelpUrgency.ShouldSeeVet.Name,
                    Behavior: Behavior.Unfriendly.Name,
                    HealthStatus: HealthStatus.Terminal.Name,
                    AgeCategory: AgeCategory.Senior.Name,
                    AdditionalRequirements: "Lorem ipsum dolor sit"
                )).Generate();

        HttpResponseMessage updateResponse =
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{personId}/cats/{randomCatId}", request);

        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails? notFoundProblemDetails = await updateResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        notFoundProblemDetails.Should().NotBeNull();
        notFoundProblemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task UpdateCat_ShouldReturnNotFound_WhenNonExistingPersonIsProvided()
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
        UpdateCatRequest request = new Faker<UpdateCatRequest>()
            .CustomInstantiator(faker =>
                new UpdateCatRequest(
                    Name: faker.Name.FirstName(),
                    IsCastrated: false,
                    MedicalHelpUrgency: MedicalHelpUrgency.ShouldSeeVet.Name,
                    Behavior: Behavior.Unfriendly.Name,
                    HealthStatus: HealthStatus.Terminal.Name,
                    AgeCategory: AgeCategory.Senior.Name,
                    AdditionalRequirements: "Lorem ipsum dolor sit"
                )).Generate();

        HttpResponseMessage updateResponse =
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{randomPersonId}/cats/{catId}", request);

        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails? notFoundProblemDetails = await updateResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        notFoundProblemDetails.Should().NotBeNull();
        notFoundProblemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task UpdateCat_ShouldReturnBadRequest_WhenEmptyDataAreProvided()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        IdResponse<PersonId> personId = await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();

        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/cats", _createCatRequest);
        IdResponse<CatId> catId = await catCreateResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<CatId>>();

        //Act
        UpdateCatRequest request =
            new(
                Name: "",
                IsCastrated: false,
                MedicalHelpUrgency: "",
                Behavior: "",
                HealthStatus: "",
                AgeCategory: ""
            );
        HttpResponseMessage response =
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{personId}/cats/{catId}",
                request);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(5);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo(
            nameof(UpdateCatRequest.Name),
            nameof(UpdateCatRequest.MedicalHelpUrgency),
            nameof(UpdateCatRequest.Behavior),
            nameof(UpdateCatRequest.AgeCategory),
            nameof(UpdateCatRequest.HealthStatus)
        );
        validationProblemDetails.Errors.Values.Count.Should().Be(5);
        validationProblemDetails.Errors[nameof(UpdateCatRequest.Name)][0]
            .Should()
            .Be("'Name' must not be empty.");

        validationProblemDetails.Errors[nameof(UpdateCatRequest.MedicalHelpUrgency)][0]
            .Should()
            .Be("'Medical Help Urgency' must not be empty.");

        validationProblemDetails.Errors[nameof(UpdateCatRequest.Behavior)][0]
            .Should()
            .Be("'Behavior' must not be empty.");

        validationProblemDetails.Errors[nameof(UpdateCatRequest.AgeCategory)][0]
            .Should()
            .Be("'Age Category' must not be empty.");

        validationProblemDetails.Errors[nameof(UpdateCatRequest.HealthStatus)][0]
            .Should()
            .Be("'Health Status' must not be empty.");
    }

    [Fact]
    public async Task UpdateCat_ShouldReturnBadRequest_WhenTooLongDataAreProvided()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        IdResponse<PersonId> personId = await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();

        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/cats", _createCatRequest);
        IdResponse<CatId> catId = await catCreateResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<CatId>>();

        //Act
        UpdateCatRequest request = new Faker<UpdateCatRequest>()
            .CustomInstantiator(faker =>
                new UpdateCatRequest(
                    Name: faker.Person.FirstName.ClampLength(CatName.MaxLength + 1),
                    IsCastrated: true,
                    MedicalHelpUrgency: MedicalHelpUrgency.ShouldSeeVet.Name,
                    Behavior: Behavior.Unfriendly.Name,
                    HealthStatus: HealthStatus.Terminal.Name,
                    AgeCategory: AgeCategory.Baby.Name,
                    AdditionalRequirements: faker.Address.State().ClampLength(Description.MaxLength + 1)
                )).Generate();

        HttpResponseMessage response =
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{personId}/cats/{catId}",
                request);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(2);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo(
            nameof(UpdateCatRequest.Name),
            nameof(UpdateCatRequest.AdditionalRequirements)
        );
        validationProblemDetails.Errors.Values.Count.Should().Be(2);

        validationProblemDetails.Errors[nameof(UpdateCatRequest.Name)][0]
            .Should()
            .StartWith($"The length of 'Name' must be {CatName.MaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(UpdateCatRequest.AdditionalRequirements)][0]
            .Should()
            .StartWith(
                $"The length of 'Additional Requirements' must be {Description.MaxLength} characters or fewer. You entered");
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
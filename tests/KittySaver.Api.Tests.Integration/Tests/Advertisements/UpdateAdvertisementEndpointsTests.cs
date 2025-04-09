using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Infrastructure.Endpoints;
using KittySaver.Api.Tests.Integration.Helpers;
using KittySaver.Domain.ValueObjects;
using KittySaver.Shared.Common.Enums;
using KittySaver.Shared.Hateoas;
using KittySaver.Shared.Responses;
using KittySaver.Shared.TypedIds;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using KittySaver.Tests.Shared;

namespace KittySaver.Api.Tests.Integration.Tests.Advertisements;

[Collection("Api")]
public class UpdateAdvertisementEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CleanupHelper _cleanup;

    public UpdateAdvertisementEndpointsTests(KittySaverApiFactory appFactory)
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
    public async Task UpdateAdvertisement_ShouldReturnSuccess_WhenValidDataIsProvided()
    {
        //Arrange
        CreatePersonRequest? createPersonRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage createPersonResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", createPersonRequest);
        IdResponse<PersonId> personId = await createPersonResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();
        
        PersonResponse person = await _httpClient.GetFromJsonAsync<PersonResponse>($"api/v1/persons/{personId}")
            ?? throw new InvalidOperationException("Person not found");

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
                    ));

        HttpResponseMessage createAdvertisementResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/advertisements", createAdvertisementRequest);
        IdResponse<AdvertisementId> advertisementId = await createAdvertisementResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<AdvertisementId>>();

        //Act
        UpdateAdvertisementRequest request =
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

        HttpResponseMessage updateResponse =
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{personId}/advertisements/{advertisementId}", request);

        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        AdvertisementHateoasResponse? hateoasResponse = await updateResponse.Content.ReadFromJsonAsync<AdvertisementHateoasResponse>();
        hateoasResponse.Should().NotBeNull();
        hateoasResponse!.Id.Should().Be(advertisementId);
        hateoasResponse.PersonId.Should().Be(personId);
        hateoasResponse.Status.Should().Be(AdvertisementStatus.ThumbnailNotUploaded);
        hateoasResponse.Links.Select(x => x.Rel).Should().BeEquivalentTo(
            EndpointRels.SelfRel,
            EndpointNames.UpdateAdvertisementThumbnail.Rel,
            EndpointNames.UpdateAdvertisement.Rel,
            EndpointNames.DeleteAdvertisement.Rel,
            EndpointNames.ReassignCatsToAdvertisement.Rel,
            EndpointNames.GetAdvertisementCats.Rel);
        hateoasResponse.Links.Select(x => x.Href).All(x => !string.IsNullOrEmpty(x)).Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAdvertisement_ShouldReturnBadRequest_WhenTooLongDataAreProvided()
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
                    ));
        HttpResponseMessage createAdvertisementResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/advertisements", createAdvertisementRequest);
        IdResponse<AdvertisementId> advertisementId = await createAdvertisementResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<AdvertisementId>>();

        //Act
        UpdateAdvertisementRequest request = new(
            Description: new string('A', Description.MaxLength + 1),
            PickupAddressCountry: new string('A', Address.CountryMaxLength + 1),
            PickupAddressState: new string('A', Address.StateMaxLength + 1),
            PickupAddressZipCode: new string('A', Address.ZipCodeMaxLength + 1),
            PickupAddressCity: new string('A', Address.CityMaxLength + 1),
            PickupAddressStreet: new string('A', Address.StreetMaxLength + 1),
            PickupAddressBuildingNumber: new string('A', Address.BuildingNumberMaxLength + 1),
            ContactInfoEmail: new string('A', Email.MaxLength + 1),
            ContactInfoPhoneNumber: new string('A', PhoneNumber.MaxLength + 1)
        );

        HttpResponseMessage responseMessage =
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{personId}/advertisements/{advertisementId}", request);

        //Assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await responseMessage.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(9);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo(
            nameof(UpdateAdvertisementRequest.Description),
            nameof(UpdateAdvertisementRequest.PickupAddressCountry),
            nameof(UpdateAdvertisementRequest.PickupAddressState),
            nameof(UpdateAdvertisementRequest.PickupAddressZipCode),
            nameof(UpdateAdvertisementRequest.PickupAddressCity),
            nameof(UpdateAdvertisementRequest.PickupAddressStreet),
            nameof(UpdateAdvertisementRequest.PickupAddressBuildingNumber),
            nameof(UpdateAdvertisementRequest.ContactInfoEmail),
            nameof(UpdateAdvertisementRequest.ContactInfoPhoneNumber)
        );
        validationProblemDetails.Errors.Values.Count.Should().Be(9);
        // inne asercje pozostają takie same
    }

    [Fact]
    public async Task UpdateAdvertisement_ShouldReturnNotFound_WhenNonRegisteredUserIdProvided()
    {
        //Arrange
        PersonId randomPersonId = PersonId.New();
        AdvertisementId randomAdvertisementId = AdvertisementId.New();

        //Act
        UpdateAdvertisementRequest request =
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

        HttpResponseMessage updateResponse =
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{randomPersonId}/advertisements/{randomAdvertisementId}", request);

        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails? notFoundProblemDetails = await updateResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        notFoundProblemDetails.Should().NotBeNull();
        notFoundProblemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task UpdateAdvertisement_ShouldReturnBadRequest_WhenEmptyDataAreProvided()
    {
        //Arrange
        PersonId fakePersonId = PersonId.New();
        AdvertisementId fakeAdvertisementId = AdvertisementId.New();
        
        //Act
        UpdateAdvertisementRequest request = new(
            Description: "",
            PickupAddressCountry: "",
            PickupAddressState: "",
            PickupAddressZipCode: "",
            PickupAddressCity: "",
            PickupAddressStreet: "",
            PickupAddressBuildingNumber: "",
            ContactInfoEmail: "",
            ContactInfoPhoneNumber: ""
        );

        HttpResponseMessage responseMessage =
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{fakePersonId}/advertisements/{fakeAdvertisementId}", request);

        //Assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await responseMessage.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(5);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo(
            nameof(UpdateAdvertisementRequest.PickupAddressCountry),
            nameof(UpdateAdvertisementRequest.PickupAddressZipCode),
            nameof(UpdateAdvertisementRequest.PickupAddressCity),
            nameof(UpdateAdvertisementRequest.ContactInfoEmail),
            nameof(UpdateAdvertisementRequest.ContactInfoPhoneNumber)
        );
        validationProblemDetails.Errors.Values.Count.Should().Be(5);
        // inne asercje pozostają takie same
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
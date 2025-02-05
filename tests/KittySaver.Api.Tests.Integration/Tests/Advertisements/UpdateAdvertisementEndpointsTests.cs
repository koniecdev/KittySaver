using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Features.Advertisements;
using KittySaver.Api.Features.Advertisements.SharedContracts;
using KittySaver.Api.Features.Persons;
using KittySaver.Api.Features.Persons.SharedContracts;
using KittySaver.Api.Shared.Endpoints;
using KittySaver.Api.Shared.Hateoas;
using KittySaver.Api.Tests.Integration.Helpers;
using KittySaver.Domain.Common.Primitives.Enums;
using KittySaver.Domain.Persons;
using KittySaver.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared;
using JsonException = System.Text.Json.JsonException;

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

    private readonly Faker<CreatePerson.CreatePersonRequest> _createPersonRequestGenerator =
        new Faker<CreatePerson.CreatePersonRequest>()
            .CustomInstantiator(faker =>
                new CreatePerson.CreatePersonRequest(
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
    public async Task UpdateAdvertisement_ShouldReturnSuccess_WhenValidDataIsProvided()
    {
        //Arrange
        CreatePerson.CreatePersonRequest? createPersonRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage createPersonResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", createPersonRequest);
        ApiResponses.CreatedWithIdResponse createPersonResponse =
            await createPersonResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>() ??
            throw new JsonException();
        PersonResponse person =
            await _httpClient.GetFromJsonAsync<PersonResponse>($"api/v1/persons/{createPersonResponse.Id}")
            ?? throw new JsonException();

        CreateCat.CreateCatRequest createCatRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage createCatResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{person.Id}/cats", createCatRequest);
        ApiResponses.CreatedWithIdResponse createCatResponse =
            await createCatResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        CreateAdvertisement.CreateAdvertisementRequest createAdvertisementRequest =
            new Faker<CreateAdvertisement.CreateAdvertisementRequest>()
                .CustomInstantiator(faker =>
                    new CreateAdvertisement.CreateAdvertisementRequest(
                        CatsIdsToAssign: [createCatResponse.Id],
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
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{createPersonResponse.Id}/advertisements", createAdvertisementRequest);
        ApiResponses.CreatedWithIdResponse advertisementResponse =
            await createAdvertisementResponseMessage.GetIdResponseFromResponseMessageAsync();

        //Act
        UpdateAdvertisement.UpdateAdvertisementRequest request =
            new Faker<UpdateAdvertisement.UpdateAdvertisementRequest>()
                .CustomInstantiator(faker =>
                    new UpdateAdvertisement.UpdateAdvertisementRequest(
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
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{createPersonResponse.Id}/advertisements/{advertisementResponse.Id}", request);

        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        AdvertisementHateoasResponse? hateoasResponse = await updateResponse.Content.ReadFromJsonAsync<AdvertisementHateoasResponse>();
        hateoasResponse.Should().NotBeNull();
        hateoasResponse!.Id.Should().Be(advertisementResponse.Id);
        hateoasResponse.PersonId.Should().Be(createPersonResponse.Id);
        hateoasResponse.Status.Should().Be(Advertisement.AdvertisementStatus.ThumbnailNotUploaded);
        hateoasResponse.Links.Select(x => x.Rel).Should().BeEquivalentTo(
            EndpointNames.SelfRel,
            EndpointNames.UpdateAdvertisementThumbnail.Rel,
            EndpointNames.DeleteAdvertisement.Rel,
            EndpointNames.UpdateAdvertisement.Rel,
            EndpointNames.ReassignCatsToAdvertisement.Rel);
        hateoasResponse.Links.Select(x => x.Href).All(x => !string.IsNullOrEmpty(x)).Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAdvertisement_ShouldReturnBadRequest_WhenTooLongDataAreProvided()
    {
        //Arrange
        CreatePerson.CreatePersonRequest personRegisterRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", personRegisterRequest);
        ApiResponses.CreatedWithIdResponse personRegisterResponse =
            await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync();
        CreateCat.CreateCatRequest catCreateRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/cats", catCreateRequest);
        ApiResponses.CreatedWithIdResponse catCreateResponse =
            await catCreateResponseMessage.GetIdResponseFromResponseMessageAsync();
        CreateAdvertisement.CreateAdvertisementRequest createAdvertisementRequest =
            new Faker<CreateAdvertisement.CreateAdvertisementRequest>()
                .CustomInstantiator(faker =>
                    new CreateAdvertisement.CreateAdvertisementRequest(
                        CatsIdsToAssign: [catCreateResponse.Id],
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
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/advertisements", createAdvertisementRequest);
        ApiResponses.CreatedWithIdResponse createAdvertisementResponse =
            await createAdvertisementResponseMessage.GetIdResponseFromResponseMessageAsync();

        //Act
        UpdateAdvertisement.UpdateAdvertisementRequest request = new(
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
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{personRegisterResponse.Id}/advertisements/{createAdvertisementResponse.Id}", request);

        //Assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await responseMessage.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(9);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo(
            nameof(UpdateAdvertisement.UpdateAdvertisementRequest.Description),
            nameof(UpdateAdvertisement.UpdateAdvertisementRequest.PickupAddressCountry),
            nameof(UpdateAdvertisement.UpdateAdvertisementRequest.PickupAddressState),
            nameof(UpdateAdvertisement.UpdateAdvertisementRequest.PickupAddressZipCode),
            nameof(UpdateAdvertisement.UpdateAdvertisementRequest.PickupAddressCity),
            nameof(UpdateAdvertisement.UpdateAdvertisementRequest.PickupAddressStreet),
            nameof(UpdateAdvertisement.UpdateAdvertisementRequest.PickupAddressBuildingNumber),
            nameof(UpdateAdvertisement.UpdateAdvertisementRequest.ContactInfoEmail),
            nameof(UpdateAdvertisement.UpdateAdvertisementRequest.ContactInfoPhoneNumber)
        );
        validationProblemDetails.Errors.Values.Count.Should().Be(9);
        validationProblemDetails.Errors[nameof(UpdateAdvertisement.UpdateAdvertisementRequest.Description)][0]
            .Should()
            .StartWith(
                $"The length of '{Extensions.InsertSpacesIntoCamelCase(nameof(UpdateAdvertisement.UpdateAdvertisementRequest.Description))}' must be {Description.MaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(UpdateAdvertisement.UpdateAdvertisementRequest.PickupAddressCountry)][0]
            .Should()
            .StartWith(
                $"The length of '{Extensions.InsertSpacesIntoCamelCase(nameof(UpdateAdvertisement.UpdateAdvertisementRequest.PickupAddressCountry))}' must be {Address.CountryMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(UpdateAdvertisement.UpdateAdvertisementRequest.PickupAddressState)][0]
            .Should()
            .StartWith(
                $"The length of '{Extensions.InsertSpacesIntoCamelCase(nameof(UpdateAdvertisement.UpdateAdvertisementRequest.PickupAddressState))}' must be {Address.StateMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(UpdateAdvertisement.UpdateAdvertisementRequest.PickupAddressZipCode)][0]
            .Should()
            .StartWith(
                $"The length of '{Extensions.InsertSpacesIntoCamelCase(nameof(UpdateAdvertisement.UpdateAdvertisementRequest.PickupAddressZipCode))}' must be {Address.ZipCodeMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(UpdateAdvertisement.UpdateAdvertisementRequest.PickupAddressCity)][0]
            .Should()
            .StartWith(
                $"The length of '{Extensions.InsertSpacesIntoCamelCase(nameof(UpdateAdvertisement.UpdateAdvertisementRequest.PickupAddressCity))}' must be {Address.CityMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(UpdateAdvertisement.UpdateAdvertisementRequest.PickupAddressStreet)][0]
            .Should()
            .StartWith(
                $"The length of '{Extensions.InsertSpacesIntoCamelCase(nameof(UpdateAdvertisement.UpdateAdvertisementRequest.PickupAddressStreet))}' must be {Address.StreetMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[
                nameof(UpdateAdvertisement.UpdateAdvertisementRequest.PickupAddressBuildingNumber)][0]
            .Should()
            .StartWith(
                $"The length of '{Extensions.InsertSpacesIntoCamelCase(nameof(UpdateAdvertisement.UpdateAdvertisementRequest.PickupAddressBuildingNumber))}' must be {Address.BuildingNumberMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(UpdateAdvertisement.UpdateAdvertisementRequest.ContactInfoEmail)][0]
            .Should()
            .StartWith(
                $"The length of '{Extensions.InsertSpacesIntoCamelCase(nameof(UpdateAdvertisement.UpdateAdvertisementRequest.ContactInfoEmail))}' must be {Email.MaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(UpdateAdvertisement.UpdateAdvertisementRequest.ContactInfoPhoneNumber)]
            [0]
            .Should()
            .StartWith(
                $"The length of '{Extensions.InsertSpacesIntoCamelCase(nameof(UpdateAdvertisement.UpdateAdvertisementRequest.ContactInfoPhoneNumber))}' must be {PhoneNumber.MaxLength} characters or fewer. You entered");
    }

    [Fact]
    public async Task UpdateAdvertisement_ShouldReturnNotFound_WhenNonRegisteredUserIdProvided()
    {
        //Arrange
        Guid randomPersonId = Guid.NewGuid();
        Guid randomAdvertisementId = Guid.NewGuid();

        //Act
        UpdateAdvertisement.UpdateAdvertisementRequest request =
            new Faker<UpdateAdvertisement.UpdateAdvertisementRequest>()
                .CustomInstantiator(faker =>
                    new UpdateAdvertisement.UpdateAdvertisementRequest(
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
        Guid fakePersonId = Guid.NewGuid();
        Guid fakeAdvertisementId = Guid.NewGuid();
        //Act
        UpdateAdvertisement.UpdateAdvertisementRequest request = new(
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
            nameof(UpdateAdvertisement.UpdateAdvertisementRequest.PickupAddressCountry),
            nameof(UpdateAdvertisement.UpdateAdvertisementRequest.PickupAddressZipCode),
            nameof(UpdateAdvertisement.UpdateAdvertisementRequest.PickupAddressCity),
            nameof(UpdateAdvertisement.UpdateAdvertisementRequest.ContactInfoEmail),
            nameof(UpdateAdvertisement.UpdateAdvertisementRequest.ContactInfoPhoneNumber)
        );
        validationProblemDetails.Errors.Values.Count.Should().Be(5);
        validationProblemDetails.Errors[nameof(UpdateAdvertisement.UpdateAdvertisementRequest.PickupAddressCountry)][0]
            .Should()
            .Be(
                $"'{Extensions.InsertSpacesIntoCamelCase(nameof(UpdateAdvertisement.UpdateAdvertisementRequest.PickupAddressCountry))}' must not be empty.");

        validationProblemDetails.Errors[nameof(UpdateAdvertisement.UpdateAdvertisementRequest.PickupAddressZipCode)][0]
            .Should()
            .Be(
                $"'{Extensions.InsertSpacesIntoCamelCase(nameof(UpdateAdvertisement.UpdateAdvertisementRequest.PickupAddressZipCode))}' must not be empty.");

        validationProblemDetails.Errors[nameof(UpdateAdvertisement.UpdateAdvertisementRequest.PickupAddressCity)][0]
            .Should()
            .Be(
                $"'{Extensions.InsertSpacesIntoCamelCase(nameof(UpdateAdvertisement.UpdateAdvertisementRequest.PickupAddressCity))}' must not be empty.");
        
        validationProblemDetails.Errors[nameof(UpdateAdvertisement.UpdateAdvertisementRequest.ContactInfoEmail)][0]
            .Should()
            .Be(
                $"'{Extensions.InsertSpacesIntoCamelCase(nameof(UpdateAdvertisement.UpdateAdvertisementRequest.ContactInfoEmail))}' must not be empty.");

        validationProblemDetails.Errors[nameof(UpdateAdvertisement.UpdateAdvertisementRequest.ContactInfoPhoneNumber)][0]
            .Should()
            .Be($"'{Extensions.InsertSpacesIntoCamelCase(nameof(UpdateAdvertisement.UpdateAdvertisementRequest.ContactInfoPhoneNumber))}' must not be empty.");
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
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using Bogus.Extensions;
using FluentAssertions;
using KittySaver.Api.Features.Advertisements;
using KittySaver.Api.Features.Persons;
using KittySaver.Api.Shared.Domain.Common.Primitives.Enums;
using KittySaver.Api.Shared.Domain.Persons;
using KittySaver.Api.Shared.Domain.ValueObjects;
using KittySaver.Api.Tests.Integration.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace KittySaver.Api.Tests.Integration.Tests.Advertisements;

[Collection("Api")]
public class CreateAdvertisementEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CleanupHelper _cleanup;

    public CreateAdvertisementEndpointsTests(KittySaverApiFactory appFactory)
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
    public async Task CreateAdvertisement_ShouldReturnSuccess_WhenOneCatIsProvided()
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

        //Act
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

        HttpResponseMessage responseMessage = await _httpClient.PostAsJsonAsync("api/v1/advertisements", request);

        //Assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.Created);
        ApiResponses.CreatedWithIdResponse? response =
            await responseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>();
        response.Should().NotBeNull();
        response!.Id.Should().NotBeEmpty();
        responseMessage.Headers.Location!.ToString().Should().Contain($"/api/v1/advertisements/{response.Id}");
    }

    [Fact]
    public async Task CreateAdvertisement_ShouldReturnSuccess_WhenMultipleCatIsProvided()
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

        //Assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.Created);
        ApiResponses.CreatedWithIdResponse? response =
            await responseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>();
        response.Should().NotBeNull();
        response!.Id.Should().NotBeEmpty();
        responseMessage.Headers.Location!.ToString().Should().Contain($"/api/v1/advertisements/{response.Id}");
    }

    [Fact]
    public async Task CreateAdvertisement_ShouldReturnNotFound_WhenCatOfAnotherPersonIsProvided()
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

        CreatePerson.CreatePersonRequest secondPersonRegisterRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage secondPersonRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", secondPersonRegisterRequest);
        ApiResponses.CreatedWithIdResponse secondPersonRegisterResponse =
            await secondPersonRegisterResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        CreateCat.CreateCatRequest secondPersonCatCreateRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage secondPersonCatCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{secondPersonRegisterResponse.Id}/cats",
                secondPersonCatCreateRequest);
        ApiResponses.CreatedWithIdResponse secondPersonCatCreateResponse =
            await secondPersonCatCreateResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();

        //Act
        CreateAdvertisement.CreateAdvertisementRequest request =
            new Faker<CreateAdvertisement.CreateAdvertisementRequest>()
                .CustomInstantiator(faker =>
                    new CreateAdvertisement.CreateAdvertisementRequest(
                        PersonId: personRegisterResponse.Id,
                        CatsIdsToAssign: [catCreateResponse.Id, secondPersonCatCreateResponse.Id],
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

        //Assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails? problemDetails = await responseMessage.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task CreateAdvertisement_ShouldReturnSuccess_WhenValidDataIsProvidedWithoutStateAndDescription(
        string? emptyValue)
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

        //Act
        CreateAdvertisement.CreateAdvertisementRequest request =
            new Faker<CreateAdvertisement.CreateAdvertisementRequest>()
                .CustomInstantiator(faker =>
                    new CreateAdvertisement.CreateAdvertisementRequest(
                        PersonId: personRegisterResponse.Id,
                        CatsIdsToAssign: [catCreateResponse.Id],
                        Description: emptyValue,
                        PickupAddressCountry: faker.Address.Country(),
                        PickupAddressState: emptyValue,
                        PickupAddressZipCode: faker.Address.ZipCode(),
                        PickupAddressCity: faker.Address.City(),
                        PickupAddressStreet: faker.Address.StreetName(),
                        PickupAddressBuildingNumber: faker.Address.BuildingNumber(),
                        ContactInfoEmail: faker.Person.Email,
                        ContactInfoPhoneNumber: faker.Person.Phone
                    ));

        HttpResponseMessage responseMessage = await _httpClient.PostAsJsonAsync("api/v1/advertisements", request);

        //Assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.Created);
        ApiResponses.CreatedWithIdResponse? response =
            await responseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>();
        response.Should().NotBeNull();
        response!.Id.Should().NotBeEmpty();
        responseMessage.Headers.Location!.ToString().Should().Contain($"/api/v1/advertisements/{response.Id}");
    }
    
    [Fact]
    public async Task CreateAdvertisement_ShouldReturnBadRequest_WhenAlreadyAssignedCatIsProvided()
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
        await _httpClient.PostAsJsonAsync("api/v1/advertisements", request);

        //Act
        HttpResponseMessage responseMessage = await _httpClient.PostAsJsonAsync("api/v1/advertisements", request);

        //Assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateAdvertisement_ShouldReturnBadRequest_WhenTooLongDataAreProvided()
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

        //Act
        CreateAdvertisement.CreateAdvertisementRequest request = new(
            PersonId: personRegisterResponse.Id,
            CatsIdsToAssign: [catCreateResponse.Id],
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

        HttpResponseMessage responseMessage = await _httpClient.PostAsJsonAsync("api/v1/advertisements", request);

        //Assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await responseMessage.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(9);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo(
            nameof(CreateAdvertisement.CreateAdvertisementRequest.Description),
            nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressCountry),
            nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressState),
            nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressZipCode),
            nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressCity),
            nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressStreet),
            nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressBuildingNumber),
            nameof(CreateAdvertisement.CreateAdvertisementRequest.ContactInfoEmail),
            nameof(CreateAdvertisement.CreateAdvertisementRequest.ContactInfoPhoneNumber)
        );
        validationProblemDetails.Errors.Values.Count.Should().Be(9);

        validationProblemDetails.Errors[nameof(CreateAdvertisement.CreateAdvertisementRequest.Description)][0]
            .Should()
            .StartWith(
                $"The length of '{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementRequest.Description))}' must be {Description.MaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressCountry)][0]
            .Should()
            .StartWith(
                $"The length of '{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressCountry))}' must be {Address.CountryMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressState)][0]
            .Should()
            .StartWith(
                $"The length of '{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressState))}' must be {Address.StateMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressZipCode)][0]
            .Should()
            .StartWith(
                $"The length of '{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressZipCode))}' must be {Address.ZipCodeMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressCity)][0]
            .Should()
            .StartWith(
                $"The length of '{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressCity))}' must be {Address.CityMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressStreet)][0]
            .Should()
            .StartWith(
                $"The length of '{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressStreet))}' must be {Address.StreetMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[
                nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressBuildingNumber)][0]
            .Should()
            .StartWith(
                $"The length of '{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressBuildingNumber))}' must be {Address.BuildingNumberMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreateAdvertisement.CreateAdvertisementRequest.ContactInfoEmail)][0]
            .Should()
            .StartWith(
                $"The length of '{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementRequest.ContactInfoEmail))}' must be {Email.MaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreateAdvertisement.CreateAdvertisementRequest.ContactInfoPhoneNumber)]
            [0]
            .Should()
            .StartWith(
                $"The length of '{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementRequest.ContactInfoPhoneNumber))}' must be {PhoneNumber.MaxLength} characters or fewer. You entered");
    }

    [Fact]
    public async Task CreateAdvertisement_ShouldReturnBadRequest_WhenEmptyDataAreProvided()
    {
        //Act
        CreateAdvertisement.CreateAdvertisementRequest request = new(
            PersonId: Guid.Empty,
            CatsIdsToAssign: [],
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

        HttpResponseMessage responseMessage = await _httpClient.PostAsJsonAsync("api/v1/advertisements", request);

        //Assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await responseMessage.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(9);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo(
            nameof(CreateAdvertisement.CreateAdvertisementRequest.PersonId),
            nameof(CreateAdvertisement.CreateAdvertisementRequest.CatsIdsToAssign),
            nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressCountry),
            nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressZipCode),
            nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressCity),
            nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressStreet),
            nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressBuildingNumber),
            nameof(CreateAdvertisement.CreateAdvertisementRequest.ContactInfoEmail),
            nameof(CreateAdvertisement.CreateAdvertisementRequest.ContactInfoPhoneNumber)
        );
        validationProblemDetails.Errors.Values.Count.Should().Be(9);

        validationProblemDetails.Errors[nameof(CreateAdvertisement.CreateAdvertisementRequest.PersonId)][0]
            .Should()
            .Be($"'{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementRequest.PersonId))}' must not be empty.");

        validationProblemDetails.Errors[nameof(CreateAdvertisement.CreateAdvertisementRequest.CatsIdsToAssign)][0]
            .Should()
            .Be($"'{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementRequest.CatsIdsToAssign))}' must not be empty.");
        
        validationProblemDetails.Errors[nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressCountry)][0]
            .Should()
            .Be($"'{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressCountry))}' must not be empty.");
        
        validationProblemDetails.Errors[nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressZipCode)][0]
            .Should()
            .Be($"'{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressZipCode))}' must not be empty.");

        validationProblemDetails.Errors[nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressCity)][0]
            .Should()
            .Be($"'{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressCity))}' must not be empty.");

        validationProblemDetails.Errors[nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressStreet)][0]
            .Should()
            .Be($"'{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressStreet))}' must not be empty.");

        validationProblemDetails.Errors[nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressBuildingNumber)][0]
            .Should()
            .Be($"'{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementRequest.PickupAddressBuildingNumber))}' must not be empty.");

        validationProblemDetails.Errors[nameof(CreateAdvertisement.CreateAdvertisementRequest.ContactInfoEmail)][0]
            .Should()
            .Be($"'{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementRequest.ContactInfoEmail))}' must not be empty.");

        validationProblemDetails.Errors[nameof(CreateAdvertisement.CreateAdvertisementRequest.ContactInfoPhoneNumber)][0]
            .Should()
            .Be($"'{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementRequest.ContactInfoPhoneNumber))}' must not be empty.");
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
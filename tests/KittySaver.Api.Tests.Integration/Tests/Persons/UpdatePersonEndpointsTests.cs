using System.Net;
using System.Net.Http.Json;
using Bogus;
using Bogus.Extensions;
using FluentAssertions;
using KittySaver.Api.Infrastructure.Endpoints;
using KittySaver.Api.Tests.Integration.Helpers;
using KittySaver.Domain.Persons.ValueObjects;
using KittySaver.Domain.ValueObjects;
using KittySaver.Shared.Hateoas;
using KittySaver.Shared.Responses;
using KittySaver.Shared.TypedIds;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using KittySaver.Tests.Shared;
using JsonException = System.Text.Json.JsonException;

namespace KittySaver.Api.Tests.Integration.Tests.Persons;

[Collection("Api")]
public class UpdatePersonEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CleanupHelper _cleanup;

    public UpdatePersonEndpointsTests(KittySaverApiFactory appFactory)
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

    [Fact]
    public async Task UpdatePerson_ShouldReturnSuccess_WhenValidDataIsProvided()
    {
        //Arrange
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        IdResponse<PersonId> personId = await response.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();

        PersonResponse person =
            await _httpClient.GetFromJsonAsync<PersonResponse>($"api/v1/persons/{personId}")
            ?? throw new JsonException();
        
        //Act
        UpdatePersonRequest request = new Faker<UpdatePersonRequest>()
            .CustomInstantiator(faker =>
                new UpdatePersonRequest(
                    Nickname: faker.Person.FirstName,
                    Email: faker.Person.Email,
                    PhoneNumber: faker.Person.Phone,
                    DefaultAdvertisementPickupAddressCountry: faker.Address.CountryCode(),
                    DefaultAdvertisementPickupAddressState: faker.Address.State(),
                    DefaultAdvertisementPickupAddressZipCode: faker.Address.ZipCode(),
                    DefaultAdvertisementPickupAddressCity: faker.Address.City(),
                    DefaultAdvertisementPickupAddressStreet: faker.Address.StreetName(),
                    DefaultAdvertisementPickupAddressBuildingNumber: faker.Address.BuildingNumber(),
                    DefaultAdvertisementContactInfoEmail: faker.Person.Email,
                    DefaultAdvertisementContactInfoPhoneNumber: faker.Person.Phone
                ));

        HttpResponseMessage updateResponse = await _httpClient.PutAsJsonAsync($"api/v1/persons/{personId}", request);
        
        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        PersonHateoasResponse hateoasResponse =
            await updateResponse.Content.ReadFromJsonAsync<PersonHateoasResponse>()
            ?? throw new JsonException();
        hateoasResponse.Links.Select(x => x.Rel).Should()
            .BeEquivalentTo(EndpointRels.SelfRel,
                EndpointNames.UpdatePerson.Rel,
                EndpointNames.DeletePerson.Rel,
                EndpointNames.GetCats.Rel,
                EndpointNames.GetAdvertisements.Rel,
                EndpointNames.CreateCat.Rel,
                EndpointNames.CreateAdvertisement.Rel);
        hateoasResponse.Links.Select(x => x.Href).All(x => x.Contains("://")).Should().BeTrue();
        hateoasResponse.Id.Should().Be(person.Id);

        PersonResponse personAfterUpdate =
            await _httpClient.GetFromJsonAsync<PersonResponse>($"api/v1/persons/{personId}")
            ?? throw new JsonException();
        personAfterUpdate.Should().NotBeEquivalentTo(person);
        personAfterUpdate.Nickname.Should().Be(request.Nickname);
        personAfterUpdate.Email.Should().Be(request.Email);
        personAfterUpdate.PhoneNumber.Should().Be(request.PhoneNumber);
    }

    [Fact]
    public async Task UpdatePerson_ShouldReturnNotFound_WhenNonRegisteredUserIdProvided()
    {
        //Arrange
        PersonId randomId = PersonId.New();

        //Act
        UpdatePersonRequest request = new Faker<UpdatePersonRequest>()
            .CustomInstantiator(faker =>
                new UpdatePersonRequest(
                    Nickname: faker.Person.FirstName,
                    Email: faker.Person.Email,
                    PhoneNumber: faker.Person.Phone,
                    DefaultAdvertisementPickupAddressCountry: faker.Address.CountryCode(),
                    DefaultAdvertisementPickupAddressState: faker.Address.State(),
                    DefaultAdvertisementPickupAddressZipCode: faker.Address.ZipCode(),
                    DefaultAdvertisementPickupAddressCity: faker.Address.City(),
                    DefaultAdvertisementPickupAddressStreet: faker.Address.StreetName(),
                    DefaultAdvertisementPickupAddressBuildingNumber: faker.Address.BuildingNumber(),
                    DefaultAdvertisementContactInfoEmail: faker.Person.Email,
                    DefaultAdvertisementContactInfoPhoneNumber: faker.Person.Phone
                ));

        HttpResponseMessage updateResponse = await _httpClient.PutAsJsonAsync($"api/v1/persons/{randomId}", request);

        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails? notFoundProblemDetails = await updateResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        notFoundProblemDetails.Should().NotBeNull();
        notFoundProblemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task UpdatePerson_ShouldReturnBadRequest_WhenEmptyDataAreProvided()
    {
        //Arrange
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        IdResponse<PersonId> personId = await response.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();

        //Act
        UpdatePersonRequest request = new(
            Nickname: "",
            Email: "",
            PhoneNumber: "",
            DefaultAdvertisementPickupAddressCountry: "",
            DefaultAdvertisementPickupAddressState: "",
            DefaultAdvertisementPickupAddressZipCode: "",
            DefaultAdvertisementPickupAddressCity: "",
            DefaultAdvertisementPickupAddressStreet: "",
            DefaultAdvertisementPickupAddressBuildingNumber: "",
            DefaultAdvertisementContactInfoEmail: "",
            DefaultAdvertisementContactInfoPhoneNumber: ""
        );
        HttpResponseMessage updateResponse =
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{personId}", request);

        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await updateResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(8);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo(
            nameof(UpdatePersonRequest.Nickname),
            nameof(UpdatePersonRequest.Email),
            nameof(UpdatePersonRequest.PhoneNumber),
            nameof(UpdatePersonRequest.DefaultAdvertisementContactInfoEmail),
            nameof(UpdatePersonRequest.DefaultAdvertisementContactInfoPhoneNumber),
            nameof(UpdatePersonRequest.DefaultAdvertisementPickupAddressCountry),
            nameof(UpdatePersonRequest.DefaultAdvertisementPickupAddressZipCode),
            nameof(UpdatePersonRequest.DefaultAdvertisementPickupAddressCity)
        );

        validationProblemDetails.Errors.Values.Count.Should().Be(8);

        // validationProblemDetails.Errors[nameof(UpdatePersonRequest.Nickname)][0]
        //     .Should()
        //     .Be("'Nickname' must not be empty.");
        //
        // validationProblemDetails.Errors[nameof(CreatePersonRequest.Email)][0]
        //     .Should()
        //     .Be("'Email' must not be empty.");
        //
        // validationProblemDetails.Errors[nameof(CreatePersonRequest.PhoneNumber)][0]
        //     .Should()
        //     .Be("'Phone Number' must not be empty.");
        //
        // validationProblemDetails.Errors[nameof(CreatePersonRequest.DefaultAdvertisementContactInfoEmail)]
        //     [0]
        //     .Should()
        //     .Be("'Default Advertisement Contact Info Email' must not be empty.");
        //
        // validationProblemDetails.Errors[
        //         nameof(CreatePersonRequest.DefaultAdvertisementContactInfoPhoneNumber)][0]
        //     .Should()
        //     .Be("'Default Advertisement Contact Info Phone Number' must not be empty.");
        //
        // validationProblemDetails.Errors[
        //         nameof(UpdatePersonRequest.DefaultAdvertisementPickupAddressCountry)][0]
        //     .Should()
        //     .Be("'Default Advertisement Pickup Address Country' must not be empty.");
        //
        // validationProblemDetails.Errors[
        //         nameof(UpdatePersonRequest.DefaultAdvertisementPickupAddressZipCode)][0]
        //     .Should()
        //     .Be("'Default Advertisement Pickup Address Zip Code' must not be empty.");
        //
        // validationProblemDetails.Errors[nameof(UpdatePersonRequest.DefaultAdvertisementPickupAddressCity)]
        //     [0]
        //     .Should()
        //     .Be("'Default Advertisement Pickup Address City' must not be empty.");
    }

    [Fact]
    public async Task UpdatePerson_ShouldReturnBadRequest_WhenTooLongDataAreProvided()
    {
        //Arrange
        HttpResponseMessage registerResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        IdResponse<PersonId> personId = await registerResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();

        UpdatePersonRequest request = new Faker<UpdatePersonRequest>()
            .CustomInstantiator(faker =>
                new UpdatePersonRequest(
                    Nickname: faker.Person.FirstName.ClampLength(Nickname.MaxLength + 1),
                    Email: faker.Person.Email.ClampLength(Email.MaxLength + 1),
                    PhoneNumber: faker.Person.Phone.ClampLength(PhoneNumber.MaxLength + 1),
                    DefaultAdvertisementPickupAddressCountry: faker.Address.CountryCode()
                        .ClampLength(Address.CountryMaxLength + 1),
                    DefaultAdvertisementPickupAddressState: faker.Address.ZipCode()
                        .ClampLength(Address.StateMaxLength + 1),
                    DefaultAdvertisementPickupAddressZipCode: faker.Address.City()
                        .ClampLength(Address.ZipCodeMaxLength + 1),
                    DefaultAdvertisementPickupAddressCity: faker.Address.StreetName()
                        .ClampLength(Address.CityMaxLength + 1),
                    DefaultAdvertisementPickupAddressStreet: faker.Address.BuildingNumber()
                        .ClampLength(Address.StreetMaxLength + 1),
                    DefaultAdvertisementPickupAddressBuildingNumber: faker.Address.State()
                        .ClampLength(Address.BuildingNumberMaxLength + 1),
                    DefaultAdvertisementContactInfoEmail: faker.Person.Email.ClampLength(Email.MaxLength + 1),
                    DefaultAdvertisementContactInfoPhoneNumber: faker.Person.Phone.ClampLength(
                        PhoneNumber.MaxLength + 1)
                ));

        //Act
        HttpResponseMessage response =
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{personId}", request);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(11);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo(
            nameof(CreatePersonRequest.Nickname),
            nameof(CreatePersonRequest.Email),
            nameof(CreatePersonRequest.PhoneNumber),
            nameof(CreatePersonRequest.DefaultAdvertisementPickupAddressCountry),
            nameof(CreatePersonRequest.DefaultAdvertisementPickupAddressState),
            nameof(CreatePersonRequest.DefaultAdvertisementPickupAddressZipCode),
            nameof(CreatePersonRequest.DefaultAdvertisementPickupAddressCity),
            nameof(CreatePersonRequest.DefaultAdvertisementPickupAddressStreet),
            nameof(CreatePersonRequest.DefaultAdvertisementPickupAddressBuildingNumber),
            nameof(CreatePersonRequest.DefaultAdvertisementContactInfoEmail),
            nameof(CreatePersonRequest.DefaultAdvertisementContactInfoPhoneNumber)
        );
        validationProblemDetails.Errors.Values.Count.Should().Be(11);

        // validationProblemDetails.Errors[nameof(CreatePersonRequest.Nickname)][0]
        //     .Should()
        //     .StartWith($"The length of 'Nickname' must be {Nickname.MaxLength} characters or fewer. You entered");
        //
        // validationProblemDetails.Errors[nameof(CreatePersonRequest.PhoneNumber)][0]
        //     .Should()
        //     .StartWith(
        //         $"The length of 'Phone Number' must be {PhoneNumber.MaxLength} characters or fewer. You entered");
        //
        // validationProblemDetails.Errors[nameof(CreatePersonRequest.Email)][0]
        //     .Should()
        //     .StartWith($"The length of 'Email' must be {Email.MaxLength} characters or fewer. You entered");
        //
        // validationProblemDetails.Errors[
        //         nameof(CreatePersonRequest.DefaultAdvertisementContactInfoPhoneNumber)][0]
        //     .Should()
        //     .StartWith(
        //         $"The length of 'Default Advertisement Contact Info Phone Number' must be {PhoneNumber.MaxLength} characters or fewer. You entered");
        //
        // validationProblemDetails.Errors[nameof(CreatePersonRequest.DefaultAdvertisementContactInfoEmail)]
        //     [0]
        //     .Should()
        //     .StartWith(
        //         $"The length of 'Default Advertisement Contact Info Email' must be {Email.MaxLength} characters or fewer. You entered");
        //
        // validationProblemDetails.Errors[
        //         nameof(CreatePersonRequest.DefaultAdvertisementPickupAddressCountry)][0]
        //     .Should()
        //     .StartWith(
        //         $"The length of 'Default Advertisement Pickup Address Country' must be {Address.CountryMaxLength} characters or fewer. You entered");
        //
        // validationProblemDetails.Errors[nameof(CreatePersonRequest.DefaultAdvertisementPickupAddressState)]
        //     [0]
        //     .Should()
        //     .StartWith(
        //         $"The length of 'Default Advertisement Pickup Address State' must be {Address.StateMaxLength} characters or fewer. You entered");
        //
        // validationProblemDetails.Errors[
        //         nameof(CreatePersonRequest.DefaultAdvertisementPickupAddressZipCode)][0]
        //     .Should()
        //     .StartWith(
        //         $"The length of 'Default Advertisement Pickup Address Zip Code' must be {Address.ZipCodeMaxLength} characters or fewer. You entered");
        //
        // validationProblemDetails.Errors[nameof(CreatePersonRequest.DefaultAdvertisementPickupAddressCity)]
        //     [0]
        //     .Should()
        //     .StartWith(
        //         $"The length of 'Default Advertisement Pickup Address City' must be {Address.CityMaxLength} characters or fewer. You entered");
        //
        // validationProblemDetails.Errors[
        //         nameof(CreatePersonRequest.DefaultAdvertisementPickupAddressStreet)][0]
        //     .Should()
        //     .StartWith(
        //         $"The length of 'Default Advertisement Pickup Address Street' must be {Address.StreetMaxLength} characters or fewer. You entered");
        //
        // validationProblemDetails.Errors[
        //         nameof(CreatePersonRequest.DefaultAdvertisementPickupAddressBuildingNumber)][0]
        //     .Should()
        //     .StartWith(
        //         $"The length of 'Default Advertisement Pickup Address Building Number' must be {Address.BuildingNumberMaxLength} characters or fewer. You entered");
    }

    [Theory]
    [ClassData(typeof(InvalidEmailData))]
    public async Task UpdatePerson_ShouldReturnBadRequest_WhenInvalidEmailIsProvided(string email)
    {
        //Arrange
        HttpResponseMessage createPersonResponse =
            await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        IdResponse<PersonId> personId = await createPersonResponse.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();
        
        PersonResponse person =
            await _httpClient.GetFromJsonAsync<PersonResponse>($"api/v1/persons/{personId}")
            ?? throw new JsonException();

        UpdatePersonRequest request = new(
            Nickname: person.Nickname,
            Email: email,
            PhoneNumber: person.PhoneNumber,
            DefaultAdvertisementPickupAddressCountry: person.DefaultAdvertisementsPickupAddress.Country,
            DefaultAdvertisementPickupAddressState: person.DefaultAdvertisementsPickupAddress.State,
            DefaultAdvertisementPickupAddressZipCode: person.DefaultAdvertisementsPickupAddress.ZipCode,
            DefaultAdvertisementPickupAddressCity: person.DefaultAdvertisementsPickupAddress.City,
            DefaultAdvertisementPickupAddressStreet: person.DefaultAdvertisementsPickupAddress.Street,
            DefaultAdvertisementPickupAddressBuildingNumber: person.DefaultAdvertisementsPickupAddress.BuildingNumber,
            DefaultAdvertisementContactInfoEmail: person.DefaultAdvertisementsContactInfoEmail,
            DefaultAdvertisementContactInfoPhoneNumber: person.DefaultAdvertisementsContactInfoPhoneNumber
        );

        //Act
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/v1/persons/{person.Id}", request);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(1);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo(nameof(UpdatePersonRequest.Email));
        validationProblemDetails.Errors.Values.Count.Should().Be(1);
        // validationProblemDetails.Errors[nameof(UpdatePersonRequest.Email)][0]
        //     .Should().Be("'Email' is not in the correct format.");
    }

    [Fact]
    public async Task UpdatePerson_ShouldReturnBadRequest_WhenAlreadyTakenUniquePropertiesAreProvided()
    {
        //Arrange
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        IdResponse<PersonId> personId = await response.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();

        CreatePersonRequest secondPersonCreateRequest = _createPersonRequest with
        {
            Email = "unique@email.com",
            PhoneNumber = "420420420"
        };
        _ = await _httpClient.PostAsJsonAsync("api/v1/persons", secondPersonCreateRequest);


        PersonResponse firstUser =
            await _httpClient.GetFromJsonAsync<PersonResponse>($"api/v1/persons/{personId}")
            ?? throw new JsonException();

        //Act
        UpdatePersonRequest firstUserUpdateRequest = new(
            Nickname: firstUser.Nickname,
            Email: secondPersonCreateRequest.Email,
            PhoneNumber: secondPersonCreateRequest.PhoneNumber,
            DefaultAdvertisementPickupAddressCountry: firstUser.DefaultAdvertisementsPickupAddress.Country,
            DefaultAdvertisementPickupAddressState: firstUser.DefaultAdvertisementsPickupAddress.State,
            DefaultAdvertisementPickupAddressZipCode: firstUser.DefaultAdvertisementsPickupAddress.ZipCode,
            DefaultAdvertisementPickupAddressCity: firstUser.DefaultAdvertisementsPickupAddress.City,
            DefaultAdvertisementPickupAddressStreet: firstUser.DefaultAdvertisementsPickupAddress.Street,
            DefaultAdvertisementPickupAddressBuildingNumber:
            firstUser.DefaultAdvertisementsPickupAddress.BuildingNumber,
            DefaultAdvertisementContactInfoEmail: firstUser.DefaultAdvertisementsContactInfoEmail,
            DefaultAdvertisementContactInfoPhoneNumber: firstUser.DefaultAdvertisementsContactInfoPhoneNumber
        );

        HttpResponseMessage updateResponse =
            await _httpClient.PutAsJsonAsync($"api/v1/persons/{personId}", firstUserUpdateRequest);

        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await updateResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(2);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo([
            nameof(UpdatePersonRequest.Email),
            nameof(UpdatePersonRequest.PhoneNumber)
        ]);
        validationProblemDetails.Errors.Values.Count.Should().Be(2);
        // validationProblemDetails.Errors[nameof(UpdatePersonRequest.Email)][0]
        //     .Should().Be("'Email' is already used by another user.");
        // validationProblemDetails.Errors[nameof(UpdatePersonRequest.PhoneNumber)][0]
        //     .Should().Be("'Phone Number' is already used by another user.");
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
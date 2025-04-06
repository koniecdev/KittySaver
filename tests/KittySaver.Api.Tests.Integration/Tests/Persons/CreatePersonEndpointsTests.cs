using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using Bogus.Extensions;
using FluentAssertions;
using KittySaver.Api.Shared.Endpoints;
using KittySaver.Api.Tests.Integration.Helpers;
using KittySaver.Domain.Persons;
using KittySaver.Domain.Persons.ValueObjects;
using KittySaver.Domain.ValueObjects;
using KittySaver.Shared.Hateoas;
using KittySaver.Shared.Responses;
using KittySaver.Shared.TypedIds;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using KittySaver.Tests.Shared;

namespace KittySaver.Api.Tests.Integration.Tests.Persons;

[Collection("Api")]
public class CreatePersonEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CleanupHelper _cleanup;

    public CreatePersonEndpointsTests(KittySaverApiFactory appFactory)
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

    [Fact]
    public async Task CreatePerson_ShouldReturnSuccess_WhenValidDataIsProvided()
    {
        //Arrange
        CreatePersonRequest request = _createPersonRequestGenerator.Generate();

        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", request);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        PersonHateoasResponse hateoasResponse =
            await response.Content.ReadFromJsonAsync<PersonHateoasResponse>()
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
        response.Headers.Location!.ToString().Should().Contain($"/api/v1/persons/{hateoasResponse.Id}");
        PersonResponse person =
            await _httpClient.GetFromJsonAsync<PersonResponse>($"api/v1/persons/{hateoasResponse.Id}")
            ?? throw new JsonException();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task CreatePerson_ShouldReturnSuccess_WhenValidDataIsProvidedWithoutState(string? state)
    {
        //Arrange
        CreatePersonRequest request = new Faker<CreatePersonRequest>()
            .CustomInstantiator(faker =>
                new CreatePersonRequest(
                    Nickname: faker.Person.FirstName,
                    Email: faker.Person.Email,
                    PhoneNumber: faker.Person.Phone,
                    Password: "Default123$",
                    DefaultAdvertisementPickupAddressCountry: faker.Address.CountryCode(),
                    DefaultAdvertisementPickupAddressState: state,
                    DefaultAdvertisementPickupAddressZipCode: faker.Address.ZipCode(),
                    DefaultAdvertisementPickupAddressCity: faker.Address.City(),
                    DefaultAdvertisementPickupAddressStreet: faker.Address.StreetName(),
                    DefaultAdvertisementPickupAddressBuildingNumber: faker.Address.BuildingNumber(),
                    DefaultAdvertisementContactInfoEmail: faker.Person.Email,
                    DefaultAdvertisementContactInfoPhoneNumber: faker.Person.Phone
                ));

        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", request);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        PersonHateoasResponse hateoasResponse =
            await response.Content.ReadFromJsonAsync<PersonHateoasResponse>()
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
        hateoasResponse.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/api/v1/persons/{hateoasResponse.Id}");
    }

    [Fact]
    public async Task CreatePerson_ShouldReturnBadRequest_WhenTooLongDataAreProvided()
    {
        //Arrange
        CreatePersonRequest request = new Faker<CreatePersonRequest>()
            .CustomInstantiator(faker =>
                new CreatePersonRequest(
                    Nickname: faker.Person.FirstName.ClampLength(Nickname.MaxLength + 1),
                    Email: faker.Person.Email.ClampLength(Email.MaxLength + 1),
                    PhoneNumber: faker.Person.Phone.ClampLength(PhoneNumber.MaxLength + 1),
                    Password: "Default123$",
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
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", request);

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

        validationProblemDetails.Errors[nameof(CreatePersonRequest.Nickname)][0]
            .Should()
            .StartWith($"The length of 'Nickname' must be {Nickname.MaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreatePersonRequest.PhoneNumber)][0]
            .Should()
            .StartWith(
                $"The length of 'Phone Number' must be {PhoneNumber.MaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreatePersonRequest.Email)][0]
            .Should()
            .StartWith($"The length of 'Email' must be {Email.MaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[
                nameof(CreatePersonRequest.DefaultAdvertisementContactInfoPhoneNumber)][0]
            .Should()
            .StartWith(
                $"The length of 'Default Advertisement Contact Info Phone Number' must be {PhoneNumber.MaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreatePersonRequest.DefaultAdvertisementContactInfoEmail)]
            [0]
            .Should()
            .StartWith(
                $"The length of 'Default Advertisement Contact Info Email' must be {Email.MaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[
                nameof(CreatePersonRequest.DefaultAdvertisementPickupAddressCountry)][0]
            .Should()
            .StartWith(
                $"The length of 'Default Advertisement Pickup Address Country' must be {Address.CountryMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreatePersonRequest.DefaultAdvertisementPickupAddressState)]
            [0]
            .Should()
            .StartWith(
                $"The length of 'Default Advertisement Pickup Address State' must be {Address.StateMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[
                nameof(CreatePersonRequest.DefaultAdvertisementPickupAddressZipCode)][0]
            .Should()
            .StartWith(
                $"The length of 'Default Advertisement Pickup Address Zip Code' must be {Address.ZipCodeMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreatePersonRequest.DefaultAdvertisementPickupAddressCity)]
            [0]
            .Should()
            .StartWith(
                $"The length of 'Default Advertisement Pickup Address City' must be {Address.CityMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[
                nameof(CreatePersonRequest.DefaultAdvertisementPickupAddressStreet)][0]
            .Should()
            .StartWith(
                $"The length of 'Default Advertisement Pickup Address Street' must be {Address.StreetMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[
                nameof(CreatePersonRequest.DefaultAdvertisementPickupAddressBuildingNumber)][0]
            .Should()
            .StartWith(
                $"The length of 'Default Advertisement Pickup Address Building Number' must be {Address.BuildingNumberMaxLength} characters or fewer. You entered");
    }

    [Fact]
    public async Task CreatePerson_ShouldReturnBadRequest_WhenEmptyDataAreProvided()
    {
        //Arrange
        CreatePersonRequest request = new(
            Nickname: "",
            Email: "",
            PhoneNumber: "",
            Password: "",
            DefaultAdvertisementPickupAddressCountry: "",
            DefaultAdvertisementPickupAddressState: "",
            DefaultAdvertisementPickupAddressZipCode: "",
            DefaultAdvertisementPickupAddressCity: "",
            DefaultAdvertisementPickupAddressStreet: "",
            DefaultAdvertisementPickupAddressBuildingNumber: "",
            DefaultAdvertisementContactInfoEmail: "",
            DefaultAdvertisementContactInfoPhoneNumber: ""
        );

        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", request);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(9);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo(
            nameof(CreatePersonRequest.Nickname),
            nameof(CreatePersonRequest.Email),
            nameof(CreatePersonRequest.PhoneNumber),
            nameof(CreatePersonRequest.DefaultAdvertisementContactInfoEmail),
            nameof(CreatePersonRequest.DefaultAdvertisementContactInfoPhoneNumber),
            nameof(CreatePersonRequest.Password),
            nameof(CreatePersonRequest.DefaultAdvertisementPickupAddressCountry),
            nameof(CreatePersonRequest.DefaultAdvertisementPickupAddressZipCode),
            nameof(CreatePersonRequest.DefaultAdvertisementPickupAddressCity)
        );
        validationProblemDetails.Errors.Values.Count.Should().Be(9);

        validationProblemDetails.Errors[nameof(CreatePersonRequest.Nickname)][0]
            .Should()
            .Be("'Nickname' must not be empty.");

        validationProblemDetails.Errors[nameof(CreatePersonRequest.Email)][0]
            .Should()
            .Be("'Email' must not be empty.");

        validationProblemDetails.Errors[nameof(CreatePersonRequest.PhoneNumber)][0]
            .Should()
            .Be("'Phone Number' must not be empty.");

        validationProblemDetails.Errors[nameof(CreatePersonRequest.DefaultAdvertisementContactInfoEmail)]
            [0]
            .Should()
            .Be("'Default Advertisement Contact Info Email' must not be empty.");

        validationProblemDetails.Errors[
                nameof(CreatePersonRequest.DefaultAdvertisementContactInfoPhoneNumber)][0]
            .Should()
            .Be("'Default Advertisement Contact Info Phone Number' must not be empty.");

        validationProblemDetails.Errors[nameof(CreatePersonRequest.Password)][0]
            .Should()
            .Be("'Password' must not be empty.");

        validationProblemDetails.Errors[
                nameof(CreatePersonRequest.DefaultAdvertisementPickupAddressCountry)][0]
            .Should()
            .Be("'Default Advertisement Pickup Address Country' must not be empty.");

        validationProblemDetails.Errors[
                nameof(CreatePersonRequest.DefaultAdvertisementPickupAddressZipCode)][0]
            .Should()
            .Be("'Default Advertisement Pickup Address Zip Code' must not be empty.");

        validationProblemDetails.Errors[nameof(CreatePersonRequest.DefaultAdvertisementPickupAddressCity)]
            [0]
            .Should()
            .Be("'Default Advertisement Pickup Address City' must not be empty.");
    }

    [Theory]
    [ClassData(typeof(InvalidEmailData))]
    public async Task CreatePerson_ShouldReturnBadRequest_WhenInvalidEmailIsProvided(string email)
    {
        //Arrange
        CreatePersonRequest request = new Faker<CreatePersonRequest>()
            .CustomInstantiator(faker =>
                new CreatePersonRequest(
                    Nickname: faker.Person.FirstName,
                    Email: email,
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

        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", request);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(1);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo(
            nameof(CreatePersonRequest.Email)
        );
        validationProblemDetails.Errors.Values.Count.Should().Be(1);
        validationProblemDetails.Errors[nameof(CreatePersonRequest.Email)][0]
            .Should().StartWith("'Email' is not in the correct format.");
    }

    [Fact]
    public async Task CreatePerson_ShouldReturnBadRequest_WhenAlreadyTakenUniquePropertiesAreProvided()
    {
        //Arrange
        CreatePersonRequest request = _createPersonRequestGenerator.Generate();
        _ = await _httpClient.PostAsJsonAsync("api/v1/persons", request);

        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", request);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(2);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo(
            nameof(CreatePersonRequest.Email),
            nameof(CreatePersonRequest.PhoneNumber)
        );
        validationProblemDetails.Errors.Values.Count.Should().Be(2);

        validationProblemDetails.Errors[nameof(CreatePersonRequest.Email)][0]
            .Should()
            .Be("'Email' is already used by another user.");

        validationProblemDetails.Errors[nameof(CreatePersonRequest.PhoneNumber)][0]
            .Should()
            .Be("'Phone Number' is already used by another user.");

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
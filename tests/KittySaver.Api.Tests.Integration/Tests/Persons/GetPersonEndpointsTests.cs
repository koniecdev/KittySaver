using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Features.Persons;
using KittySaver.Api.Features.Persons.SharedContracts;
using KittySaver.Api.Tests.Integration.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace KittySaver.Api.Tests.Integration.Tests.Persons;

[Collection("Api")]
public class GetPersonEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CleanupHelper _cleanup;

    public GetPersonEndpointsTests(KittySaverApiFactory appFactory)
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
                    UserIdentityId: Guid.NewGuid(),
                    DefaultAdvertisementPickupAddressCountry: faker.Address.Country(),
                    DefaultAdvertisementPickupAddressState: faker.Address.State(),
                    DefaultAdvertisementPickupAddressZipCode: faker.Address.ZipCode(),
                    DefaultAdvertisementPickupAddressCity: faker.Address.City(),
                    DefaultAdvertisementPickupAddressStreet: faker.Address.StreetName(),
                    DefaultAdvertisementPickupAddressBuildingNumber: faker.Address.BuildingNumber(),
                    DefaultAdvertisementContactInfoEmail: faker.Person.Email,
                    DefaultAdvertisementContactInfoPhoneNumber: faker.Person.Phone
                ));

    [Fact]
    public async Task GetPerson_ShouldReturnPerson_WhenPersonExist()
    {
        //Arrange
        CreatePerson.CreatePersonRequest request = _createPersonRequestGenerator.Generate();
        HttpResponseMessage registerResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/persons", request);
        ApiResponses.CreatedWithIdResponse registerResponse =
            await registerResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        //Act
        HttpResponseMessage response = await _httpClient.GetAsync($"api/v1/persons/{registerResponse.Id}");
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PersonResponse person = await response.Content.ReadFromJsonAsync<PersonResponse>() ?? throw new JsonException();
        person.Id.Should().Be(registerResponse.Id);
        person.Email.Should().Be(request.Email);
        person.PhoneNumber.Should().Be(request.PhoneNumber);
    }

    [Fact]
    public async Task GetPerson_ShouldReturnNotFound_WhenNoPersonExist()
    {
        //Act
        HttpResponseMessage response = await _httpClient.GetAsync($"api/v1/persons/{Guid.NewGuid()}");
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails? problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
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
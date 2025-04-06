using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Tests.Integration.Helpers;
using KittySaver.Shared.Hateoas;
using KittySaver.Shared.Responses;
using KittySaver.Shared.TypedIds;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using KittySaver.Tests.Shared;

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
    public async Task GetPerson_ShouldReturnPerson_WhenPersonExist()
    {
        //Arrange
        CreatePersonRequest request = _createPersonRequestGenerator.Generate();
        HttpResponseMessage registerResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/persons", request);
        IdResponse<PersonId> personId = await registerResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();

        
        //Act
        HttpResponseMessage response = await _httpClient.GetAsync($"api/v1/persons/{personId}");
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PersonResponse person = await response.Content.ReadFromJsonAsync<PersonResponse>() ?? throw new JsonException();
        person.Id.Should().Be(personId.Id);
        person.Email.Should().Be(request.Email);
        person.PhoneNumber.Should().Be(request.PhoneNumber);
        person.Links.Count.Should().Be(7);
    }

    [Fact]
    public async Task GetPerson_ShouldReturnNotFound_WhenNoPersonExist()
    {
        //Arrange
        PersonId randomId = PersonId.New();
        
        //Act
        HttpResponseMessage response = await _httpClient.GetAsync($"api/v1/persons/{randomId}");
        
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
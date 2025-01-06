using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Features.Persons;
using KittySaver.Api.Features.Persons.SharedContracts;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Contracts;
using KittySaver.Api.Tests.Integration.Helpers;

namespace KittySaver.Api.Tests.Integration.Tests.Persons;

[Collection("Api")]
public class GetPersonsEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CleanupHelper _cleanup;

    public GetPersonsEndpointsTests(KittySaverApiFactory appFactory)
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
    public async Task GetPersons_ShouldReturnUser_WhenUserExist()
    {
        //Arrange
        CreatePerson.CreatePersonRequest createRequest = _createPersonRequestGenerator.Generate();
        await _httpClient.PostAsJsonAsync("api/v1/persons", createRequest);
        //Act
        HttpResponseMessage response = await _httpClient.GetAsync("/api/v1/persons");
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PagedList<PersonResponse>? persons = await response.Content.ReadFromJsonAsync<PagedList<PersonResponse>>();
        persons.Should().NotBeNull();
        persons!.Items.Count.Should().BeGreaterThan(0);
        persons.Total.Should().Be(1);
        persons.Links.Count.Should().Be(2);
        persons.Links.First(x => x.Rel == EndpointNames.SelfRel).Href.Should().Contain("://");
        persons.Links.First(x => x.Rel == "by-page").Href.Should().Contain("://");
        PersonResponse registeredPerson = persons.Items.First();
        registeredPerson.Id.Should().NotBeEmpty();
        registeredPerson.Nickname.Should().Be(createRequest.Nickname);
        registeredPerson.Email.Should().Be(createRequest.Email);
        registeredPerson.PhoneNumber.Should().Be(createRequest.PhoneNumber);
        registeredPerson.Links.Count.Should().Be(7);
    }

    [Fact]
    public async Task GetPersons_ShouldReturnEmptyList_WhenNoUsersExist()
    {
        //Act
        HttpResponseMessage response = await _httpClient.GetAsync("/api/v1/persons");
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PagedList<PersonResponse>? persons = await response.Content.ReadFromJsonAsync<PagedList<PersonResponse>>();
        persons?.Items.Count.Should().Be(0);
        persons!.Links.Count.Should().Be(2);
        persons.Links.First(x => x.Rel == EndpointNames.SelfRel).Href.Should().Contain("://");
        persons.Links.First(x => x.Rel == "by-page").Href.Should().Contain("://");
        persons.Total.Should().Be(0);
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
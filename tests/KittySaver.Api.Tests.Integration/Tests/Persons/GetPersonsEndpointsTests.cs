using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Features.Persons;
using KittySaver.Api.Features.Persons.SharedContracts;
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
            .CustomInstantiator( faker =>
                new CreatePerson.CreatePersonRequest(
                    FirstName: faker.Person.FirstName,
                    LastName: faker.Person.LastName,
                    Email: faker.Person.Email,
                    PhoneNumber: faker.Person.Phone,
                    UserIdentityId: Guid.NewGuid(),
                    AddressCountry: faker.Address.Country(),
                    AddressZipCode: faker.Address.ZipCode(),
                    AddressCity: faker.Address.City(),
                    AddressStreet: faker.Address.StreetName(),
                    AddressBuildingNumber: faker.Address.BuildingNumber(),
                    AddressState: faker.Address.State(),
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
    public async Task GetPersons_ShouldReturnUser_WhenUserExist()
    {
        //Arrange
        CreatePerson.CreatePersonRequest createRequest = _createPersonRequestGenerator.Generate();
        await _httpClient.PostAsJsonAsync("api/v1/persons", createRequest);
        //Act
        HttpResponseMessage response = await _httpClient.GetAsync("/api/v1/persons");
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ICollection<PersonResponse>? persons = await response.Content.ReadFromJsonAsync<ICollection<PersonResponse>>();
        persons.Should().NotBeNull();
        persons!.Count.Should().BeGreaterThan(0);
        PersonResponse registeredPerson = persons.First();
        registeredPerson.Id.Should().NotBeEmpty();
        registeredPerson.FirstName.Should().Be(createRequest.FirstName);
        registeredPerson.LastName.Should().Be(createRequest.LastName);
        registeredPerson.FullName.Should().Be($"{createRequest.FirstName} {createRequest.LastName}");
        registeredPerson.Email.Should().Be(createRequest.Email);
        registeredPerson.PhoneNumber.Should().Be(createRequest.PhoneNumber);
        registeredPerson.ResidentalAddress.Country.Should().Be(createRequest.AddressCountry);
        registeredPerson.ResidentalAddress.State.Should().Be(createRequest.AddressState);
        registeredPerson.ResidentalAddress.ZipCode.Should().Be(createRequest.AddressZipCode);
        registeredPerson.ResidentalAddress.City.Should().Be(createRequest.AddressCity);
        registeredPerson.ResidentalAddress.Street.Should().Be(createRequest.AddressStreet);
        registeredPerson.ResidentalAddress.BuildingNumber.Should().Be(createRequest.AddressBuildingNumber);
    }
    
    [Fact]
    public async Task GetPersons_ShouldReturnEmptyList_WhenNoUsersExist()
    {
        //Arrange
        //Act
        HttpResponseMessage response = await _httpClient.GetAsync("/api/v1/persons");
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ICollection<PersonResponse>? persons = await response.Content.ReadFromJsonAsync<ICollection<PersonResponse>>();
        persons?.Count.Should().Be(0);
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

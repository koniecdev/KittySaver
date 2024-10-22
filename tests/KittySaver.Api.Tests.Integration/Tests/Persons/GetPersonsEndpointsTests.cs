using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Features.Persons;
using KittySaver.Api.Features.Persons.SharedContracts;

namespace KittySaver.Api.Tests.Integration.Persons;

[Collection("Api")]
public class GetPersonsEndpointsTests(KittySaverApiFactory appFactory)
{
    private readonly HttpClient _httpClient = appFactory.CreateClient();

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
                    AddressState: faker.Address.State()
                ));
    [Fact]
    public async Task GetPersons_ShouldReturnAtLeastDefaultAdmin_WhenEndpointIsCalled()
    {
        //Arrange
        CreatePerson.CreatePersonRequest createRequest = _createPersonRequestGenerator.Generate();
        await _httpClient.PostAsJsonAsync("api/v1/persons", createRequest);
        //Act
        HttpResponseMessage response = await _httpClient.GetAsync($"/api/v1/persons");
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ICollection<PersonResponse>? persons = await response.Content.ReadFromJsonAsync<ICollection<PersonResponse>>();
        persons?.Count.Should().BeGreaterThan(0);
    }
}

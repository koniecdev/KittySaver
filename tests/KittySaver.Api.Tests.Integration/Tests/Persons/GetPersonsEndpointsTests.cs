using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Tests.Integration.Helpers;
using KittySaver.Shared.Hateoas;
using KittySaver.Shared.Pagination;
using KittySaver.Shared.Responses;

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
    public async Task GetPersons_ShouldReturnUser_WhenUserExist()
    {
        //Arrange
        CreatePersonRequest createRequest = _createPersonRequestGenerator.Generate();
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
        persons.Links.First(x => x.Rel == EndpointRels.SelfRel).Href.Should().Contain("://");
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
        persons.Links.First(x => x.Rel == EndpointRels.SelfRel).Href.Should().Contain("://");
        persons.Links.First(x => x.Rel == "by-page").Href.Should().Contain("://");
        persons.Total.Should().Be(0);
    }

    [Fact]
    public async Task GetPersons_ShouldReturnSingleUser_WhenFiltersAreApplied()
    {
        //Arrange
        // Person 1: Primary resident in Houston with California address
        CreatePersonRequest createRequest1 = new(
            Nickname: "Sophia",
            Email: "sophia80@yahoo.com",
            PhoneNumber: "+12774688688",
            Password: "Default123$",
            DefaultAdvertisementPickupAddressCountry: "US",
            DefaultAdvertisementPickupAddressState: "CA",
            DefaultAdvertisementPickupAddressZipCode: "94225",
            DefaultAdvertisementPickupAddressCity: "Houston",
            DefaultAdvertisementPickupAddressStreet: "Oak Street",
            DefaultAdvertisementPickupAddressBuildingNumber: "23",
            DefaultAdvertisementContactInfoEmail: "sophia80@yahoo.com",
            DefaultAdvertisementContactInfoPhoneNumber: "+12774688688"
        );

        // Person 2: Miami resident with California mailing address
        CreatePersonRequest createRequest2 = new(
            Nickname: "Noah",
            Email: "noah852@gmail.com",
            PhoneNumber: "+19486935859",
            Password: "Default123$",
            DefaultAdvertisementPickupAddressCountry: "US",
            DefaultAdvertisementPickupAddressState: "CA",
            DefaultAdvertisementPickupAddressZipCode: "66613",
            DefaultAdvertisementPickupAddressCity: "Miami",
            DefaultAdvertisementPickupAddressStreet: "Elm Street",
            DefaultAdvertisementPickupAddressBuildingNumber: "504",
            DefaultAdvertisementContactInfoEmail: "noah852@outlook.com",
            DefaultAdvertisementContactInfoPhoneNumber: "+19486935859"
        );

        // Person 3: Houston resident with New York address
        CreatePersonRequest createRequest3 = new(
            Nickname: "Emma",
            Email: "emma413@gmail.com",
            PhoneNumber: "+11869419170",
            Password: "Default123$",
            DefaultAdvertisementPickupAddressCountry: "US",
            DefaultAdvertisementPickupAddressState: "NY",
            DefaultAdvertisementPickupAddressZipCode: "34882",
            DefaultAdvertisementPickupAddressCity: "Houston",
            DefaultAdvertisementPickupAddressStreet: "Elm Street",
            DefaultAdvertisementPickupAddressBuildingNumber: "597",
            DefaultAdvertisementContactInfoEmail: "emma413@hotmail.com",
            DefaultAdvertisementContactInfoPhoneNumber: "+11869419170"
        );

        // Person 4: Miami resident with California address
        CreatePersonRequest createRequest4 = new(
            Nickname: "Noah",
            Email: "noah8@gmail.com",
            PhoneNumber: "+18326616873",
            Password: "Default123$",
            DefaultAdvertisementPickupAddressCountry: "US",
            DefaultAdvertisementPickupAddressState: "CA",
            DefaultAdvertisementPickupAddressZipCode: "79122",
            DefaultAdvertisementPickupAddressCity: "Miami",
            DefaultAdvertisementPickupAddressStreet: "Pine Street",
            DefaultAdvertisementPickupAddressBuildingNumber: "532",
            DefaultAdvertisementContactInfoEmail: "noah8@gmail.com",
            DefaultAdvertisementContactInfoPhoneNumber: "+18326616873"
        );
        await _httpClient.PostAsJsonAsync("api/v1/persons", createRequest1);
        await _httpClient.PostAsJsonAsync("api/v1/persons", createRequest2);
        await _httpClient.PostAsJsonAsync("api/v1/persons", createRequest3);
        await _httpClient.PostAsJsonAsync("api/v1/persons", createRequest4);
        
        //Act
        HttpResponseMessage response = 
            await _httpClient.GetAsync("/api/v1/persons?searchTerm=nickname-eq-noah,email-in-@gmail,phonenumber-in-1,currentrole-lt-1");
        //Assert
        PagedList<PersonResponse>? persons = await response.Content.ReadFromJsonAsync<PagedList<PersonResponse>>();
        persons?.Items.Count.Should().Be(2);
        persons?.Total.Should().Be(4);
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
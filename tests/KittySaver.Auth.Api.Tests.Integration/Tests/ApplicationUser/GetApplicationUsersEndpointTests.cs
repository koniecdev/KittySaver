using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Features.Persons.SharedContracts;
using KittySaver.Auth.Api.Features.ApplicationUsers;
using Newtonsoft.Json;
using Shared;

namespace KittySaver.Auth.Api.Tests.Integration.Tests.ApplicationUser;

[Collection("AuthApi")]
public class GetApplicationUsersEndpointTests(KittySaverAuthApiFactory appFactory)
{
    private readonly HttpClient _httpClient = appFactory.CreateClient();
    private readonly Faker<Register.RegisterRequest> _createApplicationUserRequestGenerator =
        new Faker<Register.RegisterRequest>()
            .CustomInstantiator( faker =>
                new Register.RegisterRequest(
                    FirstName: faker.Person.FirstName,
                    LastName: faker.Person.LastName,
                    Email: faker.Person.Email,
                    PhoneNumber: faker.Person.Phone,
                    Password: "Default1234%"
                ));
    [Fact]
    public async Task GetApplicationUsers_ShouldReturnApplicationUsers_WhenUsersExists()
    {
        //Act
        Register.RegisterRequest registerRequest = _createApplicationUserRequestGenerator.Generate();
        HttpResponseMessage registerResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/application-users/register",
            registerRequest);
        ApiResponses.CreatedWithIdResponse registerResponse = 
            await registerResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>() ?? throw new JsonException();

        //Act
        HttpResponseMessage response = await _httpClient.GetAsync($"/api/v1/application-users");
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ICollection<PersonResponse>? persons = await response.Content.ReadFromJsonAsync<ICollection<PersonResponse>>();
        persons.Should().NotBeNull();
        const int defaultAdminPlusRecentlyRegisteredPersonCount = 2;
        persons!.Count.Should().BeGreaterOrEqualTo(defaultAdminPlusRecentlyRegisteredPersonCount);
        PersonResponse person = persons.First(x => x.Id == registerResponse.Id);
        person.FirstName.Should().Be(registerRequest.FirstName);
        person.LastName.Should().Be(registerRequest.LastName);
        person.Email.Should().Be(registerRequest.Email);
        person.PhoneNumber.Should().Be(registerRequest.PhoneNumber);
        person.FullName.Should().Be($"{registerRequest.FirstName} {registerRequest.LastName}");
    }
}

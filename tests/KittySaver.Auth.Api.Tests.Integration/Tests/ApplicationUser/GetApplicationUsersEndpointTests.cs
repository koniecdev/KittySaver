using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using KittySaver.Api.Features.Persons.SharedContracts;

namespace KittySaver.Auth.Api.Tests.Integration.Tests.ApplicationUser;

[Collection("AuthApi")]
public class GetApplicationUsersEndpointTests(KittySaverAuthApiFactory appFactory)
{
    private readonly HttpClient _httpClient = appFactory.CreateClient();

    [Fact]
    public async Task GetApplicationUsers_ShouldReturnAtLeastDefaultAdmin_WhenEndpointIsCalled()
    {
        //Act
        HttpResponseMessage response = await _httpClient.GetAsync($"/api/v1/application-users");
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ICollection<PersonResponse>? persons = await response.Content.ReadFromJsonAsync<ICollection<PersonResponse>>();
        persons?.Count.Should().BeGreaterThan(0);
    }
}

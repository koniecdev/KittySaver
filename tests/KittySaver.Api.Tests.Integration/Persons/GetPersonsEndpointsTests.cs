using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Features.Persons;
using KittySaver.Api.Features.Persons.Contracts;

namespace KittySaver.Api.Tests.Integration.Persons;

[Collection("Api")]
public class GetPersonsEndpointsTests(KittySaverApiFactory appFactory)
{
    private readonly HttpClient _httpClient = appFactory.CreateClient();

    [Fact]
    public async Task GetPersons_ShouldReturnAtLeastDefaultAdmin_WhenEndpointIsCalled()
    {
        //Act
        HttpResponseMessage response = await _httpClient.GetAsync($"/api/v1/persons");
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ICollection<PersonResponse>? persons = await response.Content.ReadFromJsonAsync<ICollection<PersonResponse>>();
        persons?.Count.Should().BeGreaterThan(0);
    }
}

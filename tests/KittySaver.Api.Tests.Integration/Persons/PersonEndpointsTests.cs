using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Features.Persons;
using KittySaver.Api.Features.Persons.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace KittySaver.Api.Tests.Integration;

[Collection("Api")]
public class PersonEndpointsTests(KittySaverApiFactory appFactory)
{
    private readonly HttpClient _httpClient = appFactory.CreateClient();

    private readonly Faker<CreatePerson.CreatePersonRequest> _createPersonRequestGenerator =
        new Faker<CreatePerson.CreatePersonRequest>()
            .RuleFor(x => x.FirstName, f => f.Person.FirstName)
            .RuleFor(x => x.LastName, f => f.Person.LastName)
            .RuleFor(x => x.Email, f => f.Person.Email)
            .RuleFor(x => x.PhoneNumber, f => f.Person.Phone)
            .RuleFor(x => x.UserIdentityId, Guid.NewGuid());

    [Fact]
    public async Task GetPersons_ShouldReturnAtLeastDefaultAdmin_WhenEndpointIsCalled()
    {
        //Act
        HttpResponseMessage response = await _httpClient.GetAsync($"/api/v1/persons");
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ICollection<PersonResponse>? persons = await response.Content.ReadFromJsonAsync<ICollection<PersonResponse>>();
        persons!.Count.Should().BeGreaterThan(0);
    }
}

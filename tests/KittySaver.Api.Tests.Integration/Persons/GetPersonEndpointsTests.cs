using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using KittySaver.Api.Features.Persons.SharedContracts;
using KittySaver.Api.Shared.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KittySaver.Api.Tests.Integration.Persons;

[Collection("Api")]
public class GetPersonEndpointsTests(KittySaverApiFactory appFactory)
{
    private readonly HttpClient _httpClient = appFactory.CreateClient();

    [Fact]
    public async Task GetPerson_ShouldReturnDefaultAdmin_WhenEndpointIsCalled()
    {
        //Act
        HttpResponseMessage response = await _httpClient.GetAsync($"api/v1/persons/{FixedIdsHelper.AdminId}");
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PersonResponse? person = await response.Content.ReadFromJsonAsync<PersonResponse>();
        person?.Id.Should().Be(FixedIdsHelper.AdminId);
        person?.Email.Should().Be(FixedIdsHelper.AdminEmail);
        person?.FullName.Should().Be("Default Admin");
        person?.PhoneNumber.Should().Be(FixedIdsHelper.AdminPhone);
    }
    
    [Fact]
    public async Task GetPerson_ShouldReturnNotFound_WhenEndpointIsCalledWithNonExistingId()
    {
        //Act
        HttpResponseMessage response = await _httpClient.GetAsync($"api/v1/persons/{Guid.NewGuid()}");
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails? problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails?.Status.Should().Be(StatusCodes.Status404NotFound);
    }
}

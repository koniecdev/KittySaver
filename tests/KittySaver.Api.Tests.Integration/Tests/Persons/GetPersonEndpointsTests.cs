using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Features.Persons;
using KittySaver.Api.Features.Persons.SharedContracts;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Auth.Api.Shared.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace KittySaver.Api.Tests.Integration.Persons;

[Collection("Api")]
public class GetPersonEndpointsTests(KittySaverApiFactory appFactory)
{
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
    private readonly HttpClient _httpClient = appFactory.CreateClient();

    [Fact]
    public async Task GetPerson_ShouldReturnDefaultAdmin_WhenEndpointIsCalled()
    {
        //Arrange
        CreatePerson.CreatePersonRequest request = _createPersonRequestGenerator.Generate();
        HttpResponseMessage registerResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/persons", request);
        ApiResponses.CreatedWithIdResponse registerResponse =
            await registerResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>() ?? throw new JsonException();
        //Act
        HttpResponseMessage response = await _httpClient.GetAsync($"api/v1/persons/{registerResponse.Id}");
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PersonResponse person = await response.Content.ReadFromJsonAsync<PersonResponse>() ?? throw new JsonException();
        person.Id.Should().Be(registerResponse.Id);
        person.Email.Should().Be(request.Email);
        person.FullName.Should().Be($"{request.FirstName} {request.LastName}");
        person.PhoneNumber.Should().Be(request.PhoneNumber);
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

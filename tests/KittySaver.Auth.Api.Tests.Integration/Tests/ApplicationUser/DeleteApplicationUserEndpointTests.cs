using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Shared.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using KittySaver.Tests.Shared;

namespace KittySaver.Auth.Api.Tests.Integration.Tests.ApplicationUser;

[Collection("AuthApi")]
public class DeleteApplicationUserEndpointTests(KittySaverAuthApiFactory appFactory)
{
    private readonly HttpClient _httpClient = appFactory.CreateClient();

    private readonly Faker<RegisterRequest> _createApplicationUserRequestGenerator =
        new Faker<RegisterRequest>()
            .CustomInstantiator( faker =>
                new RegisterRequest(
                    UserName: faker.Person.FirstName,
                    Email: faker.Person.Email,
                    PhoneNumber: faker.Person.Phone,
                    Password: "Default1234%"
                ));
    
    [Fact]
    public async Task Delete_ShouldDeleteUser_WhenItExists()
    {
        //Arrange
        RegisterRequest request = _createApplicationUserRequestGenerator.Generate();
        HttpResponseMessage userResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/application-users", request);
        ApiResponses.CreatedWithIdResponse? registerResponse = 
            await userResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>();

        //Act
        HttpResponseMessage deleteUserResponseMessage = await _httpClient.DeleteAsync($"api/v1/application-users/{registerResponse?.Id}");

        //Assert
        deleteUserResponseMessage.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
    
    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenUserWithGivenIdDoNotExist()
    {
        //Arrange
        Guid randomId = Guid.NewGuid();
        //Act
        HttpResponseMessage deleteUserResponseMessage = await _httpClient.DeleteAsync($"api/v1/application-users/{randomId}");

        //Assert
        deleteUserResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails? problemDetails = await deleteUserResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
    }
    
    [Fact]
    public async Task Delete_ShouldReturnBadRequest_WhenUserWithGivenIdIsDefaultForType()
    {
        //Arrange
        Guid emptyId = Guid.Empty;
        //Act
        HttpResponseMessage deleteUserResponseMessage = await _httpClient.DeleteAsync($"api/v1/application-users/{emptyId}");

        //Assert
        deleteUserResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails = await deleteUserResponseMessage.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors["Id"][0].Should().Be("'Id' must not be empty.");
    }
}
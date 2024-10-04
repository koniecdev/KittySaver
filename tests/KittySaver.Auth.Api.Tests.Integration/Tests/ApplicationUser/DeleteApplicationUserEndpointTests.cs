using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Auth.Api.Features.ApplicationUsers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace KittySaver.Auth.Api.Tests.Integration.Tests.ApplicationUser;

[Collection("AuthApi")]
public class DeleteEndpointTests(KittySaverAuthApiFactory appFactory)
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
    public async Task Delete_ShouldDeleteUser_WhenItExists()
    {
        //Arrange
        Register.RegisterRequest request = _createApplicationUserRequestGenerator.Generate();
        HttpResponseMessage userResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/application-users/register", request);
        ApiResponses.CreatedWithIdResponse? registerResponse = 
            await userResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>();

        //Act
        HttpResponseMessage deleteUserResponseMessage = await _httpClient.DeleteAsync($"api/v1/application-users/{registerResponse?.Id}");

        //Assert
        deleteUserResponseMessage.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        HttpResponseMessage getDeletedUser = await _httpClient.GetAsync($"api/v1/application-users/{registerResponse?.Id}");
        getDeletedUser.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
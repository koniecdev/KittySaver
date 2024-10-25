using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Auth.Api.Features.ApplicationUsers;
using KittySaver.Auth.Api.Features.ApplicationUsers.SharedContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace KittySaver.Auth.Api.Tests.Integration.Tests.ApplicationUser;

[Collection("AuthApi")]
public class GetApplicationUserEndpointTests(KittySaverAuthApiFactory appFactory)
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
    public async Task GetApplicationUser_ShouldReturnApplicationUser_WhenExistingUserIsIssued()
    {
        //Act
        Register.RegisterRequest registerRequest = _createApplicationUserRequestGenerator.Generate();
        HttpResponseMessage registerResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/application-users/register",
            registerRequest);
        ApiResponses.CreatedWithIdResponse registerResponse = 
            await registerResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>() ?? throw new JsonException();

        //Act
        HttpResponseMessage response = await _httpClient.GetAsync($"/api/v1/application-users/{registerResponse.Id}");
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ApplicationUserResponse? applicationUser = await response.Content.ReadFromJsonAsync<ApplicationUserResponse>();
        applicationUser.Should().NotBeNull();
        applicationUser!.Id.Should().Be(registerResponse.Id);
        applicationUser.FirstName.Should().Be(registerRequest.FirstName);
        applicationUser.LastName.Should().Be(registerRequest.LastName);
        applicationUser.Email.Should().Be(registerRequest.Email);
        applicationUser.PhoneNumber.Should().Be(registerRequest.PhoneNumber);
        applicationUser.FullName.Should().Be($"{registerRequest.FirstName} {registerRequest.LastName}");
    }
    
    [Fact]
    public async Task GetApplicationUser_ShouldReturnNotFound_WhenUserWithGivenIdDoNotExist()
    {
        //Arrange
        Guid randomId = Guid.NewGuid();

        //Act
        HttpResponseMessage response = await _httpClient.GetAsync($"/api/v1/application-users/{randomId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails? problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
    }
    
}

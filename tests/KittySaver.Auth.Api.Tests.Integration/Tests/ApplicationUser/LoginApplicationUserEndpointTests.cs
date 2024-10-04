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
public class LoginApplicationUserEndpointTests(KittySaverAuthApiFactory appFactory)
{
    private readonly HttpClient _httpClient = appFactory.CreateClient();

    private readonly Faker<Login.LoginRequest> _loginRequestGenerator =
        new Faker<Login.LoginRequest>()
            .CustomInstantiator( faker =>
                new Login.LoginRequest(
                    Email: faker.Person.Email,
                    Password: "Default1234%"
                ));

    [Fact]
    public async Task Login_ShouldLoginSuccessfully_WhenValidDataProvided()
    {
        //Arrange
        Login.LoginRequest request = new Login.LoginRequest(
            Email: KittySaverAuthApiFactory.DefaultAdminEmail,
            Password: "DefaultPassword123!");
        
        //Act
        HttpResponseMessage responseMessage = await _httpClient.PostAsJsonAsync("api/v1/application-users/login", request);
        
        //Assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        Login.LoginResponse? response = await responseMessage.Content.ReadFromJsonAsync<Login.LoginResponse>();
        response?.AccessToken.Should().NotBeEmpty();
        response?.AccessTokenExpiresAt
            .Should().Be(KittySaverAuthApiFactory.FixedDateTime.AddMinutes(KittySaverAuthApiFactory.FixedMinutesJwtExpire));
    }
    
    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenInvalidPasswordProvided()
    {
        //Arrange
        Login.LoginRequest request = new Login.LoginRequest(
            Email: KittySaverAuthApiFactory.DefaultAdminEmail,
            Password: "DefaultPassword123!%");
        
        //Act
        HttpResponseMessage responseMessage = await _httpClient.PostAsJsonAsync("api/v1/application-users/login", request);
        
        //Assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task Login_ShouldReturnApplicationUserNotFoundException_WhenNotRegisteredMailProvided()
    {
        //Arrange
        Login.LoginRequest request = new Login.LoginRequest(
            Email: "default@proper.mail",
            Password: "DefaultPassword123!%");
        
        //Act
        HttpResponseMessage responseMessage = await _httpClient.PostAsJsonAsync("api/v1/application-users/login", request);
        
        //Assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails? problemDetails = await responseMessage.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
    }
    
    [Fact]
    public async Task Login_ShouldReturnValidationProblemDetails_WhenEmptyDataIsProvided()
    {
        //Arrange
        Login.LoginRequest request = new Login.LoginRequest(
            Email: "",
            Password: "");
        
        //Act
        HttpResponseMessage responseMessage = await _httpClient.PostAsJsonAsync("api/v1/application-users/login", request);
        
        //Assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails = await responseMessage.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails?.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails?.Errors["Email"][0].Should().Be("'Email' must not be empty.");
        validationProblemDetails?.Errors["Password"][0].Should().Be("'Password' must not be empty.");
    }
    
    [Theory]
    [ClassData(typeof(InvalidEmailData))]
    public async Task Login_ShouldReturnValidationProblemDetails_WhenInvalidEmailIsProvided(string email)
    {
        //Arrange
        Login.LoginRequest request = new Login.LoginRequest(
            Email: email,
            Password: "Default123!");
        
        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/application-users/register", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails?.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails?.Errors["Email"][0].Should().Be("'Email' is not in the correct format.");
    }
    
}


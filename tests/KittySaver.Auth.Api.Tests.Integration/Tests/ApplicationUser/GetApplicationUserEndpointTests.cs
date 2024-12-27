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
                    UserName: faker.Person.FirstName,
                    Email: faker.Person.Email,
                    PhoneNumber: faker.Person.Phone,
                    Password: "Default1234%",
                    DefaultAdvertisementPickupAddressCountry: faker.Address.CountryCode(),
                    DefaultAdvertisementPickupAddressState: faker.Address.State(),
                    DefaultAdvertisementPickupAddressZipCode: faker.Address.ZipCode(),
                    DefaultAdvertisementPickupAddressCity: faker.Address.City(),
                    DefaultAdvertisementPickupAddressStreet: faker.Address.StreetName(),
                    DefaultAdvertisementPickupAddressBuildingNumber: faker.Address.BuildingNumber(),
                    DefaultAdvertisementContactInfoEmail: faker.Person.Email,
                    DefaultAdvertisementContactInfoPhoneNumber: faker.Person.Phone
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
        applicationUser.UserName.Should().Be(registerRequest.UserName);
        applicationUser.Email.Should().Be(registerRequest.Email);
        applicationUser.PhoneNumber.Should().Be(registerRequest.PhoneNumber);
        applicationUser.DefaultAdvertisementPickupAddressCountry.Should().Be(registerRequest.DefaultAdvertisementPickupAddressCountry);
        applicationUser.DefaultAdvertisementPickupAddressState.Should().Be(registerRequest.DefaultAdvertisementPickupAddressState);
        applicationUser.DefaultAdvertisementPickupAddressZipCode.Should().Be(registerRequest.DefaultAdvertisementPickupAddressZipCode);
        applicationUser.DefaultAdvertisementPickupAddressCity.Should().Be(registerRequest.DefaultAdvertisementPickupAddressCity);
        applicationUser.DefaultAdvertisementPickupAddressStreet.Should().Be(registerRequest.DefaultAdvertisementPickupAddressStreet);
        applicationUser.DefaultAdvertisementPickupAddressBuildingNumber.Should().Be(registerRequest.DefaultAdvertisementPickupAddressBuildingNumber);
        applicationUser.DefaultAdvertisementContactInfoEmail.Should().Be(registerRequest.DefaultAdvertisementContactInfoEmail);
        applicationUser.DefaultAdvertisementContactInfoPhoneNumber.Should().Be(registerRequest.DefaultAdvertisementContactInfoPhoneNumber);
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

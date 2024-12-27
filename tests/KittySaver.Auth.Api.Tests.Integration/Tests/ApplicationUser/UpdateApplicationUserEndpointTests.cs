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
public class UpdateApplicationUserEndpointTests(KittySaverAuthApiFactory appFactory)
{
    private readonly HttpClient _httpClient = appFactory.CreateClient();

    private readonly Faker<Register.RegisterRequest> _createApplicationUserRequestGenerator =
        new Faker<Register.RegisterRequest>()
            .CustomInstantiator(faker =>
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
    public async Task UpdateApplicationUser_ShouldSuccessfullyUpdate_WhenValidDataIsProvided()
    {
        //Arrange
        Register.RegisterRequest registerRequest = _createApplicationUserRequestGenerator.Generate();
        HttpResponseMessage registerResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/application-users/register", registerRequest);
        ApiResponses.CreatedWithIdResponse registerResponse =
            await registerResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
        
        //Act
        UpdateApplicationUser.UpdateApplicationUserRequest request = new Faker<UpdateApplicationUser.UpdateApplicationUserRequest>()
            .CustomInstantiator(faker =>
                new UpdateApplicationUser.UpdateApplicationUserRequest(
                    UserName: faker.Person.FirstName,
                    DefaultAdvertisementPickupAddressCountry: faker.Address.CountryCode(),
                    DefaultAdvertisementPickupAddressState: faker.Address.State(),
                    DefaultAdvertisementPickupAddressZipCode: faker.Address.ZipCode(),
                    DefaultAdvertisementPickupAddressCity: faker.Address.City(),
                    DefaultAdvertisementPickupAddressStreet: faker.Address.StreetName(),
                    DefaultAdvertisementPickupAddressBuildingNumber: faker.Address.BuildingNumber(),
                    DefaultAdvertisementContactInfoEmail: faker.Person.Email,
                    DefaultAdvertisementContactInfoPhoneNumber: faker.Person.Phone
                ));
        HttpResponseMessage updateResponseMessage =
            await _httpClient.PutAsJsonAsync($"api/v1/application-users/{registerResponse.Id}", request);

        //Assert
        updateResponseMessage.StatusCode.Should().Be(HttpStatusCode.NoContent);
        ApplicationUserResponse? applicationUserAfterUpdate =
            await _httpClient.GetFromJsonAsync<ApplicationUserResponse>($"api/v1/application-users/{registerResponse.Id}");
        applicationUserAfterUpdate.Should().NotBeNull();
        applicationUserAfterUpdate!.UserName.Should().Be(applicationUserAfterUpdate.UserName);
        applicationUserAfterUpdate.DefaultAdvertisementPickupAddressCountry.Should().Be(applicationUserAfterUpdate.DefaultAdvertisementPickupAddressCountry);
        applicationUserAfterUpdate.DefaultAdvertisementPickupAddressState.Should().Be(applicationUserAfterUpdate.DefaultAdvertisementPickupAddressState);
        applicationUserAfterUpdate.DefaultAdvertisementPickupAddressZipCode.Should().Be(applicationUserAfterUpdate.DefaultAdvertisementPickupAddressZipCode);
        applicationUserAfterUpdate.DefaultAdvertisementPickupAddressCity.Should().Be(applicationUserAfterUpdate.DefaultAdvertisementPickupAddressCity);
        applicationUserAfterUpdate.DefaultAdvertisementPickupAddressStreet.Should().Be(applicationUserAfterUpdate.DefaultAdvertisementPickupAddressStreet);
        applicationUserAfterUpdate.DefaultAdvertisementPickupAddressBuildingNumber.Should().Be(applicationUserAfterUpdate.DefaultAdvertisementPickupAddressBuildingNumber);
        applicationUserAfterUpdate.DefaultAdvertisementContactInfoEmail.Should().Be(applicationUserAfterUpdate.DefaultAdvertisementContactInfoEmail);
        applicationUserAfterUpdate.DefaultAdvertisementContactInfoPhoneNumber.Should().Be(applicationUserAfterUpdate.DefaultAdvertisementContactInfoPhoneNumber);
    }
    
    [Fact]
    public async Task UpdateApplicationUser_ShouldReturnBadRequest_WhenEmptyDataIsProvided()
    {
        //Arrange
        Register.RegisterRequest registerRequest = _createApplicationUserRequestGenerator.Generate();
        HttpResponseMessage registerResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/application-users/register", registerRequest);
        ApiResponses.CreatedWithIdResponse registerResponse =
            await registerResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
            ?? throw new JsonException();
    
        //Act
        UpdateApplicationUser.UpdateApplicationUserRequest request = new(
            UserName: "",
            DefaultAdvertisementPickupAddressCountry: "",
            DefaultAdvertisementPickupAddressState: "",
            DefaultAdvertisementPickupAddressZipCode: "",
            DefaultAdvertisementPickupAddressCity: "",
            DefaultAdvertisementPickupAddressStreet: "",
            DefaultAdvertisementPickupAddressBuildingNumber: "",
            DefaultAdvertisementContactInfoEmail: "",
            DefaultAdvertisementContactInfoPhoneNumber: ""
        );
        HttpResponseMessage updateResponseMessage =
            await _httpClient.PutAsJsonAsync($"api/v1/application-users/{registerResponse.Id}", request);
    
        //Assert
        updateResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await updateResponseMessage.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors["UserName"][0].Should().Be("'User Name' must not be empty.");
        
        validationProblemDetails.Errors[
                nameof(UpdateApplicationUser.UpdateApplicationUserRequest.UserName)][0]
            .Should()
            .Be("'User Name' must not be empty.");
        validationProblemDetails.Errors[
                nameof(UpdateApplicationUser.UpdateApplicationUserRequest.DefaultAdvertisementPickupAddressCountry)][0]
            .Should()
            .Be("'Default Advertisement Pickup Address Country' must not be empty.");

        validationProblemDetails.Errors[
                nameof(UpdateApplicationUser.UpdateApplicationUserRequest.DefaultAdvertisementPickupAddressZipCode)][0]
            .Should()
            .Be("'Default Advertisement Pickup Address Zip Code' must not be empty.");

        validationProblemDetails.Errors[nameof(UpdateApplicationUser.UpdateApplicationUserRequest.DefaultAdvertisementPickupAddressCity)][0]
            .Should()
            .Be("'Default Advertisement Pickup Address City' must not be empty.");
    }
}
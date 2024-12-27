using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Auth.Api.Features.ApplicationUsers;
using KittySaver.Auth.Api.Features.ApplicationUsers.SharedContracts;
using Newtonsoft.Json;
using Shared;

namespace KittySaver.Auth.Api.Tests.Integration.Tests.ApplicationUser;

[Collection("AuthApi")]
public class GetApplicationUsersEndpointTests(KittySaverAuthApiFactory appFactory)
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
    public async Task GetApplicationUsers_ShouldReturnApplicationUsers_WhenUsersExists()
    {
        //Act
        Register.RegisterRequest registerRequest = _createApplicationUserRequestGenerator.Generate();
        HttpResponseMessage registerResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/application-users/register",
            registerRequest);
        ApiResponses.CreatedWithIdResponse registerResponse = 
            await registerResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>() ?? throw new JsonException();

        //Act
        HttpResponseMessage response = await _httpClient.GetAsync($"/api/v1/application-users");
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ICollection<ApplicationUserResponse>? applicationUsers = await response.Content.ReadFromJsonAsync<ICollection<ApplicationUserResponse>>();
        applicationUsers.Should().NotBeNull();
        const int defaultAdminPlusRecentlyRegisteredPersonCount = 2;
        applicationUsers!.Count.Should().BeGreaterOrEqualTo(defaultAdminPlusRecentlyRegisteredPersonCount);
        ApplicationUserResponse applicationUser = applicationUsers.First(x => x.Id == registerResponse.Id);
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
}

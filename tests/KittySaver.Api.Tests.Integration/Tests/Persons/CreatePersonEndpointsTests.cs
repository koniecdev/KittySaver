using System.Net;
using System.Net.Http.Json;
using Bogus;
using Bogus.Extensions;
using FluentAssertions;
using KittySaver.Api.Features.Persons;
using KittySaver.Api.Tests.Integration.Helpers;
using KittySaver.Domain.Persons;
using KittySaver.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace KittySaver.Api.Tests.Integration.Tests.Persons;

[Collection("Api")]
public class CreatePersonEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CleanupHelper _cleanup;
    public CreatePersonEndpointsTests(KittySaverApiFactory appFactory)
    {
        _httpClient = appFactory.CreateClient();
        _cleanup = new CleanupHelper(_httpClient);
    }
    
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
                    AddressState: faker.Address.State(),
                    AddressZipCode: faker.Address.ZipCode(),
                    AddressCity: faker.Address.City(),
                    AddressStreet: faker.Address.StreetName(),
                    AddressBuildingNumber: faker.Address.BuildingNumber(),
                    DefaultAdvertisementPickupAddressCountry: faker.Address.Country(),
                    DefaultAdvertisementPickupAddressState: faker.Address.State(),
                    DefaultAdvertisementPickupAddressZipCode: faker.Address.ZipCode(),
                    DefaultAdvertisementPickupAddressCity: faker.Address.City(),
                    DefaultAdvertisementPickupAddressStreet: faker.Address.StreetName(),
                    DefaultAdvertisementPickupAddressBuildingNumber: faker.Address.BuildingNumber(),
                    DefaultAdvertisementContactInfoEmail: faker.Person.Email,
                    DefaultAdvertisementContactInfoPhoneNumber: faker.Person.Phone
                ));
    
    [Fact]
    public async Task CreatePerson_ShouldReturnSuccess_WhenValidDataIsProvided()
    {
        //Arrange
        CreatePerson.CreatePersonRequest request = _createPersonRequestGenerator.Generate();
        
        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        ApiResponses.CreatedWithIdResponse? registerResponse = await response.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>();
        registerResponse.Should().NotBeNull();
        registerResponse!.Id.Should().NotBeEmpty();
        response.Headers.Location!.ToString().Should().Contain($"/api/v1/persons/{registerResponse.Id}");
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task CreatePerson_ShouldReturnSuccess_WhenValidDataIsProvidedWithoutState(string? state)
    {
        //Arrange
        CreatePerson.CreatePersonRequest request = new Faker<CreatePerson.CreatePersonRequest>()
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
                    AddressState: state,
                    DefaultAdvertisementPickupAddressCountry: faker.Address.Country(),
                    DefaultAdvertisementPickupAddressState: faker.Address.State(),
                    DefaultAdvertisementPickupAddressZipCode: faker.Address.ZipCode(),
                    DefaultAdvertisementPickupAddressCity: faker.Address.City(),
                    DefaultAdvertisementPickupAddressStreet: faker.Address.StreetName(),
                    DefaultAdvertisementPickupAddressBuildingNumber: faker.Address.BuildingNumber(),
                    DefaultAdvertisementContactInfoEmail: faker.Person.Email,
                    DefaultAdvertisementContactInfoPhoneNumber: faker.Person.Phone
                ));
        
        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        ApiResponses.CreatedWithIdResponse? registerResponse = await response.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>();
        registerResponse.Should().NotBeNull();
        registerResponse!.Id.Should().NotBeEmpty();
        response.Headers.Location!.ToString().Should().Contain($"/api/v1/persons/{registerResponse.Id}");
    }
    
    [Fact]
    public async Task CreatePerson_ShouldReturnBadRequest_WhenTooLongDataAreProvided()
    {
        //Arrange
        CreatePerson.CreatePersonRequest request = new Faker<CreatePerson.CreatePersonRequest>()
            .CustomInstantiator( faker =>
                new CreatePerson.CreatePersonRequest(
                    FirstName: faker.Person.FirstName.ClampLength(FirstName.MaxLength + 1),
                    LastName: faker.Person.LastName.ClampLength(LastName.MaxLength + 1),
                    Email: faker.Person.Email.ClampLength(Email.MaxLength + 1),
                    PhoneNumber: faker.Person.Phone.ClampLength(PhoneNumber.MaxLength + 1),
                    UserIdentityId: Guid.NewGuid(),
                    AddressCountry: faker.Address.Country().ClampLength(Address.CountryMaxLength + 1),
                    AddressZipCode: faker.Address.ZipCode().ClampLength(Address.ZipCodeMaxLength + 1),
                    AddressCity: faker.Address.City().ClampLength(Address.CityMaxLength + 1),
                    AddressStreet: faker.Address.StreetName().ClampLength(Address.StreetMaxLength + 1),
                    AddressBuildingNumber: faker.Address.BuildingNumber().ClampLength(Address.BuildingNumberMaxLength + 1),
                    AddressState: faker.Address.State().ClampLength(Address.StateMaxLength + 1),
                    DefaultAdvertisementPickupAddressCountry: faker.Address.Country().ClampLength(Address.CountryMaxLength + 1),
                    DefaultAdvertisementPickupAddressState: faker.Address.ZipCode().ClampLength(Address.StateMaxLength + 1),
                    DefaultAdvertisementPickupAddressZipCode: faker.Address.City().ClampLength(Address.ZipCodeMaxLength + 1),
                    DefaultAdvertisementPickupAddressCity: faker.Address.StreetName().ClampLength(Address.CityMaxLength + 1),
                    DefaultAdvertisementPickupAddressStreet: faker.Address.BuildingNumber().ClampLength(Address.StreetMaxLength + 1),
                    DefaultAdvertisementPickupAddressBuildingNumber: faker.Address.State().ClampLength(Address.BuildingNumberMaxLength + 1),
                    DefaultAdvertisementContactInfoEmail:faker.Person.Email.ClampLength(Email.MaxLength + 1),
                    DefaultAdvertisementContactInfoPhoneNumber: faker.Person.Phone.ClampLength(PhoneNumber.MaxLength + 1)
                ));
        
        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(18);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo(
            nameof(CreatePerson.CreatePersonRequest.FirstName),
            nameof(CreatePerson.CreatePersonRequest.LastName),
            nameof(CreatePerson.CreatePersonRequest.Email),
            nameof(CreatePerson.CreatePersonRequest.PhoneNumber),
            nameof(CreatePerson.CreatePersonRequest.AddressCountry),
            nameof(CreatePerson.CreatePersonRequest.AddressState),
            nameof(CreatePerson.CreatePersonRequest.AddressZipCode),
            nameof(CreatePerson.CreatePersonRequest.AddressCity),
            nameof(CreatePerson.CreatePersonRequest.AddressStreet),
            nameof(CreatePerson.CreatePersonRequest.AddressBuildingNumber),
            nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementPickupAddressCountry),
            nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementPickupAddressState),
            nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementPickupAddressZipCode),
            nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementPickupAddressCity),
            nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementPickupAddressStreet),
            nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementPickupAddressBuildingNumber),
            nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementContactInfoEmail),
            nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementContactInfoPhoneNumber)
        );
        validationProblemDetails.Errors.Values.Count.Should().Be(18);
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.FirstName)][0]
            .Should()
            .StartWith($"The length of 'First Name' must be {FirstName.MaxLength} characters or fewer. You entered");
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.LastName)][0]
            .Should()
            .StartWith($"The length of 'Last Name' must be {LastName.MaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.PhoneNumber)][0]
            .Should()
            .StartWith($"The length of 'Phone Number' must be {PhoneNumber.MaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.Email)][0]
            .Should()
            .StartWith($"The length of 'Email' must be {Email.MaxLength} characters or fewer. You entered");
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementContactInfoPhoneNumber)][0]
            .Should()
            .StartWith($"The length of 'Default Advertisement Contact Info Phone Number' must be {PhoneNumber.MaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementContactInfoEmail)][0]
            .Should()
            .StartWith($"The length of 'Default Advertisement Contact Info Email' must be {Email.MaxLength} characters or fewer. You entered");
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.AddressCountry)][0]
            .Should()
            .StartWith($"The length of 'Address Country' must be {Address.CountryMaxLength} characters or fewer. You entered");
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.AddressState)][0]
            .Should()
            .StartWith($"The length of 'Address State' must be {Address.StateMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.AddressZipCode)][0]
            .Should()
            .StartWith($"The length of 'Address Zip Code' must be {Address.ZipCodeMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.AddressCity)][0]
            .Should()
            .StartWith($"The length of 'Address City' must be {Address.CityMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.AddressStreet)][0]
            .Should()
            .StartWith($"The length of 'Address Street' must be {Address.StreetMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.AddressBuildingNumber)][0]
            .Should()
            .StartWith($"The length of 'Address Building Number' must be {Address.BuildingNumberMaxLength} characters or fewer. You entered");
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementPickupAddressCountry)][0]
            .Should()
            .StartWith($"The length of 'Default Advertisement Pickup Address Country' must be {Address.CountryMaxLength} characters or fewer. You entered");
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementPickupAddressState)][0]
            .Should()
            .StartWith($"The length of 'Default Advertisement Pickup Address State' must be {Address.StateMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementPickupAddressZipCode)][0]
            .Should()
            .StartWith($"The length of 'Default Advertisement Pickup Address Zip Code' must be {Address.ZipCodeMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementPickupAddressCity)][0]
            .Should()
            .StartWith($"The length of 'Default Advertisement Pickup Address City' must be {Address.CityMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementPickupAddressStreet)][0]
            .Should()
            .StartWith($"The length of 'Default Advertisement Pickup Address Street' must be {Address.StreetMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementPickupAddressBuildingNumber)][0]
            .Should()
            .StartWith($"The length of 'Default Advertisement Pickup Address Building Number' must be {Address.BuildingNumberMaxLength} characters or fewer. You entered");

    }
    
    [Fact]
    public async Task CreatePerson_ShouldReturnBadRequest_WhenEmptyDataAreProvided()
    {
        //Arrange
        CreatePerson.CreatePersonRequest request = new(
            FirstName: "",
            LastName: "",
            Email: "",
            PhoneNumber: "",
            UserIdentityId: Guid.Empty,
            AddressCountry: "",
            AddressZipCode: "",
            AddressCity: "",
            AddressStreet: "",
            AddressBuildingNumber: "",
            AddressState: "",
            DefaultAdvertisementPickupAddressCountry: "", 
            DefaultAdvertisementPickupAddressState: "", 
            DefaultAdvertisementPickupAddressZipCode: "", 
            DefaultAdvertisementPickupAddressCity: "", 
            DefaultAdvertisementPickupAddressStreet: "", 
            DefaultAdvertisementPickupAddressBuildingNumber: "", 
            DefaultAdvertisementContactInfoEmail: "",
            DefaultAdvertisementContactInfoPhoneNumber: "" 
        );
        
        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(15);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo(
            nameof(CreatePerson.CreatePersonRequest.FirstName),
            nameof(CreatePerson.CreatePersonRequest.LastName),
            nameof(CreatePerson.CreatePersonRequest.Email),
            nameof(CreatePerson.CreatePersonRequest.PhoneNumber),
            nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementContactInfoEmail),
            nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementContactInfoPhoneNumber),
            nameof(CreatePerson.CreatePersonRequest.UserIdentityId),
            nameof(CreatePerson.CreatePersonRequest.AddressCountry),
            nameof(CreatePerson.CreatePersonRequest.AddressZipCode),
            nameof(CreatePerson.CreatePersonRequest.AddressCity),
            nameof(CreatePerson.CreatePersonRequest.AddressStreet),
            nameof(CreatePerson.CreatePersonRequest.AddressBuildingNumber),
            nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementPickupAddressCountry),
            nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementPickupAddressZipCode),
            nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementPickupAddressCity)
        );
        validationProblemDetails.Errors.Values.Count.Should().Be(15);
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.FirstName)][0]
            .Should()
            .Be("'First Name' must not be empty.");
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.LastName)][0]
            .Should()
            .Be("'Last Name' must not be empty.");
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.Email)][0]
            .Should()
            .Be("'Email' must not be empty.");
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.PhoneNumber)][0]
            .Should()
            .Be("'Phone Number' must not be empty.");
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementContactInfoEmail)][0]
            .Should()
            .Be("'Default Advertisement Contact Info Email' must not be empty.");
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementContactInfoPhoneNumber)][0]
            .Should()
            .Be("'Default Advertisement Contact Info Phone Number' must not be empty.");
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.UserIdentityId)][0]
            .Should()
            .Be("'User Identity Id' must not be empty.");
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.AddressCountry)][0]
            .Should()
            .Be("'Address Country' must not be empty.");
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.AddressZipCode)][0]
            .Should()
            .Be("'Address Zip Code' must not be empty.");
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.AddressCity)][0]
            .Should()
            .Be("'Address City' must not be empty.");
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.AddressStreet)][0]
            .Should()
            .Be("'Address Street' must not be empty.");
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.AddressBuildingNumber)][0]
            .Should()
            .Be("'Address Building Number' must not be empty.");
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementPickupAddressCountry)][0]
            .Should()
            .Be("'Default Advertisement Pickup Address Country' must not be empty.");
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementPickupAddressZipCode)][0]
            .Should()
            .Be("'Default Advertisement Pickup Address Zip Code' must not be empty.");
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementPickupAddressCity)][0]
            .Should()
            .Be("'Default Advertisement Pickup Address City' must not be empty.");
    }
    
    [Theory]
    [ClassData(typeof(InvalidEmailData))]
    public async Task CreatePerson_ShouldReturnBadRequest_WhenInvalidEmailIsProvided(string email)
    {
        //Arrange
        CreatePerson.CreatePersonRequest request = new Faker<CreatePerson.CreatePersonRequest>()
            .CustomInstantiator( faker =>
                new CreatePerson.CreatePersonRequest(
                    FirstName: faker.Person.FirstName,
                    LastName: faker.Person.LastName,
                    Email: email,
                    PhoneNumber: faker.Person.Phone,
                    UserIdentityId: Guid.NewGuid(),
                    AddressCountry: faker.Address.Country(),
                    AddressZipCode: faker.Address.ZipCode(),
                    AddressCity: faker.Address.City(),
                    AddressStreet: faker.Address.StreetName(),
                    AddressBuildingNumber: faker.Address.BuildingNumber(),
                    AddressState: faker.Address.State(),
                    DefaultAdvertisementPickupAddressCountry: faker.Address.Country(),
                    DefaultAdvertisementPickupAddressState: faker.Address.State(),
                    DefaultAdvertisementPickupAddressZipCode: faker.Address.ZipCode(),
                    DefaultAdvertisementPickupAddressCity: faker.Address.City(),
                    DefaultAdvertisementPickupAddressStreet: faker.Address.StreetName(),
                    DefaultAdvertisementPickupAddressBuildingNumber: faker.Address.BuildingNumber(),
                    DefaultAdvertisementContactInfoEmail: faker.Person.Email,
                    DefaultAdvertisementContactInfoPhoneNumber: faker.Person.Phone
                ));
        
        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(1);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo(
            nameof(CreatePerson.CreatePersonRequest.Email)
        );
        validationProblemDetails.Errors.Values.Count.Should().Be(1);
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.Email)][0]
            .Should().StartWith("'Email' is not in the correct format.");
    }
    
    [Fact]
    public async Task CreatePerson_ShouldReturnBadRequest_WhenAlreadyTakenUniquePropertiesAreProvided()
    {
        //Arrange
        CreatePerson.CreatePersonRequest request = _createPersonRequestGenerator.Generate();
        _ = await _httpClient.PostAsJsonAsync("api/v1/persons", request);
        
        //Act
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(3);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo(
            nameof(CreatePerson.CreatePersonRequest.Email),
            nameof(CreatePerson.CreatePersonRequest.PhoneNumber),
            nameof(CreatePerson.CreatePersonRequest.UserIdentityId)
        );
        validationProblemDetails.Errors.Values.Count.Should().Be(3);
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.Email)][0]
            .Should()
            .Be("'Email' is already used by another user.");
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.PhoneNumber)][0]
            .Should()
            .Be("'Phone Number' is already used by another user.");
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.UserIdentityId)][0]
            .Should()
            .Be("'User Identity Id' is already used by another user.");
    }
    
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _cleanup.Cleanup();
    }
}

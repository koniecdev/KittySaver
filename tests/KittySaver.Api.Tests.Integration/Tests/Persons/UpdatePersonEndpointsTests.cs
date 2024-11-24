using System.Net;
using System.Net.Http.Json;
using Bogus;
using Bogus.Extensions;
using FluentAssertions;
using KittySaver.Api.Features.Persons;
using KittySaver.Api.Features.Persons.SharedContracts;
using KittySaver.Api.Tests.Integration.Helpers;
using KittySaver.Domain.Persons;
using KittySaver.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared;
using JsonException = System.Text.Json.JsonException;

namespace KittySaver.Api.Tests.Integration.Tests.Persons;

[Collection("Api")]
public class UpdatePersonEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CleanupHelper _cleanup;
    public UpdatePersonEndpointsTests(KittySaverApiFactory appFactory)
    {
        _httpClient = appFactory.CreateClient();
        _cleanup = new CleanupHelper(_httpClient);
    }

    private readonly CreatePerson.CreatePersonRequest _createPersonRequest =
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
                    AddressState: faker.Address.State(),
                    DefaultAdvertisementPickupAddressCountry: faker.Address.Country(),
                    DefaultAdvertisementPickupAddressState: faker.Address.State(),
                    DefaultAdvertisementPickupAddressZipCode: faker.Address.ZipCode(),
                    DefaultAdvertisementPickupAddressCity: faker.Address.City(),
                    DefaultAdvertisementPickupAddressStreet: faker.Address.StreetName(),
                    DefaultAdvertisementPickupAddressBuildingNumber: faker.Address.BuildingNumber(),
                    DefaultAdvertisementContactInfoEmail: faker.Person.Email,
                    DefaultAdvertisementContactInfoPhoneNumber: faker.Person.Phone
                )).Generate();
    
    [Fact]
    public async Task UpdatePerson_ShouldReturnSuccess_WhenValidDataIsProvided()
    {
        //Arrange
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        ApiResponses.CreatedWithIdResponse registeredPersonResponse = await response.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
                                                                      ?? throw new JsonException();
        PersonResponse person = await _httpClient.GetFromJsonAsync<PersonResponse>($"api/v1/persons/{registeredPersonResponse.Id}") 
                                ?? throw new JsonException();
        //Act
        UpdatePerson.UpdatePersonRequest request = new Faker<UpdatePerson.UpdatePersonRequest>()
            .CustomInstantiator(faker =>
                new UpdatePerson.UpdatePersonRequest(
                    FirstName: faker.Person.FirstName,
                    LastName: faker.Person.LastName,
                    Email: faker.Person.Email,
                    PhoneNumber: faker.Person.Phone,
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
        
        HttpResponseMessage updateResponse = await _httpClient.PutAsJsonAsync($"api/v1/persons/{registeredPersonResponse.Id}", request);
        
        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        PersonResponse personAfterUpdate = await _httpClient.GetFromJsonAsync<PersonResponse>($"api/v1/persons/{registeredPersonResponse.Id}") 
                                ?? throw new JsonException();
        personAfterUpdate.Should().NotBeEquivalentTo(person);
        personAfterUpdate.FirstName.Should().Be(request.FirstName);
        personAfterUpdate.LastName.Should().Be(request.LastName);
        personAfterUpdate.Email.Should().Be(request.Email);
        personAfterUpdate.PhoneNumber.Should().Be(request.PhoneNumber);
        personAfterUpdate.FullName.Should().Be($"{request.FirstName} {request.LastName}");
        personAfterUpdate.ResidentalAddress.Country.Should().Be(request.AddressCountry);
        personAfterUpdate.ResidentalAddress.State.Should().Be(request.AddressState);
        personAfterUpdate.ResidentalAddress.ZipCode.Should().Be(request.AddressZipCode);
        personAfterUpdate.ResidentalAddress.City.Should().Be(request.AddressCity);
        personAfterUpdate.ResidentalAddress.Street.Should().Be(request.AddressStreet);
        personAfterUpdate.ResidentalAddress.City.Should().Be(request.AddressCity);
        personAfterUpdate.ResidentalAddress.BuildingNumber.Should().Be(request.AddressBuildingNumber);
    }
    
    [Fact]
    public async Task UpdatePerson_ShouldReturnSuccess_WhenValidDataIsProvidedWithUserIdentityId()
    {
        //Arrange
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        ApiResponses.CreatedWithIdResponse registeredPersonResponse = await response.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
                                                                      ?? throw new JsonException();
        PersonResponse person = await _httpClient.GetFromJsonAsync<PersonResponse>($"api/v1/persons/{registeredPersonResponse.Id}") 
                                ?? throw new JsonException();
        //Act
        UpdatePerson.UpdatePersonRequest request = new Faker<UpdatePerson.UpdatePersonRequest>()
            .CustomInstantiator(faker =>
                new UpdatePerson.UpdatePersonRequest(
                    FirstName: person.FirstName,
                    LastName: faker.Person.LastName,
                    Email: person.Email,
                    PhoneNumber: person.PhoneNumber,
                    AddressCountry: person.ResidentalAddress.Country,
                    AddressZipCode: person.ResidentalAddress.ZipCode,
                    AddressCity: person.ResidentalAddress.City,
                    AddressStreet: person.ResidentalAddress.Street,
                    AddressBuildingNumber: faker.Address.BuildingNumber(),
                    AddressState: person.ResidentalAddress.State,
                    DefaultAdvertisementPickupAddressCountry: faker.Address.Country(),
                    DefaultAdvertisementPickupAddressState: faker.Address.State(),
                    DefaultAdvertisementPickupAddressZipCode: faker.Address.ZipCode(),
                    DefaultAdvertisementPickupAddressCity: faker.Address.City(),
                    DefaultAdvertisementPickupAddressStreet: faker.Address.StreetName(),
                    DefaultAdvertisementPickupAddressBuildingNumber: faker.Address.BuildingNumber(),
                    DefaultAdvertisementContactInfoEmail: faker.Person.Email,
                    DefaultAdvertisementContactInfoPhoneNumber: faker.Person.Phone
                )).Generate();
        
        HttpResponseMessage updateResponse = await _httpClient.PutAsJsonAsync($"api/v1/persons/{person.UserIdentityId}", request);
        
        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        PersonResponse personAfterUpdate = await _httpClient.GetFromJsonAsync<PersonResponse>($"api/v1/persons/{registeredPersonResponse.Id}") 
                                           ?? throw new JsonException();
        personAfterUpdate.Should().NotBeEquivalentTo(person);
        personAfterUpdate.FirstName.Should().Be(request.FirstName);
        personAfterUpdate.LastName.Should().Be(request.LastName);
        personAfterUpdate.FullName.Should().Be($"{request.FirstName} {request.LastName}");
        personAfterUpdate.Email.Should().Be(request.Email);
        personAfterUpdate.PhoneNumber.Should().Be(request.PhoneNumber);
        personAfterUpdate.ResidentalAddress.Country.Should().Be(request.AddressCountry);
        personAfterUpdate.ResidentalAddress.State.Should().Be(request.AddressState);
        personAfterUpdate.ResidentalAddress.ZipCode.Should().Be(request.AddressZipCode);
        personAfterUpdate.ResidentalAddress.City.Should().Be(request.AddressCity);
        personAfterUpdate.ResidentalAddress.Street.Should().Be(request.AddressStreet);
        personAfterUpdate.ResidentalAddress.City.Should().Be(request.AddressCity);
        personAfterUpdate.ResidentalAddress.BuildingNumber.Should().Be(request.AddressBuildingNumber);
    }
    
    [Fact]
    public async Task UpdatePerson_ShouldReturnNotFound_WhenNonRegisteredUserIdProvided()
    {
        //Arrange
        Guid randomId = Guid.NewGuid();
        
        //Act
        UpdatePerson.UpdatePersonRequest request = new Faker<UpdatePerson.UpdatePersonRequest>()
            .CustomInstantiator(faker =>
                new UpdatePerson.UpdatePersonRequest(
                    FirstName: faker.Person.FirstName,
                    LastName: faker.Person.LastName,
                    Email: faker.Person.Email,
                    PhoneNumber: faker.Person.Phone,
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
        
        HttpResponseMessage updateResponse = await _httpClient.PutAsJsonAsync($"api/v1/persons/{randomId}", request);
        
        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails? notFoundProblemDetails = await updateResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        notFoundProblemDetails.Should().NotBeNull();
        notFoundProblemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task UpdatePerson_ShouldReturnBadRequest_WhenEmptyDataAreProvided()
    {
        //Arrange
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        ApiResponses.CreatedWithIdResponse firstUserIdResponse = await response.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
                                                                 ?? throw new JsonException();
        
        //Act
        UpdatePerson.UpdatePersonRequest request = new(
                FirstName: "",
                LastName: "",
                Email: "",
                PhoneNumber: "",
                AddressCountry: "",
                AddressState: "",
                AddressZipCode: "",
                AddressCity: "",
                AddressStreet: "",
                AddressBuildingNumber: "",
                DefaultAdvertisementPickupAddressCountry: "", 
                DefaultAdvertisementPickupAddressState: "", 
                DefaultAdvertisementPickupAddressZipCode: "", 
                DefaultAdvertisementPickupAddressCity: "", 
                DefaultAdvertisementPickupAddressStreet: "", 
                DefaultAdvertisementPickupAddressBuildingNumber: "", 
                DefaultAdvertisementContactInfoEmail: "",
                DefaultAdvertisementContactInfoPhoneNumber: "" 
            );
        HttpResponseMessage updateResponse = await _httpClient.PutAsJsonAsync($"api/v1/persons/{firstUserIdResponse.Id}", request);
        
        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails = await updateResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(14);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo(
            nameof(UpdatePerson.UpdatePersonRequest.FirstName),
            nameof(UpdatePerson.UpdatePersonRequest.LastName),
            nameof(UpdatePerson.UpdatePersonRequest.Email),
            nameof(UpdatePerson.UpdatePersonRequest.PhoneNumber),
            nameof(UpdatePerson.UpdatePersonRequest.DefaultAdvertisementContactInfoEmail),
            nameof(UpdatePerson.UpdatePersonRequest.DefaultAdvertisementContactInfoPhoneNumber),
            nameof(UpdatePerson.UpdatePersonRequest.AddressCountry),
            nameof(UpdatePerson.UpdatePersonRequest.AddressZipCode),
            nameof(UpdatePerson.UpdatePersonRequest.AddressCity),
            nameof(UpdatePerson.UpdatePersonRequest.AddressStreet),
            nameof(UpdatePerson.UpdatePersonRequest.AddressBuildingNumber),
            nameof(UpdatePerson.UpdatePersonRequest.DefaultAdvertisementPickupAddressCountry),
            nameof(UpdatePerson.UpdatePersonRequest.DefaultAdvertisementPickupAddressZipCode),
            nameof(UpdatePerson.UpdatePersonRequest.DefaultAdvertisementPickupAddressCity)
        );
        
        validationProblemDetails.Errors.Values.Count.Should().Be(14);
        
        validationProblemDetails.Errors[nameof(UpdatePerson.UpdatePersonRequest.FirstName)][0]
            .Should()
            .Be("'First Name' must not be empty.");
        
        validationProblemDetails.Errors[nameof(UpdatePerson.UpdatePersonRequest.LastName)][0]
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
        
        validationProblemDetails.Errors[nameof(UpdatePerson.UpdatePersonRequest.AddressCountry)][0]
            .Should()
            .Be("'Address Country' must not be empty.");
        
        validationProblemDetails.Errors[nameof(UpdatePerson.UpdatePersonRequest.AddressZipCode)][0]
            .Should()
            .Be("'Address Zip Code' must not be empty.");
        
        validationProblemDetails.Errors[nameof(UpdatePerson.UpdatePersonRequest.AddressCity)][0]
            .Should()
            .Be("'Address City' must not be empty.");
        
        validationProblemDetails.Errors[nameof(UpdatePerson.UpdatePersonRequest.AddressStreet)][0]
            .Should()
            .Be("'Address Street' must not be empty.");
        
        validationProblemDetails.Errors[nameof(UpdatePerson.UpdatePersonRequest.AddressBuildingNumber)][0]
            .Should()
            .Be("'Address Building Number' must not be empty.");
        
        validationProblemDetails.Errors[nameof(UpdatePerson.UpdatePersonRequest.DefaultAdvertisementPickupAddressCountry)][0]
            .Should()
            .Be("'Default Advertisement Pickup Address Country' must not be empty.");
        
        validationProblemDetails.Errors[nameof(UpdatePerson.UpdatePersonRequest.DefaultAdvertisementPickupAddressZipCode)][0]
            .Should()
            .Be("'Default Advertisement Pickup Address Zip Code' must not be empty.");
        
        validationProblemDetails.Errors[nameof(UpdatePerson.UpdatePersonRequest.DefaultAdvertisementPickupAddressCity)][0]
            .Should()
            .Be("'Default Advertisement Pickup Address City' must not be empty.");
    }
    
    [Fact]
    public async Task UpdatePerson_ShouldReturnBadRequest_WhenTooLongDataAreProvided()
    {
        //Arrange
        HttpResponseMessage registerResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        ApiResponses.CreatedWithIdResponse registerResponse = await registerResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
                                                                     ?? throw new JsonException();
        
        UpdatePerson.UpdatePersonRequest request = new Faker<UpdatePerson.UpdatePersonRequest>()
            .CustomInstantiator( faker =>
                new UpdatePerson.UpdatePersonRequest(
                    FirstName: faker.Person.FirstName.ClampLength(FirstName.MaxLength + 1),
                    LastName: faker.Person.LastName.ClampLength(LastName.MaxLength + 1),
                    Email: faker.Person.Email.ClampLength(Email.MaxLength + 1),
                    PhoneNumber: faker.Person.Phone.ClampLength(PhoneNumber.MaxLength + 1),
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
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/v1/persons/{registerResponse.Id}", request);
        
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
    
    [Theory]
    [ClassData(typeof(InvalidEmailData))]
    public async Task UpdatePerson_ShouldReturnBadRequest_WhenInvalidEmailIsProvided(string email)
    {
        //Arrange
        HttpResponseMessage createPersonResponse = await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        ApiResponses.CreatedWithIdResponse idOfCreatedPersonResponse = 
            await createPersonResponse.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>() 
            ?? throw new JsonException();
        PersonResponse person = await _httpClient.GetFromJsonAsync<PersonResponse>($"api/v1/persons/{idOfCreatedPersonResponse.Id}") 
                                ?? throw new JsonException();
        
        UpdatePerson.UpdatePersonRequest request = new UpdatePerson.UpdatePersonRequest(
            FirstName: person.FirstName,
            LastName: person.LastName,
            Email: email,
            PhoneNumber: person.PhoneNumber,
            AddressCountry: person.ResidentalAddress.Country,
            AddressState: person.ResidentalAddress.State,
            AddressZipCode: person.ResidentalAddress.ZipCode,
            AddressCity: person.ResidentalAddress.City,
            AddressStreet: person.ResidentalAddress.Street,
            AddressBuildingNumber: person.ResidentalAddress.BuildingNumber,
            DefaultAdvertisementPickupAddressCountry: person.DefaultAdvertisementsPickupAddress.Country,
            DefaultAdvertisementPickupAddressState: person.DefaultAdvertisementsPickupAddress.State,
            DefaultAdvertisementPickupAddressZipCode: person.DefaultAdvertisementsPickupAddress.ZipCode,
            DefaultAdvertisementPickupAddressCity: person.DefaultAdvertisementsPickupAddress.City,
            DefaultAdvertisementPickupAddressStreet: person.DefaultAdvertisementsPickupAddress.Street,
            DefaultAdvertisementPickupAddressBuildingNumber: person.DefaultAdvertisementsPickupAddress.BuildingNumber,
            DefaultAdvertisementContactInfoEmail: person.DefaultAdvertisementsContactInfoEmail,
            DefaultAdvertisementContactInfoPhoneNumber: person.DefaultAdvertisementsContactInfoPhoneNumber
        );
        
        //Act
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/v1/persons/{person.Id}", request);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(1);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo([
            nameof(UpdatePerson.UpdatePersonRequest.Email)
        ]);
        validationProblemDetails.Errors.Values.Count.Should().Be(1);
        validationProblemDetails.Errors[nameof(UpdatePerson.UpdatePersonRequest.Email)][0]
            .Should().Be("'Email' is not in the correct format.");
    }
    
    [Fact]
    public async Task UpdatePerson_ShouldReturnBadRequest_WhenAlreadyTakenUniquePropertiesAreProvided()
    {
        //Arrange
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        ApiResponses.CreatedWithIdResponse firstUserIdResponse = await response.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
                                                                 ?? throw new JsonException();
        
        CreatePerson.CreatePersonRequest secondPersonCreateRequest = _createPersonRequest with
        {
            Email = "unique@email.com",
            PhoneNumber = "420420420",
            UserIdentityId = Guid.NewGuid()
        };
        _= await _httpClient.PostAsJsonAsync("api/v1/persons", secondPersonCreateRequest);
        
        
        PersonResponse firstUser = await _httpClient.GetFromJsonAsync<PersonResponse>($"api/v1/persons/{firstUserIdResponse.Id}") 
                                 ?? throw new JsonException();
        
        //Act
        UpdatePerson.UpdatePersonRequest firstUserUpdateRequest = new(
            FirstName: firstUser.FirstName,
            LastName: firstUser.LastName,
            Email: secondPersonCreateRequest.Email,
            PhoneNumber: secondPersonCreateRequest.PhoneNumber,
            AddressCountry: firstUser.ResidentalAddress.Country,
            AddressState: firstUser.ResidentalAddress.State,
            AddressZipCode: firstUser.ResidentalAddress.ZipCode,
            AddressCity: firstUser.ResidentalAddress.City,
            AddressStreet: firstUser.ResidentalAddress.Street,
            AddressBuildingNumber: firstUser.ResidentalAddress.BuildingNumber,
            DefaultAdvertisementPickupAddressCountry: firstUser.DefaultAdvertisementsPickupAddress.Country,
            DefaultAdvertisementPickupAddressState: firstUser.DefaultAdvertisementsPickupAddress.State,
            DefaultAdvertisementPickupAddressZipCode: firstUser.DefaultAdvertisementsPickupAddress.ZipCode,
            DefaultAdvertisementPickupAddressCity: firstUser.DefaultAdvertisementsPickupAddress.City,
            DefaultAdvertisementPickupAddressStreet: firstUser.DefaultAdvertisementsPickupAddress.Street,
            DefaultAdvertisementPickupAddressBuildingNumber: firstUser.DefaultAdvertisementsPickupAddress.BuildingNumber,
            DefaultAdvertisementContactInfoEmail: firstUser.DefaultAdvertisementsContactInfoEmail,
            DefaultAdvertisementContactInfoPhoneNumber: firstUser.DefaultAdvertisementsContactInfoPhoneNumber
        );
        
        HttpResponseMessage updateResponse = await _httpClient.PutAsJsonAsync($"api/v1/persons/{firstUserIdResponse.Id}", firstUserUpdateRequest);
        
        //Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await updateResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(2);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo([
            nameof(UpdatePerson.UpdatePersonRequest.Email),
            nameof(UpdatePerson.UpdatePersonRequest.PhoneNumber)
        ]);
        validationProblemDetails.Errors.Values.Count.Should().Be(2);
        validationProblemDetails.Errors[nameof(UpdatePerson.UpdatePersonRequest.Email)][0]
            .Should().Be("'Email' is already used by another user.");
        validationProblemDetails.Errors[nameof(UpdatePerson.UpdatePersonRequest.PhoneNumber)][0]
            .Should().Be("'Phone Number' is already used by another user.");
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
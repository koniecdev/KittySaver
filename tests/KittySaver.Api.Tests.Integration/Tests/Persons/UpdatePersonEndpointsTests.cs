using System.Net;
using System.Net.Http.Json;
using Bogus;
using Bogus.Extensions;
using FluentAssertions;
using KittySaver.Api.Features.Persons;
using KittySaver.Api.Features.Persons.SharedContracts;
using KittySaver.Api.Shared.Domain.Common.Interfaces;
using KittySaver.Api.Tests.Integration.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared;
using JsonException = System.Text.Json.JsonException;
using Person = KittySaver.Api.Shared.Domain.Entites.Person;

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
        personAfterUpdate.Address.Country.Should().Be(request.AddressCountry);
        personAfterUpdate.Address.State.Should().Be(request.AddressState);
        personAfterUpdate.Address.ZipCode.Should().Be(request.AddressZipCode);
        personAfterUpdate.Address.City.Should().Be(request.AddressCity);
        personAfterUpdate.Address.Street.Should().Be(request.AddressStreet);
        personAfterUpdate.Address.City.Should().Be(request.AddressCity);
        personAfterUpdate.Address.BuildingNumber.Should().Be(request.AddressBuildingNumber);
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
                    AddressCountry: person.Address.Country,
                    AddressZipCode: person.Address.ZipCode,
                    AddressCity: person.Address.City,
                    AddressStreet: person.Address.Street,
                    AddressBuildingNumber: faker.Address.BuildingNumber(),
                    AddressState: person.Address.State,
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
        personAfterUpdate.Address.Country.Should().Be(request.AddressCountry);
        personAfterUpdate.Address.State.Should().Be(request.AddressState);
        personAfterUpdate.Address.ZipCode.Should().Be(request.AddressZipCode);
        personAfterUpdate.Address.City.Should().Be(request.AddressCity);
        personAfterUpdate.Address.Street.Should().Be(request.AddressStreet);
        personAfterUpdate.Address.City.Should().Be(request.AddressCity);
        personAfterUpdate.Address.BuildingNumber.Should().Be(request.AddressBuildingNumber);
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
                    FirstName: faker.Person.FirstName.ClampLength(Person.Constraints.FirstNameMaxLength + 1),
                    LastName: faker.Person.LastName.ClampLength(Person.Constraints.LastNameMaxLength + 1),
                    Email: faker.Person.Email.ClampLength(IContact.Constraints.EmailMaxLength + 1),
                    PhoneNumber: faker.Person.Phone.ClampLength(IContact.Constraints.PhoneNumberMaxLength + 1),
                    AddressCountry: faker.Address.Country().ClampLength(IAddress.Constraints.CountryMaxLength + 1),
                    AddressZipCode: faker.Address.ZipCode().ClampLength(IAddress.Constraints.ZipCodeMaxLength + 1),
                    AddressCity: faker.Address.City().ClampLength(IAddress.Constraints.CityMaxLength + 1),
                    AddressStreet: faker.Address.StreetName().ClampLength(IAddress.Constraints.StreetMaxLength + 1),
                    AddressBuildingNumber: faker.Address.BuildingNumber().ClampLength(IAddress.Constraints.BuildingNumberMaxLength + 1),
                    AddressState: faker.Address.State().ClampLength(IAddress.Constraints.StateMaxLength + 1),
                    DefaultAdvertisementPickupAddressCountry: faker.Address.Country().ClampLength(IAddress.Constraints.CountryMaxLength + 1),
                    DefaultAdvertisementPickupAddressState: faker.Address.ZipCode().ClampLength(IAddress.Constraints.StateMaxLength + 1),
                    DefaultAdvertisementPickupAddressZipCode: faker.Address.City().ClampLength(IAddress.Constraints.ZipCodeMaxLength + 1),
                    DefaultAdvertisementPickupAddressCity: faker.Address.StreetName().ClampLength(IAddress.Constraints.CityMaxLength + 1),
                    DefaultAdvertisementPickupAddressStreet: faker.Address.BuildingNumber().ClampLength(IAddress.Constraints.StreetMaxLength + 1),
                    DefaultAdvertisementPickupAddressBuildingNumber: faker.Address.State().ClampLength(IAddress.Constraints.BuildingNumberMaxLength + 1),
                    DefaultAdvertisementContactInfoEmail:faker.Person.Email.ClampLength(IContact.Constraints.EmailMaxLength + 1),
                    DefaultAdvertisementContactInfoPhoneNumber: faker.Person.Phone.ClampLength(IContact.Constraints.PhoneNumberMaxLength + 1)
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
            .StartWith($"The length of 'First Name' must be {Person.Constraints.FirstNameMaxLength} characters or fewer. You entered");
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.LastName)][0]
            .Should()
            .StartWith($"The length of 'Last Name' must be {Person.Constraints.LastNameMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.PhoneNumber)][0]
            .Should()
            .StartWith($"The length of 'Phone Number' must be {IContact.Constraints.PhoneNumberMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.Email)][0]
            .Should()
            .StartWith($"The length of 'Email' must be {IContact.Constraints.EmailMaxLength} characters or fewer. You entered");
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementContactInfoPhoneNumber)][0]
            .Should()
            .StartWith($"The length of 'Default Advertisement Contact Info Phone Number' must be {IContact.Constraints.PhoneNumberMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementContactInfoEmail)][0]
            .Should()
            .StartWith($"The length of 'Default Advertisement Contact Info Email' must be {IContact.Constraints.EmailMaxLength} characters or fewer. You entered");
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.AddressCountry)][0]
            .Should()
            .StartWith($"The length of 'Address Country' must be {IAddress.Constraints.CountryMaxLength} characters or fewer. You entered");
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.AddressState)][0]
            .Should()
            .StartWith($"The length of 'Address State' must be {IAddress.Constraints.StateMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.AddressZipCode)][0]
            .Should()
            .StartWith($"The length of 'Address Zip Code' must be {IAddress.Constraints.ZipCodeMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.AddressCity)][0]
            .Should()
            .StartWith($"The length of 'Address City' must be {IAddress.Constraints.CityMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.AddressStreet)][0]
            .Should()
            .StartWith($"The length of 'Address Street' must be {IAddress.Constraints.StreetMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.AddressBuildingNumber)][0]
            .Should()
            .StartWith($"The length of 'Address Building Number' must be {IAddress.Constraints.BuildingNumberMaxLength} characters or fewer. You entered");
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementPickupAddressCountry)][0]
            .Should()
            .StartWith($"The length of 'Default Advertisement Pickup Address Country' must be {IAddress.Constraints.CountryMaxLength} characters or fewer. You entered");
        
        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementPickupAddressState)][0]
            .Should()
            .StartWith($"The length of 'Default Advertisement Pickup Address State' must be {IAddress.Constraints.StateMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementPickupAddressZipCode)][0]
            .Should()
            .StartWith($"The length of 'Default Advertisement Pickup Address Zip Code' must be {IAddress.Constraints.ZipCodeMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementPickupAddressCity)][0]
            .Should()
            .StartWith($"The length of 'Default Advertisement Pickup Address City' must be {IAddress.Constraints.CityMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementPickupAddressStreet)][0]
            .Should()
            .StartWith($"The length of 'Default Advertisement Pickup Address Street' must be {IAddress.Constraints.StreetMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreatePerson.CreatePersonRequest.DefaultAdvertisementPickupAddressBuildingNumber)][0]
            .Should()
            .StartWith($"The length of 'Default Advertisement Pickup Address Building Number' must be {IAddress.Constraints.BuildingNumberMaxLength} characters or fewer. You entered");

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
            AddressCountry: person.Address.Country,
            AddressState: person.Address.State,
            AddressZipCode: person.Address.ZipCode,
            AddressCity: person.Address.City,
            AddressStreet: person.Address.Street,
            AddressBuildingNumber: person.Address.BuildingNumber,
            DefaultAdvertisementPickupAddressCountry: person.DefaultAdvertisementsPickupAddress.Country,
            DefaultAdvertisementPickupAddressState: person.DefaultAdvertisementsPickupAddress.State,
            DefaultAdvertisementPickupAddressZipCode: person.DefaultAdvertisementsPickupAddress.ZipCode,
            DefaultAdvertisementPickupAddressCity: person.DefaultAdvertisementsPickupAddress.City,
            DefaultAdvertisementPickupAddressStreet: person.DefaultAdvertisementsPickupAddress.Street,
            DefaultAdvertisementPickupAddressBuildingNumber: person.DefaultAdvertisementsPickupAddress.BuildingNumber,
            DefaultAdvertisementContactInfoEmail: person.DefaultAdvertisementsContactInfo.Email,
            DefaultAdvertisementContactInfoPhoneNumber: person.DefaultAdvertisementsContactInfo.PhoneNumber
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
            AddressCountry: firstUser.Address.Country,
            AddressState: firstUser.Address.State,
            AddressZipCode: firstUser.Address.ZipCode,
            AddressCity: firstUser.Address.City,
            AddressStreet: firstUser.Address.Street,
            AddressBuildingNumber: firstUser.Address.BuildingNumber,
            DefaultAdvertisementPickupAddressCountry: firstUser.DefaultAdvertisementsPickupAddress.Country,
            DefaultAdvertisementPickupAddressState: firstUser.DefaultAdvertisementsPickupAddress.State,
            DefaultAdvertisementPickupAddressZipCode: firstUser.DefaultAdvertisementsPickupAddress.ZipCode,
            DefaultAdvertisementPickupAddressCity: firstUser.DefaultAdvertisementsPickupAddress.City,
            DefaultAdvertisementPickupAddressStreet: firstUser.DefaultAdvertisementsPickupAddress.Street,
            DefaultAdvertisementPickupAddressBuildingNumber: firstUser.DefaultAdvertisementsPickupAddress.BuildingNumber,
            DefaultAdvertisementContactInfoEmail: firstUser.DefaultAdvertisementsContactInfo.Email,
            DefaultAdvertisementContactInfoPhoneNumber: firstUser.DefaultAdvertisementsContactInfo.PhoneNumber
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
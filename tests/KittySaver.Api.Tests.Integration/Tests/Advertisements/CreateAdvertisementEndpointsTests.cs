using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Features.Advertisements;
using KittySaver.Api.Infrastructure.Endpoints;
using KittySaver.Api.Tests.Integration.Helpers;
using KittySaver.Domain.ValueObjects;
using KittySaver.Shared.Common.Enums;
using KittySaver.Shared.Hateoas;
using KittySaver.Shared.Responses;
using KittySaver.Shared.TypedIds;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using KittySaver.Tests.Shared;

namespace KittySaver.Api.Tests.Integration.Tests.Advertisements;

[Collection("Api")]
public class CreateAdvertisementEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CleanupHelper _cleanup;

    public CreateAdvertisementEndpointsTests(KittySaverApiFactory appFactory)
    {
        _httpClient = appFactory.CreateClient();
        _cleanup = new CleanupHelper(_httpClient);
    }

    private readonly Faker<CreatePersonRequest> _createPersonRequestGenerator =
        new Faker<CreatePersonRequest>()
            .CustomInstantiator(faker =>
                new CreatePersonRequest(
                    Nickname: faker.Person.FirstName,
                    Email: faker.Person.Email,
                    PhoneNumber: faker.Person.Phone,
                    Password: "Default123$",
                    DefaultAdvertisementPickupAddressCountry: faker.Address.CountryCode(),
                    DefaultAdvertisementPickupAddressState: faker.Address.State(),
                    DefaultAdvertisementPickupAddressZipCode: faker.Address.ZipCode(),
                    DefaultAdvertisementPickupAddressCity: faker.Address.City(),
                    DefaultAdvertisementPickupAddressStreet: faker.Address.StreetName(),
                    DefaultAdvertisementPickupAddressBuildingNumber: faker.Address.BuildingNumber(),
                    DefaultAdvertisementContactInfoEmail: faker.Person.Email,
                    DefaultAdvertisementContactInfoPhoneNumber: faker.Person.Phone
                ));

    private readonly Faker<CreateCatRequest> _createCatRequestGenerator =
        new Faker<CreateCatRequest>()
            .CustomInstantiator(faker =>
                new CreateCatRequest(
                    Name: faker.Name.FirstName(),
                    IsCastrated: true,
                    MedicalHelpUrgency: MedicalHelpUrgency.NoNeed.Name,
                    Behavior: Behavior.Friendly.Name,
                    HealthStatus: HealthStatus.Good.Name,
                    AgeCategory: AgeCategory.Adult.Name,
                    AdditionalRequirements: "Lorem ipsum"
                ));

    [Fact]
    public async Task CreateAdvertisement_ShouldReturnSuccess_WhenOneCatIsProvided()
    {
        //Arrange
        CreatePersonRequest personRegisterRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", personRegisterRequest);
        IdResponse<PersonId> personId = await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();
        
        CreateCatRequest catCreateRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/cats", catCreateRequest);
        IdResponse<CatId> catId = await catCreateResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<CatId>>();

        //Act
        CreateAdvertisementRequest request =
            new Faker<CreateAdvertisementRequest>()
                .CustomInstantiator(faker =>
                    new CreateAdvertisementRequest(
                        CatsIdsToAssign: [catId],
                        Description: faker.Lorem.Lines(2),
                        PickupAddressCountry: faker.Address.CountryCode(),
                        PickupAddressState: faker.Address.State(),
                        PickupAddressZipCode: faker.Address.ZipCode(),
                        PickupAddressCity: faker.Address.City(),
                        PickupAddressStreet: faker.Address.StreetName(),
                        PickupAddressBuildingNumber: faker.Address.BuildingNumber(),
                        ContactInfoEmail: faker.Person.Email,
                        ContactInfoPhoneNumber: faker.Person.Phone
                    ));

        HttpResponseMessage responseMessage = 
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/advertisements", request);

        //Assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.Created);
        AdvertisementHateoasResponse? hateoasResponse = await responseMessage.Content.ReadFromJsonAsync<AdvertisementHateoasResponse>();
        hateoasResponse.Should().NotBeNull();
        hateoasResponse!.PersonId.Should().Be(personId.Id);
        hateoasResponse.Status.Should().Be(AdvertisementStatus.ThumbnailNotUploaded);
        responseMessage.Headers.Location!.ToString()
            .Should().Contain($"/api/v1/persons/{personId}/advertisements/{hateoasResponse.Id}");
        hateoasResponse.Links.Select(x => x.Rel).Should().BeEquivalentTo(
            EndpointRels.SelfRel,
            EndpointNames.UpdateAdvertisementThumbnail.Rel,
            EndpointNames.UpdateAdvertisement.Rel,
            EndpointNames.DeleteAdvertisement.Rel,
            EndpointNames.ReassignCatsToAdvertisement.Rel,
            EndpointNames.GetAdvertisementCats.Rel);
        hateoasResponse.Links.All(x => !string.IsNullOrWhiteSpace(x.Href)).Should().BeTrue();
    }

    [Fact]
    public async Task CreateAdvertisement_ShouldReturnSuccess_WhenMultipleCatIsProvided()
    {
        //Arrange
        CreatePersonRequest personRegisterRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", personRegisterRequest);
        IdResponse<PersonId> personId = await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();
        
        CreateCatRequest catCreateRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/cats", catCreateRequest);
        IdResponse<CatId> catId = await catCreateResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<CatId>>();
        
        CreateCatRequest secondCatCreateRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage secondCatCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/cats", secondCatCreateRequest);
        IdResponse<CatId> secondCatId = await secondCatCreateResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<CatId>>();

        //Act
        CreateAdvertisementRequest request =
            new Faker<CreateAdvertisementRequest>()
                .CustomInstantiator(faker =>
                    new CreateAdvertisementRequest(
                        CatsIdsToAssign: [catId, secondCatId],
                        Description: faker.Lorem.Lines(2),
                        PickupAddressCountry: faker.Address.CountryCode(),
                        PickupAddressState: faker.Address.State(),
                        PickupAddressZipCode: faker.Address.ZipCode(),
                        PickupAddressCity: faker.Address.City(),
                        PickupAddressStreet: faker.Address.StreetName(),
                        PickupAddressBuildingNumber: faker.Address.BuildingNumber(),
                        ContactInfoEmail: faker.Person.Email,
                        ContactInfoPhoneNumber: faker.Person.Phone
                    ));

        HttpResponseMessage responseMessage = 
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/advertisements", request);
        
        //Assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.Created);
        AdvertisementHateoasResponse? hateoasResponse = await responseMessage.Content.ReadFromJsonAsync<AdvertisementHateoasResponse>();
        hateoasResponse.Should().NotBeNull();
        hateoasResponse!.PersonId.Should().Be(personId.Id);
        hateoasResponse.Status.Should().Be(AdvertisementStatus.ThumbnailNotUploaded);
        responseMessage.Headers.Location!.ToString()
            .Should().Contain($"/api/v1/persons/{personId}/advertisements/{hateoasResponse.Id}");
        hateoasResponse.Links.Select(x => x.Rel).Should().BeEquivalentTo(
            EndpointRels.SelfRel,
            EndpointNames.UpdateAdvertisementThumbnail.Rel,
            EndpointNames.UpdateAdvertisement.Rel,
            EndpointNames.DeleteAdvertisement.Rel,
            EndpointNames.ReassignCatsToAdvertisement.Rel,
            EndpointNames.GetAdvertisementCats.Rel);
        hateoasResponse.Links.All(x => !string.IsNullOrWhiteSpace(x.Href)).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task CreateAdvertisement_ShouldReturnSuccess_WhenValidDataIsProvidedWithoutStateAndDescription(
        string? emptyValue)
    {
        //Arrange
        CreatePersonRequest personRegisterRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", personRegisterRequest);
        IdResponse<PersonId> personId = await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();
        
        CreateCatRequest catCreateRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/cats", catCreateRequest);
        IdResponse<CatId> catId = await catCreateResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<CatId>>();

        //Act
        CreateAdvertisementRequest request =
            new Faker<CreateAdvertisementRequest>()
                .CustomInstantiator(faker =>
                    new CreateAdvertisementRequest(
                        CatsIdsToAssign: [catId],
                        Description: emptyValue,
                        PickupAddressCountry: faker.Address.CountryCode(),
                        PickupAddressState: emptyValue,
                        PickupAddressZipCode: faker.Address.ZipCode(),
                        PickupAddressCity: faker.Address.City(),
                        PickupAddressStreet: faker.Address.StreetName(),
                        PickupAddressBuildingNumber: faker.Address.BuildingNumber(),
                        ContactInfoEmail: faker.Person.Email,
                        ContactInfoPhoneNumber: faker.Person.Phone
                    ));

        HttpResponseMessage responseMessage = await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/advertisements", request);

        //Assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.Created);
        AdvertisementHateoasResponse? hateoasResponse = await responseMessage.Content.ReadFromJsonAsync<AdvertisementHateoasResponse>();
        hateoasResponse.Should().NotBeNull();
        hateoasResponse!.PersonId.Should().Be(personId.Id);
        hateoasResponse.Status.Should().Be(AdvertisementStatus.ThumbnailNotUploaded);
        responseMessage.Headers.Location!.ToString()
            .Should().Contain($"/api/v1/persons/{personId}/advertisements/{hateoasResponse.Id}");
        hateoasResponse.Links.Select(x => x.Rel).Should().BeEquivalentTo(
            EndpointRels.SelfRel,
            EndpointNames.UpdateAdvertisementThumbnail.Rel,
            EndpointNames.UpdateAdvertisement.Rel,
            EndpointNames.DeleteAdvertisement.Rel,
            EndpointNames.ReassignCatsToAdvertisement.Rel,
            EndpointNames.GetAdvertisementCats.Rel);
        hateoasResponse.Links.All(x => !string.IsNullOrWhiteSpace(x.Href)).Should().BeTrue();
    }
    
    [Fact]
    public async Task CreateAdvertisement_ShouldReturnBadRequest_WhenAlreadyAssignedCatIsProvided()
    {
        //Arrange
        CreatePersonRequest personRegisterRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", personRegisterRequest);
        IdResponse<PersonId> personId = await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();
        
        CreateCatRequest catCreateRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/cats", catCreateRequest);
        IdResponse<CatId> catId = await catCreateResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<CatId>>();
        
        CreateAdvertisementRequest request =
            new Faker<CreateAdvertisementRequest>()
                .CustomInstantiator(faker =>
                    new CreateAdvertisementRequest(
                        CatsIdsToAssign: [catId],
                        Description: faker.Lorem.Lines(2),
                        PickupAddressCountry: faker.Address.CountryCode(),
                        PickupAddressState: faker.Address.State(),
                        PickupAddressZipCode: faker.Address.ZipCode(),
                        PickupAddressCity: faker.Address.City(),
                        PickupAddressStreet: faker.Address.StreetName(),
                        PickupAddressBuildingNumber: faker.Address.BuildingNumber(),
                        ContactInfoEmail: faker.Person.Email,
                        ContactInfoPhoneNumber: faker.Person.Phone
                    ));
        await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/advertisements", request);

        //Act
        HttpResponseMessage responseMessage = await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/advertisements", request);

        //Assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateAdvertisement_ShouldReturnBadRequest_WhenTooLongDataAreProvided()
    {
        //Arrange
        CreatePersonRequest personRegisterRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", personRegisterRequest);
        IdResponse<PersonId> personId = await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();
        
        CreateCatRequest catCreateRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/cats", catCreateRequest);
        IdResponse<CatId> catId = await catCreateResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<CatId>>();

        //Act
        CreateAdvertisementRequest request = new(
            CatsIdsToAssign: [catId],
            Description: new string('A', Description.MaxLength + 1),
            PickupAddressCountry: new string('A', Address.CountryMaxLength + 1),
            PickupAddressState: new string('A', Address.StateMaxLength + 1),
            PickupAddressZipCode: new string('A', Address.ZipCodeMaxLength + 1),
            PickupAddressCity: new string('A', Address.CityMaxLength + 1),
            PickupAddressStreet: new string('A', Address.StreetMaxLength + 1),
            PickupAddressBuildingNumber: new string('A', Address.BuildingNumberMaxLength + 1),
            ContactInfoEmail: new string('A', Email.MaxLength + 1),
            ContactInfoPhoneNumber: new string('A', PhoneNumber.MaxLength + 1)
        );

        HttpResponseMessage responseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/advertisements", request);

        //Assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await responseMessage.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(9);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo(
            nameof(CreateAdvertisement.CreateAdvertisementCommand.Description),
            nameof(CreateAdvertisement.CreateAdvertisementCommand.PickupAddressCountry),
            nameof(CreateAdvertisement.CreateAdvertisementCommand.PickupAddressState),
            nameof(CreateAdvertisement.CreateAdvertisementCommand.PickupAddressZipCode),
            nameof(CreateAdvertisement.CreateAdvertisementCommand.PickupAddressCity),
            nameof(CreateAdvertisement.CreateAdvertisementCommand.PickupAddressStreet),
            nameof(CreateAdvertisement.CreateAdvertisementCommand.PickupAddressBuildingNumber),
            nameof(CreateAdvertisement.CreateAdvertisementCommand.ContactInfoEmail),
            nameof(CreateAdvertisement.CreateAdvertisementCommand.ContactInfoPhoneNumber)
        );
        validationProblemDetails.Errors.Values.Count.Should().Be(9);

        validationProblemDetails.Errors[nameof(CreateAdvertisement.CreateAdvertisementCommand.Description)][0]
            .Should()
            .StartWith(
                $"The length of '{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementCommand.Description))}' must be {Description.MaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreateAdvertisement.CreateAdvertisementCommand.PickupAddressCountry)][0]
            .Should()
            .StartWith(
                $"The length of '{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementCommand.PickupAddressCountry))}' must be {Address.CountryMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreateAdvertisement.CreateAdvertisementCommand.PickupAddressState)][0]
            .Should()
            .StartWith(
                $"The length of '{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementCommand.PickupAddressState))}' must be {Address.StateMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreateAdvertisement.CreateAdvertisementCommand.PickupAddressZipCode)][0]
            .Should()
            .StartWith(
                $"The length of '{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementCommand.PickupAddressZipCode))}' must be {Address.ZipCodeMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreateAdvertisement.CreateAdvertisementCommand.PickupAddressCity)][0]
            .Should()
            .StartWith(
                $"The length of '{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementCommand.PickupAddressCity))}' must be {Address.CityMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreateAdvertisement.CreateAdvertisementCommand.PickupAddressStreet)][0]
            .Should()
            .StartWith(
                $"The length of '{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementCommand.PickupAddressStreet))}' must be {Address.StreetMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[
                nameof(CreateAdvertisement.CreateAdvertisementCommand.PickupAddressBuildingNumber)][0]
            .Should()
            .StartWith(
                $"The length of '{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementCommand.PickupAddressBuildingNumber))}' must be {Address.BuildingNumberMaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreateAdvertisementRequest.ContactInfoEmail)][0]
            .Should()
            .StartWith(
                $"The length of '{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementCommand.ContactInfoEmail))}' must be {Email.MaxLength} characters or fewer. You entered");

        validationProblemDetails.Errors[nameof(CreateAdvertisement.CreateAdvertisementCommand.ContactInfoPhoneNumber)][0]
            .Should()
            .StartWith(
                $"The length of '{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementCommand.ContactInfoPhoneNumber))}' must be {PhoneNumber.MaxLength} characters or fewer. You entered");
    }

    [Fact]
    public async Task CreateAdvertisement_ShouldReturnBadRequest_WhenEmptyDataAreProvided()
    {
        //Act
        PersonId emptyId = default;
        CreateAdvertisementRequest request = new(
            CatsIdsToAssign: [],
            Description: "",
            PickupAddressCountry: "",
            PickupAddressState: "",
            PickupAddressZipCode: "",
            PickupAddressCity: "",
            PickupAddressStreet: "",
            PickupAddressBuildingNumber: "",
            ContactInfoEmail: "",
            ContactInfoPhoneNumber: ""
        );

        HttpResponseMessage responseMessage = await _httpClient.PostAsJsonAsync($"api/v1/persons/{emptyId}/advertisements", request);

        //Assert
        responseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblemDetails =
            await responseMessage.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblemDetails.Should().NotBeNull();
        validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationProblemDetails.Errors.Count.Should().Be(7);
        validationProblemDetails.Errors.Keys.Should().BeEquivalentTo(
            nameof(CreateAdvertisement.CreateAdvertisementCommand.PersonId),
            nameof(CreateAdvertisement.CreateAdvertisementCommand.CatsIdsToAssign),
            nameof(CreateAdvertisement.CreateAdvertisementCommand.PickupAddressCountry),
            nameof(CreateAdvertisement.CreateAdvertisementCommand.PickupAddressZipCode),
            nameof(CreateAdvertisement.CreateAdvertisementCommand.PickupAddressCity),
            nameof(CreateAdvertisement.CreateAdvertisementCommand.ContactInfoEmail),
            nameof(CreateAdvertisement.CreateAdvertisementCommand.ContactInfoPhoneNumber)
        );
        validationProblemDetails.Errors.Values.Count.Should().Be(7);

        validationProblemDetails.Errors[nameof(CreateAdvertisement.CreateAdvertisementCommand.PersonId)][0]
            .Should()
            .Be($"'{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementCommand.PersonId))}' must not be empty.");

        validationProblemDetails.Errors[nameof(CreateAdvertisement.CreateAdvertisementCommand.CatsIdsToAssign)][0]
            .Should()
            .Be($"'{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementCommand.CatsIdsToAssign))}' must not be empty.");
        
        validationProblemDetails.Errors[nameof(CreateAdvertisement.CreateAdvertisementCommand.PickupAddressCountry)][0]
            .Should()
            .Be($"'{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementCommand.PickupAddressCountry))}' must not be empty.");
        
        validationProblemDetails.Errors[nameof(CreateAdvertisement.CreateAdvertisementCommand.PickupAddressZipCode)][0]
            .Should()
            .Be($"'{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisementRequest.PickupAddressZipCode))}' must not be empty.");

        validationProblemDetails.Errors[nameof(CreateAdvertisement.CreateAdvertisementCommand.PickupAddressCity)][0]
            .Should()
            .Be($"'{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementCommand.PickupAddressCity))}' must not be empty.");
        
        validationProblemDetails.Errors[nameof(CreateAdvertisement.CreateAdvertisementCommand.ContactInfoEmail)][0]
            .Should()
            .Be($"'{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementCommand.ContactInfoEmail))}' must not be empty.");

        validationProblemDetails.Errors[nameof(CreateAdvertisementRequest.ContactInfoPhoneNumber)][0]
            .Should()
            .Be($"'{Extensions.InsertSpacesIntoCamelCase(nameof(CreateAdvertisement.CreateAdvertisementCommand.ContactInfoPhoneNumber))}' must not be empty.");
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
﻿global using System.Net.Http.Headers;
using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Tests.Integration.Helpers;
using KittySaver.Shared.Common.Enums;
using KittySaver.Shared.Responses;
using KittySaver.Shared.TypedIds;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KittySaver.Api.Tests.Integration.Tests.Advertisements;

[Collection("Api")]
public class GetAdvertisementThumbnailEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CleanupHelper _cleanup;

    public GetAdvertisementThumbnailEndpointsTests(KittySaverApiFactory appFactory)
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
    public async Task GetThumbnail_ShouldReturnSuccess_WhenValidImageIsProvided()
    {
        // Arrange
        (PersonId personId, AdvertisementId advertisementId) = await CreateTestAdvertisement();
        
        await using Stream imageStream = CreateTestImage();
        using MultipartFormDataContent content = new();
        StreamContent imageContent = new(imageStream);
        imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        content.Add(imageContent, "thumbnail", "test.jpg");
        await _httpClient.PutAsync(
            $"/api/v1/persons/{personId}/advertisements/{advertisementId}/thumbnail", 
            content);

        // Act
        var getResponse = await _httpClient.GetAsync($"/api/v1/advertisements/{advertisementId}/thumbnail");
        
        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    

    [Fact]
    public async Task GetThumbnail_ShouldReturnNotFound_WhenAdvertisementDoesNotExist()
    {
        // Act
        AdvertisementId randomId = AdvertisementId.New();
        HttpResponseMessage response = await _httpClient.GetAsync($"/api/v1/advertisements/{randomId}/thumbnail");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails? problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
    }
    
    [Fact]
    public async Task UpdateThumbnail_ShouldReturnBadRequest_WhenTheSameValueIsProvidedForPersonIdAndId()
    {
        // Arrange
        (_, AdvertisementId advertisementId) = await CreateTestAdvertisement();
        // Act
        HttpResponseMessage response = await _httpClient.GetAsync($"/api/v1/advertisements/{advertisementId}/thumbnail");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ProblemDetails? problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Detail.Should().Be("Thumbnail is not uploaded");
    }

    private async Task<(PersonId PersonId, AdvertisementId AdvertisementId)> CreateTestAdvertisement()
    {
        CreatePersonRequest? createPersonRequest = _createPersonRequestGenerator.Generate();
        HttpResponseMessage personResponse = await _httpClient.PostAsJsonAsync("/api/v1/persons", createPersonRequest);
        IdResponse<PersonId> personId = await personResponse.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();
        
        CreateCatRequest? createCatRequest = _createCatRequestGenerator.Generate();
        HttpResponseMessage catResponse = await _httpClient.PostAsJsonAsync(
            $"/api/v1/persons/{personId}/cats", 
            createCatRequest);
        IdResponse<CatId> catId = await catResponse.GetIdResponseFromResponseMessageAsync<IdResponse<CatId>>();
        
        CreateAdvertisementRequest? createAdvertisementRequest =
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
                    )).Generate();

        HttpResponseMessage advertisementResponse = await _httpClient.PostAsJsonAsync(
            $"/api/v1/persons/{personId}/advertisements",
            createAdvertisementRequest);
        
        IdResponse<AdvertisementId> advertisementId = await advertisementResponse.GetIdResponseFromResponseMessageAsync<IdResponse<AdvertisementId>>();

        return (personId, advertisementId);
    }

    private static MemoryStream CreateTestImage(int width = 100, int height = 100)
    {
        MemoryStream stream = new();
        byte[] bytes = new byte[width * height * 3]; // RGB data
        new Random().NextBytes(bytes);
        stream.Write(bytes, 0, bytes.Length);
        stream.Position = 0;
        return stream;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _cleanup.Cleanup();
    }
}
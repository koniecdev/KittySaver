using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using FluentAssertions;
using KittySaver.Api.Infrastructure.Endpoints;
using KittySaver.Api.Tests.Integration.Helpers;
using KittySaver.Shared.Common.Enums;
using KittySaver.Shared.Hateoas;
using KittySaver.Shared.Responses;
using KittySaver.Shared.TypedIds;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using KittySaver.Tests.Shared;

namespace KittySaver.Api.Tests.Integration.Tests.Cats;

[Collection("Api")]
public class GetCatsEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly CleanupHelper _cleanup;

    public GetCatsEndpointsTests(KittySaverApiFactory appFactory)
    {
        _httpClient = appFactory.CreateClient();
        _cleanup = new CleanupHelper(_httpClient);
    }

    private readonly CreatePersonRequest _createPersonRequest =
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
                )).Generate();

    private readonly CreateCatRequest _createCatRequest =
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
                )).Generate();

    [Fact]
    public async Task GetCats_ShouldReturnCat_WhenCatExist()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        IdResponse<PersonId> personId = await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();
        
        HttpResponseMessage catCreateResponseMessage =
            await _httpClient.PostAsJsonAsync($"api/v1/persons/{personId}/cats", _createCatRequest);
        IdResponse<CatId> catId = await catCreateResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<CatId>>();

        //Act
        HttpResponseMessage response = await _httpClient.GetAsync($"/api/v1/persons/{personId}/cats");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PagedList<CatResponse>? cats = await response.Content.ReadFromJsonAsync<PagedList<CatResponse>>();
        cats.Should().NotBeNull();
        cats!.Items.Count.Should().BeGreaterThan(0);
        cats.Total.Should().Be(1);
        cats.Links.Count.Should().Be(2);
        cats.Links.Count(x => x.Rel == EndpointRels.SelfRel).Should().Be(1);
        cats.Links.First(x => x.Rel == "by-page").Href.Should().Contain("://");
        
        CatResponse cat = cats.Items.First();
        cat.Id.Should().Be(catId);
        cat.Name.Should().Be(_createCatRequest.Name);
        cat.AdditionalRequirements.Should().Be(_createCatRequest.AdditionalRequirements);
        cat.Behavior.Should().Be(_createCatRequest.Behavior);
        cat.AgeCategory.Should().Be(_createCatRequest.AgeCategory);
        cat.MedicalHelpUrgency.Should().Be(_createCatRequest.MedicalHelpUrgency);
        cat.HealthStatus.Should().Be(_createCatRequest.HealthStatus);
        cat.IsCastrated.Should().Be(_createCatRequest.IsCastrated);
        cat.PriorityScore.Should().BeGreaterThan(0);
        cat.Links.Select(x => x.Rel).Should()
            .BeEquivalentTo(EndpointRels.SelfRel,
                EndpointNames.UpdateCat.Rel,
                EndpointNames.DeleteCat.Rel,
                EndpointNames.UpdateCatThumbnail.Rel,
                EndpointNames.GetCatGallery.Rel,
                EndpointNames.AddPicturesToCatGallery.Rel,
                EndpointNames.RemovePictureFromCatGallery.Rel,
                EndpointNames.GetCatGalleryPicture.Rel);
        cat.Links.Select(x => x.Href).All(x => x.Contains("://")).Should().BeTrue();
    }

    [Fact]
    public async Task GetCats_ShouldReturnEmptyList_WhenNoCatsExist()
    {
        //Arrange
        HttpResponseMessage personRegisterResponseMessage =
            await _httpClient.PostAsJsonAsync("api/v1/persons", _createPersonRequest);
        IdResponse<PersonId> personId = await personRegisterResponseMessage.GetIdResponseFromResponseMessageAsync<IdResponse<PersonId>>();

        //Act
        HttpResponseMessage response = await _httpClient.GetAsync($"/api/v1/persons/{personId}/cats");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PagedList<CatResponse>? cats = await response.Content.ReadFromJsonAsync<PagedList<CatResponse>>();
        cats.Should().NotBeNull();
        cats!.Total.Should().Be(0);
        cats.Links.Count.Should().Be(2);
        cats.Links.First(x => x.Rel == EndpointRels.SelfRel).Href.Should().Contain("://");
        cats.Links.First(x => x.Rel == "by-page").Href.Should().Contain("://");
        cats.Items.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetCats_ShouldReturnNotFound_WhenNonExistingPersonIsProvided()
    {
        //Arrange
        PersonId randomPersonId = PersonId.New();
        
        //Act
        HttpResponseMessage response = await _httpClient.GetAsync($"api/v1/persons/{randomPersonId}/cats");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails? problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
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
// using System.Net;
// using System.Net.Http.Json;
// using System.Text.Json;
// using Bogus;
// using FluentAssertions;
// using KittySaver.Auth.Api.Features.ApplicationUsers;
// using KittySaver.Auth.Api.Features.ApplicationUsers.SharedContracts;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Shared;
//
// namespace KittySaver.Auth.Api.Tests.Integration.Tests.ApplicationUser;
//
// [Collection("AuthApi")]
// public class UpdateApplicationUserEndpointTests(KittySaverAuthApiFactory appFactory)
// {
//     private readonly HttpClient _httpClient = appFactory.CreateClient();
//
//     private readonly Faker<Register.RegisterRequest> _createApplicationUserRequestGenerator =
//         new Faker<Register.RegisterRequest>()
//             .CustomInstantiator(faker =>
//                 new Register.RegisterRequest(
//                     UserName: faker.Person.FirstName,
//                     Email: faker.Person.Email,
//                     PhoneNumber: faker.Person.Phone,
//                     Password: "Default1234%"
//                 ));
//
//     [Fact]
//     public async Task UpdateApplicationUser_ShouldSuccessfullyUpdate_WhenValidDataIsProvided()
//     {
//         //Arrange
//         Register.RegisterRequest registerRequest = _createApplicationUserRequestGenerator.Generate();
//         HttpResponseMessage registerResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/application-users/register", registerRequest);
//         ApiResponses.CreatedWithIdResponse registerResponse =
//             await registerResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
//             ?? throw new JsonException();
//         
//         //Act
//         UpdateApplicationUser.UpdateApplicationUserRequest request = new Faker<UpdateApplicationUser.UpdateApplicationUserRequest>()
//             .CustomInstantiator(faker =>
//                 new UpdateApplicationUser.UpdateApplicationUserRequest(
//                     UserName: faker.Person.FirstName
//                 ));
//         HttpResponseMessage updateResponseMessage =
//             await _httpClient.PutAsJsonAsync($"api/v1/application-users/{registerResponse.Id}", request);
//
//         //Assert
//         updateResponseMessage.StatusCode.Should().Be(HttpStatusCode.NoContent);
//     }
//     
//     [Fact]
//     public async Task UpdateApplicationUser_ShouldReturnBadRequest_WhenEmptyDataIsProvided()
//     {
//         //Arrange
//         Register.RegisterRequest registerRequest = _createApplicationUserRequestGenerator.Generate();
//         HttpResponseMessage registerResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/application-users/register", registerRequest);
//         ApiResponses.CreatedWithIdResponse registerResponse =
//             await registerResponseMessage.Content.ReadFromJsonAsync<ApiResponses.CreatedWithIdResponse>()
//             ?? throw new JsonException();
//     
//         //Act
//         UpdateApplicationUser.UpdateApplicationUserRequest request = new(
//             UserName: ""
//         );
//         HttpResponseMessage updateResponseMessage =
//             await _httpClient.PutAsJsonAsync($"api/v1/application-users/{registerResponse.Id}", request);
//     
//         //Assert
//         updateResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
//         ValidationProblemDetails? validationProblemDetails =
//             await updateResponseMessage.Content.ReadFromJsonAsync<ValidationProblemDetails>();
//         validationProblemDetails.Should().NotBeNull();
//         validationProblemDetails!.Status.Should().Be(StatusCodes.Status400BadRequest);
//         validationProblemDetails.Errors["UserName"][0].Should().Be("'User Name' must not be empty.");
//         
//         validationProblemDetails.Errors[
//                 nameof(UpdateApplicationUser.UpdateApplicationUserRequest.UserName)][0]
//             .Should()
//             .Be("'User Name' must not be empty.");
//     }
// }
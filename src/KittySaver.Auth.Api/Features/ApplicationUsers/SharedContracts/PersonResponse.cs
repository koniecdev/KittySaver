namespace KittySaver.Auth.Api.Features.ApplicationUsers.SharedContracts;

public sealed class ApplicationUserResponse
{
    public required Guid Id { get; init; }
    public required string FullName { get; init; }
    public required string Email { get; init; }
    public required string PhoneNumber { get; init; }
}
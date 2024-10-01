using KittySaver.Auth.Api.Shared.Domain.Entites;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Auth.Api.Features.ApplicationUsers.SharedContracts;

public sealed class ApplicationUserResponse
{
    public required Guid Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string FullName { get; init; }
    public required string Email { get; init; }
    public required string PhoneNumber { get; init; }
}

[Mapper]
public static partial class ApplicationUsersMapper
{
    public static partial IQueryable<ApplicationUserResponse> ProjectToDto(
        this IQueryable<ApplicationUser> applicationUsers);
}
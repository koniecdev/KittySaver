using KittySaver.Auth.Api.Shared.Domain.Entites;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Auth.Api.Features.ApplicationUsers.SharedContracts;

public sealed class ApplicationUserResponse
{
    public required Guid Id { get; init; }
    public required string UserName { get; init; }
    public required string Email { get; init; }
    public required string PhoneNumber { get; init; }
    public required string DefaultAdvertisementPickupAddressCountry { get; init; }
    public required string DefaultAdvertisementPickupAddressState { get; init; }
    public required string DefaultAdvertisementPickupAddressZipCode { get; init; }
    public required string DefaultAdvertisementPickupAddressCity { get; init; }
    public required string DefaultAdvertisementPickupAddressStreet { get; init; }
    public required string DefaultAdvertisementPickupAddressBuildingNumber { get; init; }
    public required string DefaultAdvertisementContactInfoEmail { get; init; }
    public required string DefaultAdvertisementContactInfoPhoneNumber { get; init; }
}

[Mapper]
public static partial class ApplicationUsersMapper
{
    public static partial IQueryable<ApplicationUserResponse> ProjectToDto(
        this IQueryable<ApplicationUser> applicationUsers);
}
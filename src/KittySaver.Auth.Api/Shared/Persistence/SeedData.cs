using KittySaver.Auth.Api.Shared.Domain.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.Auth.Api.Shared.Persistence;

public static class FixedIdsHelper
{
    public static Guid AdminRoleId { get; } = Guid.Parse("b1a6bd96-90d6-4949-b618-ce662f41f87a");
    public static Guid ShelterRoleId { get; } = Guid.Parse("86c81ae7-b11c-4343-9577-872a74637200");
    public static Guid AdminId { get; } = Guid.Parse("c4ff57f4-2d8c-4c66-b62d-333ad2e3c424");
    public static Guid AdminStamp { get; } = Guid.Parse("34d18c72-0a52-4bbe-9b00-754a02e2293e");
}
public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole<Guid>>
{
    
    public void Configure(EntityTypeBuilder<IdentityRole<Guid>> builder)
    {
        builder.HasData(
            new IdentityRole<Guid>
            {
                Id = FixedIdsHelper.AdminRoleId,
                Name = "Administrator",
                NormalizedName = "ADMINISTRATOR"
            },
            new IdentityRole<Guid>
            {
                Id = FixedIdsHelper.ShelterRoleId,
                Name = "Shelter",
                NormalizedName = "Shelter"
            }
        );
    }
}

public class AdminConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        ApplicationUser admin = new()
        {
            Id = FixedIdsHelper.AdminId,
            NormalizedUserName = "DEFAULTADMIN@KONIEC.DEV",
            UserName = "DefaultAdmin",
            Email = "defaultadmin@koniec.dev",
            NormalizedEmail = "DEFAULTADMIN@KONIEC.DEV",
            PhoneNumber = "XXXXXXXXX",
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            SecurityStamp = FixedIdsHelper.AdminStamp.ToString(),
            ConcurrencyStamp = "f581a969-a6e1-44c7-bc00-630b94556ab9",
            PasswordHash = "AQAAAAIAAYagAAAAELDnCnSS1y6NXPsgFQyW6+5hDyDz/sX9Kfj4phlvmD9W+xoZOgBCzSIENV2vAnJDNA==",
            DefaultAdvertisementPickupAddressCountry = "DefaultCountry",
            DefaultAdvertisementPickupAddressState = "DefaultState",
            DefaultAdvertisementPickupAddressZipCode = "00000",
            DefaultAdvertisementPickupAddressCity = "DefaultCity",
            DefaultAdvertisementPickupAddressStreet = "DefaultStreet",
            DefaultAdvertisementPickupAddressBuildingNumber = "1",
            DefaultAdvertisementContactInfoEmail = "defaultadmin@koniec.dev",
            DefaultAdvertisementContactInfoPhoneNumber = "XXXXXXXXX"
        };

        builder.HasData(admin);
    }

    private static string PassGenerate(ApplicationUser user)
    {
        PasswordHasher<ApplicationUser> passHash = new();
        return passHash.HashPassword(user, "DefaultPassword123!");
    }
}

public class UsersWithRolesConfiguration : IEntityTypeConfiguration<IdentityUserRole<Guid>>
{
    private readonly Guid _adminId = FixedIdsHelper.AdminId;
    private readonly Guid _adminRoleId = FixedIdsHelper.AdminRoleId;

    public void Configure(EntityTypeBuilder<IdentityUserRole<Guid>> builder)
    {
        IdentityUserRole<Guid> iur = new IdentityUserRole<Guid>
        {
            RoleId = _adminRoleId,
            UserId = _adminId
        };

        builder.HasData(iur);
    }
}
using KittySaver.Auth.Api.Shared.Domain.Entites;
using KittySaver.SharedForApi;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.Auth.Api.Shared.Persistence;

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
            PasswordHash = "AQAAAAIAAYagAAAAELDnCnSS1y6NXPsgFQyW6+5hDyDz/sX9Kfj4phlvmD9W+xoZOgBCzSIENV2vAnJDNA=="
        };

        builder.HasData(admin);
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
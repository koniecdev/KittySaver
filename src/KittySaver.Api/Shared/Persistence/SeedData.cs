using KittySaver.Api.Shared.Domain.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.Api.Shared.Persistence;

public static class FixedIdsHelper
{
    public static Guid AdminRoleId { get; } = Guid.Parse("b1a6bd96-90d6-4949-b618-ce662f41f87a");
    public static Guid ShelterRoleId { get; } = Guid.Parse("86c81ae7-b11c-4343-9577-872a74637200");
    public static Guid AdminId { get; } = Guid.Parse("c4ff57f4-2d8c-4c66-b62d-333ad2e3c424");
    public static Guid AdminUserIdentityId { get; } = Guid.Parse("a4018ea1-525a-48eb-a701-a96c1a261e72");
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

public class AdminConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        Person admin = new()
        {
            Id = FixedIdsHelper.AdminId,
            UserName = "defaultadmin@koniec.dev",
            NormalizedUserName = "DEFAULTADMIN@KONIEC.DEV",
            FirstName = "Default",
            LastName = "Admin",
            Email = "defaultadmin@koniec.dev",
            NormalizedEmail = "DEFAULTADMIN@KONIEC.DEV",
            PhoneNumber = "XXXXXXXXX",
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            SecurityStamp = FixedIdsHelper.AdminStamp.ToString(),
            UserIdentityId = FixedIdsHelper.AdminUserIdentityId
        };

        admin.PasswordHash = PassGenerate(admin);

        builder.HasData(admin);
    }

    private static string PassGenerate(Person user)
    {
        PasswordHasher<Person> passHash = new();
        return passHash.HashPassword(user, "DefaultPassword123!");
    }
}
//
// public class UsersWithRolesConfiguration : IEntityTypeConfiguration<IdentityUserRole<Guid>>
// {
//     private readonly Guid _adminId = FixedIdsHelper.AdminId;
//     private readonly Guid _adminRoleId = FixedIdsHelper.AdminRoleId;
//
//     public void Configure(EntityTypeBuilder<IdentityUserRole<Guid>> builder)
//     {
//         IdentityUserRole<Guid> iur = new IdentityUserRole<Guid>
//         {
//             RoleId = _adminRoleId,
//             UserId = _adminId
//         };
//
//         builder.HasData(iur);
//     }
// }
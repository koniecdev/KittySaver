using KittySaver.Api.Shared.Domain.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KittySaver.Api.Shared.Persistence;

public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole<Guid>>
{
    private readonly Guid _adminId = Guid.Parse("b1a6bd96-90d6-4949-b618-ce662f41f87a");
    private readonly Guid _shelterId = Guid.Parse("86c81ae7-b11c-4343-9577-872a74637200");
    
    public void Configure(EntityTypeBuilder<IdentityRole<Guid>> builder)
    {
        builder.HasData(
            new IdentityRole<Guid>
            {
                Id = _adminId,
                Name = "Administrator",
                NormalizedName = "ADMINISTRATOR"
            },
            new IdentityRole<Guid>
            {
                Id = _shelterId,
                Name = "Shelter",
                NormalizedName = "Shelter"
            }
        );
    }
}

public class AdminConfiguration(IConfiguration configuration) : IEntityTypeConfiguration<Person>
{
    private readonly Guid _adminId = Guid.Parse("c4ff57f4-2d8c-4c66-b62d-333ad2e3c424");

    public void Configure(EntityTypeBuilder<Person> builder)
    {
        Person admin = new()
        {
            Id = _adminId,
            UserName = "defaultadmin@koniec.dev",
            NormalizedUserName = "DEFAULTADMIN@KONIEC.DEV",
            FirstName = "Default",
            LastName = "Admin",
            Email = "defaultadmin@koniec.dev",
            NormalizedEmail = "DEFAULTADMIN@KONIEC.DEV",
            PhoneNumber = "XXXXXXXXX",
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            SecurityStamp = new Guid().ToString("D"),
            UserId = Guid.Parse("c35f84aa-9c22-4567-a529-174789e02a4c")
        };

        admin.PasswordHash = PassGenerate(admin);

        builder.HasData(admin);
    }

    private string PassGenerate(Person user)
    {
        PasswordHasher<Person> passHash = new();
        return passHash.HashPassword(user, configuration["Data:DefaultAdminPassword"]!);
    }
}

public class UsersWithRolesConfiguration : IEntityTypeConfiguration<IdentityUserRole<Guid>>
{
    private readonly Guid _adminUserId = Guid.Parse("c4ff57f4-2d8c-4c66-b62d-333ad2e3c424");
    private readonly Guid _adminRoleId = Guid.Parse("b1a6bd96-90d6-4949-b618-ce662f41f87a");

    public void Configure(EntityTypeBuilder<IdentityUserRole<Guid>> builder)
    {
        IdentityUserRole<Guid> iur = new IdentityUserRole<Guid>
        {
            RoleId = _adminRoleId,
            UserId = _adminUserId
        };

        builder.HasData(iur);
    }
}
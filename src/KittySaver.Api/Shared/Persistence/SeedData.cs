// using KittySaver.Api.Shared.Domain.Entites;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Metadata.Builders;
//
// namespace KittySaver.Api.Shared.Persistence;
//
// public static class FixedIdsHelper
// {
//     public static Guid AdminId { get; } = Guid.Parse("a4018ea1-525a-48eb-a701-a96c1a261e72");
//     public const string AdminEmail = "defaultadmin@koniec.dev";
//     public const string AdminPhone = "XXXXXXXXX";
//     public static Guid AdminUserIdentityId { get; } = Guid.Parse("c4ff57f4-2d8c-4c66-b62d-333ad2e3c424");
// }
//
// public class AdminConfiguration : IEntityTypeConfiguration<Person>
// {
//     public void Configure(EntityTypeBuilder<Person> builder)
//     {
//         Person admin = new()
//         {
//             Id = FixedIdsHelper.AdminId,
//             FirstName = "Default",
//             LastName = "Admin",
//             Email = "defaultadmin@koniec.dev",
//             PhoneNumber = "XXXXXXXXX",
//             CreatedOn = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
//             UserIdentityId = FixedIdsHelper.AdminUserIdentityId
//         };
//         typeof(Person).GetProperty(nameof(Person.CurrentRole))?.SetValue(admin, Person.Role.Admin, null);
//         builder.HasData(admin);
//     }
// }
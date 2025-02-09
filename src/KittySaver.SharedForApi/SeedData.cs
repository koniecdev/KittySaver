namespace KittySaver.SharedForApi;

public static class FixedIdsHelper
{
    public static Guid AdminRoleId { get; } = Guid.Parse("b1a6bd96-90d6-4949-b618-ce662f41f87a");
    public static Guid ShelterRoleId { get; } = Guid.Parse("86c81ae7-b11c-4343-9577-872a74637200");
    public static Guid AdminId { get; } = Guid.Parse("c4ff57f4-2d8c-4c66-b62d-333ad2e3c424");
    public static Guid AdminStamp { get; } = Guid.Parse("34d18c72-0a52-4bbe-9b00-754a02e2293e");
}

namespace KittySaver.Shared.Responses;

public class LoginResponse
{
    public required string AccessToken { get; set; }
    public required DateTimeOffset AccessTokenExpiresAt { get; set; }
    public string? RefreshToken { get; set; }
    public DateTimeOffset? RefreshTokenExpiresAt { get; set; }
}

public class UserStatusResponse
{
    public Guid Id { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public bool EmailConfirmed { get; set; }
    public List<string> Roles { get; set; } = [];
}
namespace KittySaver.Wasm.Shared.Auth;

public sealed class LoginResponse
{
    public required string AccessToken { get; init; }
    public required DateTimeOffset AccessTokenExpiresAt { get; init; }
}
namespace KittySaver.Shared.Requests;

public sealed record LoginRequest(
    string Email,
    string Password);
    
public sealed record RegisterRequest(
    string UserName,
    string Email,
    string PhoneNumber,
    string Password);
    
public sealed record RefreshTokenRequest(string AccessToken, string RefreshToken);

public sealed record ConfirmEmailRequest(string UserId, string Token);

public sealed record ForgotPasswordRequest(string Email);
public sealed record ResetPasswordRequest(string Email, string Token, string Password);
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public sealed record LogoutRequest(string RefreshToken);
public sealed record ResendEmailConfirmationRequest(string Email);
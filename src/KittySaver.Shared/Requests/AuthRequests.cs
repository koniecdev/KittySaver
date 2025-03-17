namespace KittySaver.Shared.Requests;

public sealed record LoginRequest(
    string Email,
    string Password);
    
public sealed record RegisterRequest(
    string UserName,
    string Email,
    string PhoneNumber,
    string Password);
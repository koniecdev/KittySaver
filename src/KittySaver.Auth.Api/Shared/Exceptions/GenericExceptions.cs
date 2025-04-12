namespace KittySaver.Auth.Api.Shared.Exceptions;

public abstract class NotFoundException(string entity, string identifier) 
    : Exception($"'{entity}' with identifier '{identifier}' was not found.");

// Możesz dodać konkretne typy wyjątków Not Found tutaj w razie potrzeby
public static class NotFoundExceptions
{
    public sealed class ApplicationUserNotFoundException() 
        : NotFoundException("ApplicationUser", "unknown");
        
    public sealed class RefreshTokenNotFoundException() 
        : NotFoundException("RefreshToken", "unknown");
}
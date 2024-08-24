using Microsoft.AspNetCore.Identity;

namespace KittySaver.Auth.Api.Shared.Exceptions;

public interface IApplicationError
{
    string ApplicationCode { get; }
    string Description { get; }
}

public class NotFoundException(string applicationCode, string description) : Exception, IApplicationError
{
    public string ApplicationCode { get; } = applicationCode;
    public string Description { get; } = description;
}

public class BadRequestException(string applicationCode, string description) : Exception, IApplicationError
{
    public string ApplicationCode { get; } = applicationCode;
    public string Description { get; } = description;
}

public class IdentityResultException(IEnumerable<IdentityError> identityErrors) : Exception
{
    public IEnumerable<IdentityError> IdentityErrors { get; } = identityErrors;
}
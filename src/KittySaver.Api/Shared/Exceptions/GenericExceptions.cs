namespace KittySaver.Api.Shared.Exceptions;

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
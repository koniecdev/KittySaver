// ReSharper disable ConvertToPrimaryConstructor
namespace KittySaver.Api.Shared.Exceptions;

public interface IApplicationException
{
    public string ApplicationCode { get; }
    public string Message { get; }
}

public abstract class NotFoundException : Exception, IApplicationException
{
    protected NotFoundException(string applicationCode, string message) : base(message)
    {
        ApplicationCode = applicationCode;
    }

    public string ApplicationCode { get; }
}
public abstract class DomainException : Exception, IApplicationException
{
    protected DomainException(string applicationCode, string message) : base(message)
    {
        ApplicationCode = applicationCode;
    }

    public string ApplicationCode { get; }
}
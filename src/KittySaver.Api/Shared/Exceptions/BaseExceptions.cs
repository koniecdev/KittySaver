// ReSharper disable ConvertToPrimaryConstructor
namespace KittySaver.Api.Shared.Exceptions;

public interface IApplicationException
{
    public string ApplicationCode { get; }
    public string Message { get; }
}


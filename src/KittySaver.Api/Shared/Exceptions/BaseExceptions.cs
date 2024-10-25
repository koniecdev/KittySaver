// ReSharper disable ConvertToPrimaryConstructor
namespace KittySaver.Api.Shared.Exceptions;

public abstract class NotFoundException : Exception
{
    protected NotFoundException(string entity, string identifier) : base($"'{entity}' with identifier '{identifier}' was not found.")
    {
    }
}

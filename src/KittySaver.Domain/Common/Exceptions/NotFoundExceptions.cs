using KittySaver.Domain.Persons;

namespace KittySaver.Domain.Common.Exceptions;

public abstract class NotFoundException(string entity, string identifier)
    : Exception($"'{entity}' with identifier '{identifier}' was not found.");

public static class NotFoundExceptions
{
    public sealed class PersonNotFoundException(Guid id) : NotFoundException(nameof(Person), id.ToString());
    public sealed class CatNotFoundException(Guid id) : NotFoundException(nameof(Cat), id.ToString());
    public sealed class AdvertisementNotFoundException(Guid id) : NotFoundException(nameof(Advertisement), id.ToString());
}
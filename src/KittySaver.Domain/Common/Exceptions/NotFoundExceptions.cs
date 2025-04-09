using KittySaver.Domain.Persons.Entities;
using KittySaver.Shared.TypedIds;

namespace KittySaver.Domain.Common.Exceptions;

public abstract class NotFoundException(string entity, string identifier)
    : Exception($"'{entity}' with identifier '{identifier}' was not found.");

public static class NotFoundExceptions
{
    public sealed class PersonNotFoundException(PersonId id) : NotFoundException(nameof(Person), id.ToString());
    public sealed class CatNotFoundException(CatId id) : NotFoundException(nameof(Cat), id.ToString());
    public sealed class AdvertisementNotFoundException(AdvertisementId id) : NotFoundException(nameof(Advertisement), id.ToString());
    public sealed class FileNotFoundException(string filename) : NotFoundException("File", filename);
}
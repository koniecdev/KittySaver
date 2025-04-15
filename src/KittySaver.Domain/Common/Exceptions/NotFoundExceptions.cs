using KittySaver.Domain.Persons.Entities;
using KittySaver.Shared.TypedIds;

namespace KittySaver.Domain.Common.Exceptions;

public abstract class NotFoundException(string entity, string identifier, bool shouldBeInPolish)
    : Exception(shouldBeInPolish 
        ? $"Nie znaleziono zasobu '{entity}' z identyfikatorem '{identifier}'" 
        : $"'{entity}' with identifier '{identifier}' was not found.");
public class NotFoundException<TId>(string entity, TId identifier)
    : NotFoundException(entity, identifier.ToString() ?? "", true)
    where TId : notnull;

public static class NotFoundExceptions
{
    private const bool ShouldBeInPolish = true;
    public sealed class PersonNotFoundException(PersonId id) 
        : NotFoundException(ShouldBeInPolish ? "Użytkownik" : nameof(Person), id.ToString(), ShouldBeInPolish);
    public sealed class CatNotFoundException(CatId id) 
        : NotFoundException(ShouldBeInPolish ? "Kot" : nameof(Cat), id.ToString(), ShouldBeInPolish);
    public sealed class AdvertisementNotFoundException(AdvertisementId id) 
        : NotFoundException(ShouldBeInPolish ? "Ogłoszenie" : nameof(Advertisement), id.ToString(), ShouldBeInPolish);
    public sealed class FileNotFoundException(string filename) 
        : NotFoundException(ShouldBeInPolish ? "Plik" : "File", filename, ShouldBeInPolish);
}
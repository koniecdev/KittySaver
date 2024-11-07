using KittySaver.Api.Shared.Domain.Entites;

namespace KittySaver.Api.Shared.Exceptions.ApplicationExceptions;

public static class NotFoundExceptions
{
    public sealed class PersonNotFoundException(Guid id) : NotFoundException(nameof(Person), id.ToString());
    public sealed class CatNotFoundException(Guid id) : NotFoundException(nameof(Cat), id.ToString());
    public sealed class AdvertisementNotFoundException(Guid id) : NotFoundException(nameof(Advertisement), id.ToString());
}
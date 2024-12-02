using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Persons;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Shared.Persistence.Repositories;

public class CatRepository : ICatRepository
{
    public IEnumerable<Cat> GetAllCats(Person owner)
        => owner.Cats;

    public Cat GetCatById(Person owner, Guid id)
        => owner.Cats
               .FirstOrDefault(cat => cat.Id == id)
           ?? throw new NotFoundExceptions.CatNotFoundException(id);
}
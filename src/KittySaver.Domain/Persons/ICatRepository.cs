namespace KittySaver.Domain.Persons;

public interface ICatRepository
{
    public IEnumerable<Cat> GetAllCats(Person owner);
    public Cat GetCatById(Person owner, Guid id);
}
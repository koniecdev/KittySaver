using KittySaver.Domain.Persons;

namespace KittySaver.Domain.Advertisements;

public interface IAdvertisementRepository
{
    public Task<Advertisement> GetAdvertisementByIdAsync(Guid id, CancellationToken cancellationToken);
    public void Insert(Advertisement advertisement);
    public void Remove(Advertisement advertisement);
}
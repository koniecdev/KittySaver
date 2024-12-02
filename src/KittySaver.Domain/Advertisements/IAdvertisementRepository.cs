namespace KittySaver.Domain.Advertisements;

public interface IAdvertisementRepository
{
    public Task<IEnumerable<Advertisement>> GetAllAdvertisementsAsync();
    public Task<Advertisement> GetAdvertisementByIdAsync(Guid id);
}
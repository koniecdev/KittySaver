using KittySaver.Wasm.Shared.HttpClients.MainApiResponses;

namespace KittySaver.Wasm.Shared.Components.Advertisement;

public interface IAdvertisementStateService
{
    Link? GetSelectedAdvertisementLink(Guid id);
    void SetSelectedAdvertisementLink(Guid id, Link link);
    void ClearSelectedAdvertisementLink(Guid id);
}

public class AdvertisementStateService : IAdvertisementStateService
{
    private readonly Dictionary<Guid, Link> _selectedAdvertisementLinks = new();

    public Link? GetSelectedAdvertisementLink(Guid id)
    {
        return _selectedAdvertisementLinks.GetValueOrDefault(id);
    }

    public void SetSelectedAdvertisementLink(Guid id, Link link)
    {
        _selectedAdvertisementLinks[id] = link;
    }

    public void ClearSelectedAdvertisementLink(Guid id)
    {
        _selectedAdvertisementLinks.Remove(id);
    }
}
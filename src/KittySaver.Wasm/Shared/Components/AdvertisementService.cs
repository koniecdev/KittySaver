using KittySaver.Shared.Hateoas;
using KittySaver.Shared.TypedIds;

namespace KittySaver.Wasm.Shared.Components;

public interface IAdvertisementStateService
{
    Link? GetSelectedAdvertisementLink(AdvertisementId id);
    void SetSelectedAdvertisementLink(AdvertisementId id, Link link);
    void ClearSelectedAdvertisementLink(AdvertisementId id);
}

public class AdvertisementStateService : IAdvertisementStateService
{
    private readonly Dictionary<AdvertisementId, Link> _selectedAdvertisementLinks = new();

    public Link? GetSelectedAdvertisementLink(AdvertisementId id)
    {
        return _selectedAdvertisementLinks.GetValueOrDefault(id);
    }

    public void SetSelectedAdvertisementLink(AdvertisementId id, Link link)
    {
        _selectedAdvertisementLinks[id] = link;
    }

    public void ClearSelectedAdvertisementLink(AdvertisementId id)
    {
        _selectedAdvertisementLinks.Remove(id);
    }
}
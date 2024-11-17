using KittySaver.Api.Shared.Domain.Persons;

namespace KittySaver.Api.Shared.Domain.Advertisement;

public sealed class AdvertisementService
{
    private readonly Advertisement _advertisement;
    private readonly Person _person;

    private AdvertisementService(Advertisement advertisement, Person person)
    {
        _advertisement = advertisement;
        _person = person;
    }

    public static AdvertisementService Create(Advertisement advertisement, Person person)
    {
        if (advertisement.PersonId != person.Id)
        {
            throw new InvalidOperationException($"Person with id '{person.Id}' do not have permission to alter Advertisement with id '{advertisement.Id}'");
        }

        AdvertisementService advertisementService = new(advertisement, person);
        return advertisementService;
    }
    
    public void RecalculatePriorityScore()
    {
        IEnumerable<Guid> catsAssignedToAdvertisement = _person.Cats
            .Where(x => x.AdvertisementId == _advertisement.Id)
            .Select(x=>x.Id);
        
        double catsToAssignToAdvertisementHighestPriorityScore = _person.GetHighestPriorityScoreFromGivenCats(catsAssignedToAdvertisement);
        _advertisement.PriorityScore = catsToAssignToAdvertisementHighestPriorityScore;
    }
}
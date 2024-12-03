using KittySaver.Domain.Persons;
// ReSharper disable MemberCanBeMadeStatic.Local
// ReSharper disable MemberCanBeMadeStatic.Global

namespace KittySaver.Domain.Advertisements;

public sealed class AdvertisementService(IAdvertisementRepository advertisementRepository, IPersonRepository personRepository)
{
    public async Task ReplaceCatsOfAdvertisementAsync(
        Guid advertisementId,
        IEnumerable<Guid> catIdsToAssign,
        CancellationToken cancellationToken)
    {
        Advertisement advertisement = await advertisementRepository.GetAdvertisementByIdAsync(advertisementId, cancellationToken);
        Person advertisementOwner = await personRepository.GetPersonByIdAsync(advertisement.PersonId, cancellationToken);
        List<Guid> catIdsToAssignList = catIdsToAssign.ToList();
        
        foreach (Guid catId in advertisementOwner.GetAssignedToConcreteAdvertisementCatIds(advertisement.Id))
        {
            advertisementOwner.UnassignCatFromAdvertisement(catId);
        }

        foreach (Guid catId in catIdsToAssignList)
        {
            advertisementOwner.AssignCatToAdvertisement(advertisement.Id, catId);
        }
        
        SetAdvertisementPriorityScore(advertisementOwner, advertisement, catIdsToAssignList);
    }
    
    public async Task RecalculatePriorityScoreAsync(
        Guid advertisementId,
        CancellationToken cancellationToken)
    {
        Advertisement advertisement = await advertisementRepository.GetAdvertisementByIdAsync(advertisementId, cancellationToken);
        Person advertisementOwner = await personRepository.GetPersonByIdAsync(advertisement.PersonId, cancellationToken);
        
        advertisement.ValidateOwnership(advertisementOwner.Id);
        IEnumerable<Guid> catsAssignedToAdvertisement = advertisementOwner.GetAssignedToConcreteAdvertisementCatIds(advertisement.Id);
        SetAdvertisementPriorityScore(advertisementOwner, advertisement, catsAssignedToAdvertisement);
    }
    
    private static void SetAdvertisementPriorityScore(Person person, Advertisement advertisement, IEnumerable<Guid> catIdsToAssignList)
    {
        double catsToAssignToAdvertisementHighestPriorityScore = person.GetHighestPriorityScoreFromGivenCats(catIdsToAssignList);
        advertisement.PriorityScore = catsToAssignToAdvertisementHighestPriorityScore;
    }
}
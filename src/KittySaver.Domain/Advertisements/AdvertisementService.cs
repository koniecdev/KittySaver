using KittySaver.Domain.Persons;

// ReSharper disable MemberCanBeMadeStatic.Global

namespace KittySaver.Domain.Advertisements;

public sealed class AdvertisementService
{
    public void AssignCatsToAdvertisement(
        Person person,
        Advertisement advertisement,
        IEnumerable<Guid> catIdsToAssign)
    {
        List<Guid> catIdsToAssignList = catIdsToAssign.ToList();

        InvokeCatAssignmentToAdvertisement(person, advertisement, catIdsToAssignList);

        SetAdvertisementPriorityScore(person, advertisement, catIdsToAssignList);
    }
    public void ReplaceCatsOfAdvertisement(
        Person person,
        Advertisement advertisement,
        IEnumerable<Guid> catIdsToAssign)
    {
        List<Guid> catIdsToAssignList = catIdsToAssign.ToList();
        
        foreach (Guid catId in person.GetAssignedToConcreteAdvertisementCatIds(advertisement.Id))
        {
            person.UnassignCatFromAdvertisement(catId);
        }

        InvokeCatAssignmentToAdvertisement(person, advertisement, catIdsToAssignList);

        SetAdvertisementPriorityScore(person, advertisement, catIdsToAssignList);
    }

    private static void InvokeCatAssignmentToAdvertisement(Person person, Advertisement advertisement,
        List<Guid> catIdsToAssignList)
    {
        foreach (Guid catId in catIdsToAssignList)
        {
            person.AssignCatToAdvertisement(advertisement.Id, catId);
        }
    }

    public void RecalculatePriorityScore(Person person, Advertisement advertisement)
    {
        advertisement.ValidateOwnership(person.Id);
        IEnumerable<Guid> catsAssignedToAdvertisement = person.GetAssignedToConcreteAdvertisementCatIds(advertisement.Id);
        SetAdvertisementPriorityScore(person, advertisement, catsAssignedToAdvertisement);
    }

    private static void SetAdvertisementPriorityScore(Person person, Advertisement advertisement, IEnumerable<Guid> catIdsToAssignList)
    {
        double catsToAssignToAdvertisementHighestPriorityScore = person.GetHighestPriorityScoreFromGivenCats(catIdsToAssignList);
        advertisement.PriorityScore = catsToAssignToAdvertisementHighestPriorityScore;
    }
}
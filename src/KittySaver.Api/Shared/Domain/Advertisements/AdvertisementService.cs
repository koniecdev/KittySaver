using KittySaver.Api.Shared.Domain.Persons;

namespace KittySaver.Api.Shared.Domain.Advertisements;

public sealed class AdvertisementService
{
    // ReSharper disable once MemberCanBeMadeStatic.Global - suggestion is purely mechanical.
    public void RecalculatePriorityScore(Advertisement advertisement, Person person)
    {
        advertisement.ValidateOwnership(person.Id);
        
        double catsToAssignToAdvertisementHighestPriorityScore =
            person.GetHighestPriorityScoreFromGivenCats(person.GetAssignedToConcreteAdvertisementCatIds(advertisement.Id));
        
        advertisement.PriorityScore = catsToAssignToAdvertisementHighestPriorityScore;
    }
}
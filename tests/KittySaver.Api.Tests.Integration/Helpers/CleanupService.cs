using System.Net.Http.Json;
using KittySaver.Api.Features.Persons.SharedContracts;

namespace KittySaver.Api.Tests.Integration.Helpers;

public class CleanupHelper
{
    private readonly HttpClient _httpClient;

    public CleanupHelper(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task Cleanup()
    {
        await CleanupPersons();
    }

    private async Task CleanupPersons()
    {
        ICollection<PersonResponse>? persons = await _httpClient.GetFromJsonAsync<ICollection<PersonResponse>>("api/v1/persons");
        if (persons is null)
        {
            return;
        }
        foreach (PersonResponse person in persons)
        {
            await _httpClient.DeleteAsync($"api/v1/persons/{person.Id}");
        }
    }
}